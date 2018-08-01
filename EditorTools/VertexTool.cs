using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;
using Elmanager.Forms;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace Elmanager.EditorTools
{
    internal class VertexTool : ToolBase, IEditorTool
    {
        private Polygon _currentPolygon;
        private int nearestSegmentIndex = -1;
        private Vector _rectangleStart;

        internal VertexTool(LevelEditor editor)
            : base(editor)
        {
        }

        private bool CreatingVertex { get; set; }

        private bool CreatingRectangle { get; set; }

        public void Activate()
        {
            UpdateHelp();
        }

        public void UpdateHelp()
        {
            LevEditor.InfoLabel.Text =
                "Left mouse button: create vertex. Click near a vertex of a polygon to add vertices to it.";
            if (CreatingVertex)
            {
                var lngth = (_currentPolygon.Vertices[_currentPolygon.Count - 1] -
                             _currentPolygon.Vertices[_currentPolygon.Count - 2]).Length;
                LevEditor.InfoLabel.Text += $" Edge length: {lngth:F3}";
            }
        }

        public void ExtraRendering()
        {
            if (CreatingVertex)
                Renderer.DrawLine(_currentPolygon.GetLastVertex(), _currentPolygon.Vertices[0], Color.Red);
            else if (CreatingRectangle)
            {
                AdjustForGrid(CurrentPos);
                Renderer.DrawRectangle(_rectangleStart, CurrentPos, Color.Blue);
            }
            else
            {
                if (Global.AppSettings.LevelEditor.UseHighlight && nearestSegmentIndex >= 0)
                {
                    Renderer.DrawLine(NearestPolygon[nearestSegmentIndex], NearestPolygon[nearestSegmentIndex + 1], Color.Yellow);
                }
            }
        }

        public void InActivate()
        {
            FinishVertexCreation();
        }

        public void KeyDown(KeyEventArgs key)
        {
            if (key.KeyCode != Keys.Space || !CreatingVertex) return;
            _currentPolygon.RemoveLastVertex();
            _currentPolygon.ChangeOrientation();
            _currentPolygon.Add(CurrentPos);
            _currentPolygon.UpdateDecomposition(false);
        }

        public void MouseDown(MouseEventArgs mouseData)
        {
            switch (mouseData.Button)
            {
                case MouseButtons.Left:
                    int nearestIndex = GetNearestVertexIndex(CurrentPos);
                    AdjustForGrid(CurrentPos);
                    if (CreatingRectangle)
                    {
                        CreatingRectangle = false;
                        var rect = Polygon.Rectangle(_rectangleStart, CurrentPos);
                        Lev.Polygons.Add(rect);
                        rect.UpdateDecomposition();
                        LevEditor.Modified = true;
                        return;
                    }
                    if (!CreatingVertex)
                    {
                        if (Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            CreatingRectangle = true;
                            _rectangleStart = CurrentPos;
                        }
                        else
                        {
                            CreatingVertex = true;
                            if (nearestIndex >= -1)
                            {
                                nearestSegmentIndex = NearestPolygon.GetNearestSegmentIndex(CurrentPos);
                                _currentPolygon = NearestPolygon;
                                _currentPolygon.SetBeginPoint(nearestSegmentIndex + 1);
                                _currentPolygon.Add(CurrentPos);
                                ChangeToDefaultCursorIfHand();
                            }
                            else
                                _currentPolygon = new Polygon(CurrentPos, CurrentPos);
                        }
                    }
                    else
                    {
                        _currentPolygon.Add(CurrentPos);
                        if (_currentPolygon.Count == 3)
                        {
                            Lev.Polygons.Add(_currentPolygon);
                            _currentPolygon.UpdateDecomposition(false);
                        }
                    }
                    break;
                case MouseButtons.Right:
                    FinishVertexCreation();
                    break;
            }
        }

        public void MouseMove(Vector p)
        {
            CurrentPos = p;
            if (CreatingVertex)
            {
                AdjustForGrid(CurrentPos);
                _currentPolygon.Vertices[_currentPolygon.Count - 1] = p;
                if (_currentPolygon.Count > 2)
                    _currentPolygon.UpdateDecomposition(false);
                UpdateHelp();
            }
            else if(!CreatingRectangle)
            {
                ResetHighlight();
                int nearestVertex = GetNearestVertexIndex(p);
                if (nearestVertex >= -1)
                {
                    nearestSegmentIndex = NearestPolygon.GetNearestSegmentIndex(p);
                    ChangeCursorToHand();
                }
                else
                {
                    ChangeToDefaultCursorIfHand();
                    nearestSegmentIndex = -1;
                }
            }
        }

        public void MouseOutOfEditor()
        {
            ResetHighlight();
        }

        public void MouseUp(MouseEventArgs mouseData)
        {
        }

        private void FinishVertexCreation()
        {
            if (CreatingVertex)
            {
                CreatingVertex = false;
                LevEditor.InfoLabel.Text =
                    "Left mouse button: create vertex. Click near a vertex of a polygon to add vertices to it.";
                _currentPolygon.RemoveLastVertex();
                if (_currentPolygon.Count > 2)
                {
                    _currentPolygon.UpdateDecomposition();
                    LevEditor.Modified = true;
                }
                else
                    Lev.Polygons.Remove(_currentPolygon);
            }
            else if (CreatingRectangle)
            {
                CreatingRectangle = false;
            }
        }

        public override bool Busy => CreatingVertex || CreatingRectangle;
    }
}