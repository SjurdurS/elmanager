using System.Drawing;
using System.Windows.Forms;
using Elmanager.Forms;

namespace Elmanager.EditorTools
{
    internal class DrawTool : ToolBase, IEditorTool
    {
        private const double ThresholdAdjustStep = 0.125;
        private Polygon _currentPolygon;
        private bool _drawing;
        private Vector _lastMousePosition;
        private double _mouseTrip;

        internal DrawTool(LevelEditor editor)
            : base(editor)
        {
        }

        private bool Drawing
        {
            get { return _drawing; }
            set
            {
                _drawing = value;
                _Busy = value;
            }
        }

        public void Activate()
        {
            UpdateHelp();
            Renderer.RedrawScene();
        }

        public void UpdateHelp()
        {
            LevEditor.InfoLabel.Text =
                $"Press and hold left mouse button to create vertex. Threshold: {Global.AppSettings.LevelEditor.DrawStep:F2}";
        }

        public void ExtraRendering()
        {
            if (Drawing)
                Renderer.DrawLine(_currentPolygon.GetLastVertex(), _currentPolygon.Vertices[0], Color.Red);
        }

        public void InActivate()
        {
            Drawing = false;
        }

        public void KeyDown(KeyEventArgs key)
        {
            switch (key.KeyCode)
            {
                case Constants.Increase:
                    Global.AppSettings.LevelEditor.DrawStep += ThresholdAdjustStep;
                    break;
                case Constants.Decrease:
                    if (Global.AppSettings.LevelEditor.DrawStep > ThresholdAdjustStep)
                    {
                        Global.AppSettings.LevelEditor.DrawStep -= ThresholdAdjustStep;
                    }
                    break;
            }
            UpdateHelp();
        }

        public void MouseDown(MouseEventArgs mouseData)
        {
            switch (mouseData.Button)
            {
                case MouseButtons.Left:
                    Drawing = true;
                    _currentPolygon = new Polygon();
                    _currentPolygon.Add(CurrentPos);
                    _mouseTrip = 0;
                    _lastMousePosition = CurrentPos;
                    break;
                case MouseButtons.Right:
                    break;
            }
        }

        public void MouseMove(Vector p)
        {
            CurrentPos = p;
            if (!Drawing) return;
            _mouseTrip += (p - _lastMousePosition).Length;
            _lastMousePosition = p;
            var scaledStep = Global.AppSettings.LevelEditor.DrawStep * Renderer.ZoomLevel * 0.1;
            if (_mouseTrip > scaledStep)
            {
                while (!(_mouseTrip < scaledStep))
                    _mouseTrip -= scaledStep;
                _currentPolygon.Add(p);
                if (_currentPolygon.Count == 3)
                    Lev.Polygons.Add(_currentPolygon);
                if (_currentPolygon.Count > 2)
                    _currentPolygon.UpdateDecomposition();
            }
            Renderer.RedrawScene();
        }

        public void MouseOutOfEditor()
        {
        }

        public void MouseUp(MouseEventArgs mouseData)
        {
            if (!Drawing) return;
            Drawing = false;
            if (_currentPolygon.Count > 2)
            {
                _currentPolygon.UpdateDecomposition();
                LevEditor.Modified = true;
            }
            Renderer.RedrawScene();
        }
    }
}