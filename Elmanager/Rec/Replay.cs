using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Elmanager.Application;
using Elmanager.IO;
using Elmanager.Lev;
using Elmanager.Utilities;

namespace Elmanager.Rec;

internal class Replay : ElmaFile
{
    [Description("Finished")] public readonly bool Finished; //Is replay finished

    [Description("Multi")] public readonly bool IsMulti; //Is it single or multi replay

    [Description("Level exists")] public readonly bool LevelExists; //Does the replay's level exist

    [Description("Level")] public string LevelDesc => Level.GetPossiblyInternal(LevelFilename);

    public readonly string LevelFilename; //Filename of the level of the replay

    [Description("Time")] public string TimeStr => Time.ToTimeString();

    public readonly double Time; //Time of the replay

    [Description("Wrong version")] public readonly bool WrongLevelVersion; //Is the version of the level wrong
    internal readonly bool AcrossLevel; //Is the level across level
    private readonly int _internalIndex;
    internal readonly bool IsInternal; //Is it internal or external replay

    internal readonly string LevelPath; //Path of the level file if it is found
    internal Player Player1 => Players[0];
    internal Player Player2 => Players[1];
    internal readonly List<Player> Players = new(2);

    internal Replay(string replayPath)
    {
        Path = replayPath;
        Size = (int) new FileInfo(replayPath).Length;
        DateModified = File.GetLastWriteTime(replayPath);
        using (var stream = File.OpenRead(replayPath))
        {
            var rec = new BinaryReader(stream);
            try
            {
                var frames = rec.ReadInt32();
                var magic = rec.ReadInt32();
                if (magic != 0x83)
                    throw new BadFileException($"Unexpected magic ({magic}) in replay file: {replayPath}");
                IsMulti = rec.ReadInt32() == 1;
                var isFlagtag = rec.ReadInt32() == 1;
                LevId = rec.ReadInt32();
                LevelFilename = rec.ReadNullTerminatedString(12);
                rec.ReadInt32();
                IsInternal = Level.IsInternalLevel(LevelFilename);
                Players.Add(new Player(rec, frames));
                if (IsMulti)
                {
                    frames = rec.ReadInt32();
                    rec.BaseStream.Seek(32, SeekOrigin.Current);
                    Players.Add(new Player(rec, frames));
                }
            }
            catch (EndOfStreamException)
            {
                throw new BadFileException($"Corrupted replay file: {replayPath}");
            }
        }

        if (IsMulti)
        {
            if (!Player1.Finished)
            {
                if (Player1.FakeFinish && Player2.IsLastEventApple)
                {
                    Finished = true;
                    Time = Player1.Time;
                }
                else if (Player2.FakeFinish && Player1.IsLastEventApple)
                {
                    Finished = true;
                    Time = Player2.Time;
                }
                else if (Player2.Finished)
                {
                    Finished = true;
                    Time = Player2.Time;
                }
                else //In this case, neither of players finished
                    Time = Math.Max(Player1.Time, Player2.Time);
            }
            else
            {
                Time = Player1.Time;
                Finished = true;
            }
        }
        else
        {
            Finished = Player1.Finished;
            Time = Player1.Time;
        }

        if (IsInternal)
        {
            LevelExists = true;
            WrongLevelVersion = false;
            AcrossLevel = false;
            _internalIndex = int.Parse(LevelFilename.Substring(6, 2));
        }
        else
        {
            _internalIndex = -1;
            foreach (var levelFile in Global.GetLevelFiles())
            {
                if (System.IO.Path.GetFileName(levelFile).CompareWith(LevelFilename))
                {
                    LevelExists = true;
                    LevelPath = levelFile;
                    var fileStream = File.OpenRead(levelFile);
                    var levelStream = new BinaryReader(fileStream);
                    fileStream.Seek(3, SeekOrigin.Begin);
                    //Check also the version of the level
                    if (fileStream.Length > 0)
                    {
                        if (levelStream.ReadByte() == 49)
                            //If Level(3) = 49, it is Elma lev, otherwise (when 48) Across lev
                        {
                            AcrossLevel = false;
                            fileStream.Seek(7, SeekOrigin.Begin);
                        }
                        else
                        {
                            AcrossLevel = true;
                            fileStream.Seek(5, SeekOrigin.Begin);
                        }

                        if (levelStream.ReadInt32() != LevId)
                        {
                            WrongLevelVersion = true;
                            break;
                        }
                    }
                    else
                        LevelExists = false;

                    levelStream.Close();
                    break;
                }
            }
        }
    }

    public int LevId { get; }

    internal Level GetLevel()
    {
        if (LevelExists)
        {
            if (IsInternal)
            {
                while (Global.Internals == null)
                {
                }

                return Global.Internals[_internalIndex - 1];
            }

            return Level.FromPath(LevelPath);
        }

        throw new FileNotFoundException("The level file does not exist!", LevelFilename);
    }
}