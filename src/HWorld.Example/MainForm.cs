using System;
using System.Drawing;
using System.Windows.Forms;
using HWorld.Core.World;
using HWorld.WinForms;
using HWorld.WinForms.Helpers;
using HWorld.Console;

namespace HWorld.Example
{
    internal sealed class MainForm : Form
    {
        public MainForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(11, 14, 18);
            ForeColor = Color.FromArgb(235, 239, 244);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(28)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54f));
            Controls.Add(root);

            var header = new Header
            {
                Dock = DockStyle.Fill,
                Title = "HWorld.Example",
                Subtitle = "Library test harness  •  run the same world through different front ends",
                AllowMove = false,
                AllowMinimize = true,
                AllowClose = true,
                AllowHelp = true
            };
            header.PerformOnClose += delegate { Close(); };
            header.PerformOnHelp += delegate { HMessage.ShowInformation(this, "Design World opens the reusable WinForms designer.\r\nRun GDI opens the reusable GDI+ runtime.\r\nRun Console opens the reusable console runtime.", "HWorld.Example"); };
            root.Controls.Add(header, 0, 0);

            var center = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, BackColor = Color.Transparent };
            center.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            center.RowStyles.Add(new RowStyle(SizeType.Absolute, 82f));
            center.RowStyles.Add(new RowStyle(SizeType.Absolute, 18f));
            center.RowStyles.Add(new RowStyle(SizeType.Absolute, 82f));
            center.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            root.Controls.Add(center, 0, 1);

            var title = new Label { Text = "HWorld test laboratory", Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomCenter, Font = new Font("Segoe UI", 24f, FontStyle.Bold), ForeColor = Color.FromArgb(246, 248, 250) };
            center.Controls.Add(title, 0, 0);

            var buttons = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(60, 0, 60, 0) };
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
            center.Controls.Add(buttons, 0, 1);

            var designer = MakeLauncherButton("Design World");
            designer.Click += delegate { OpenDesigner(); };
            buttons.Controls.Add(designer, 0, 0);
            var gdi = MakeLauncherButton("Run GDI");
            gdi.Click += delegate { OpenGdi(); };
            buttons.Controls.Add(gdi, 1, 0);
            var console = MakeLauncherButton("Run Console");
            console.Click += delegate { OpenConsole(); };
            buttons.Controls.Add(console, 2, 0);

            var description = new Label { Text = "One core world  →  reusable designer  →  GDI runtime  →  console runtime", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopCenter, Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(143, 154, 167) };
            center.Controls.Add(description, 0, 3);

            var status = new Label { Text = "HWorld.Core + HWorld.WinForms + HWorld.Console", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(103, 116, 130) };
            root.Controls.Add(status, 0, 2);
        }

        private static LauncherButton MakeLauncherButton(string text)
        {
            return new LauncherButton
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ButtonLeaveBackGroundColor1 = Color.FromArgb(46, 56, 68),
                ButtonLeaveBackGroundColor2 = Color.FromArgb(28, 35, 44),
                ButtonEnterBackGroundColor1 = Color.FromArgb(75, 91, 109),
                ButtonEnterBackGroundColor2 = Color.FromArgb(47, 57, 69),
                ButtonDownBackGroundColor1 = Color.FromArgb(31, 38, 47),
                ButtonDownBackGroundColor2 = Color.FromArgb(22, 28, 35),
                ButtonLeaveForeColor = Color.FromArgb(235, 240, 245),
                ButtonEnterForeColor = Color.White,
                ButtonDownForeColor = Color.White
            };
        }

        private static World CreateTestWorld()
        {
            var scenario = WorldScenarioFactory.CreateHandBuilt();
            return scenario.World;
        }

        private void OpenDesigner()
        {
            try { new WorldDesignerForm(CreateTestWorld()).ShowDialog(this); }
            catch (Exception ex) { HMessage.ShowException(this, "The world designer could not be opened.", "HWorld.Example", ex); }
        }

        private void OpenGdi()
        {
            try
            {
                var world = CreateTestWorld();
                var form = new GdiWorldForm(world, world.Actors[0]);
                form.Show(this);
            }
            catch (Exception ex) { HMessage.ShowException(this, "The GDI runtime could not be opened.", "HWorld.Example", ex); }
        }

        private void OpenConsole()
        {
            try
            {
                var world = CreateTestWorld();
                var player = world.Actors[0];
                System.Threading.Tasks.Task.Run(delegate { ConsoleWorldRunner.Run(world, player); });
            }
            catch (Exception ex) { HMessage.ShowException(this, "The console runtime could not be opened.", "HWorld.Example", ex); }
        }
    }
}
