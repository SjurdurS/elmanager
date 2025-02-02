﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using Elmanager.Lev;
using Elmanager.Rendering.Camera;
using Elmanager.Rendering;
using OpenTK.GLControl;
using Button = System.Windows.Forms.Button;

namespace Elmanager.LevelEditor.ShapeGallery
{
    internal partial class LevelControlFormTest : Form
    {
        private LevelControl levelControl1;
        private LevelControl levelControl2;
        private Button testButton;

        public LevelControlFormTest(GLControl sharedControl, ElmaRenderer renderer, Level level, ElmaCamera camera, SceneSettings sceneSettings, RenderingSettings renderingSettings)
        {

            // Remove InitializeComponent if you're not using the designer
            // InitializeComponent();

            // Create LevelControls with shared OpenGL context
            levelControl1 = new LevelControl(sharedControl, renderer, level, camera, sceneSettings, renderingSettings)
            {
                Width = 200,
                Height = 200,
                Dock = DockStyle.Fill
            };
            levelControl1.Visible = true;
            levelControl2 = new LevelControl(sharedControl, renderer, level, camera, sceneSettings, renderingSettings)
            {
                Width = 200,
                Height = 200,
                Dock = DockStyle.Fill
            };
            levelControl2.Visible = true;

            // Create and configure the test button
            testButton = new Button
            {
                Text = "Test Button",
                Width = 100,
                Height = 50,
                Dock = DockStyle.Fill
            };

            // Arrange controls side by side using TableLayoutPanel
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            
            // Add controls to the layout
            panel.Controls.Add(levelControl1, 0, 0);
            panel.Controls.Add(levelControl2, 1, 0);
            panel.Controls.Add(testButton, 2, 0);
            Controls.Add(panel);

            panel.Visible = true;
        }
    }
}
