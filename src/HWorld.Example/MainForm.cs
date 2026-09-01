using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using HWorld.Core.World;
using HWorld.WinForms;
using HWorld.WinForms.Helpers;
using HWorld.Console;
using HWorld.WinForms.Helpers.Button;
using HWorld.HAgent;

namespace HWorld.Example
{
    internal sealed class MainForm : Form
    {
        public MainForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 680);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(11, 14, 18);
            ForeColor = Color.FromArgb(235, 239, 244);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = BackColor, ColumnCount = 1, RowCount = 3, Padding = new Padding(28) };
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
            header.PerformOnHelp += delegate { HMessage.ShowInformation(this, "Design World opens the reusable WinForms designer.\r\nRun GDI opens the reusable GDI+ runtime.\r\nRun Console opens the reusable console runtime.\r\nMulti-Actor Lab demonstrates independent actors, collision and actor-specific perception.\r\nDecision Lab demonstrates asynchronous decision scheduling without an LLM.\r\nHAgent Config opens the optional external cognition configuration UI.\r\nRun HAgent starts one live HWorld actor controlled by one persistent HAgent runtime instance.", "HWorld.Example"); };
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

            var buttons = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 7, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(4, 0, 4, 0) };
            for (int i = 0; i < 7; i++) buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857f));
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

            var multiActor = MakeLauncherButton("Multi-Actor Lab");
            multiActor.Click += delegate { OpenMultiActorLab(); };
            buttons.Controls.Add(multiActor, 3, 0);

            var decision = MakeLauncherButton("Decision Lab");
            decision.Click += delegate { OpenDecisionLab(); };
            buttons.Controls.Add(decision, 4, 0);

            var hAgent = MakeLauncherButton("HAgent Config");
            hAgent.Click += delegate { OpenHAgentConfiguration(); };
            buttons.Controls.Add(hAgent, 5, 0);

            var hAgentRun = MakeLauncherButton("Run HAgent");
            hAgentRun.Click += async delegate { await OpenHAgentWorldAsync(); };
            buttons.Controls.Add(hAgentRun, 6, 0);

            var description = new Label { Text = "One core world  →  reusable designer  →  GDI runtime  →  console runtime  →  multi-actor lab  →  decision scheduling  →  external cognition  →  live HAgent actor", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopCenter, Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(143, 154, 167) };
            center.Controls.Add(description, 0, 3);

            var status = new Label { Text = "HWorld.Core + HWorld.WinForms + HWorld.Console + HWorld.Example + optional HWorld.HAgent", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(103, 116, 130) };
            root.Controls.Add(status, 0, 2);
        }

        private static HButton MakeLauncherButton(string text)
        {
            return new HButton {
                Text = text,
                Width = 270,
                Height = 36,
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ButtonLeaveBackGroundColor1 = Color.FromArgb(46, 56, 68),
                ButtonLeaveBackGroundColor2 = Color.FromArgb(28, 35, 44),
                ButtonEnterBackGroundColor1 = Color.FromArgb(75, 91, 109),
                ButtonEnterBackGroundColor2 = Color.FromArgb(47, 57, 69),
                ButtonDownBackGroundColor1 = Color.FromArgb(31, 38, 47),
                ButtonDownBackGroundColor2 = Color.FromArgb(22, 28, 35),
                ButtonLeaveForeColor = Color.White,
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
                new GdiWorldForm(world, world.Actors[0]).Show(this);
            }
            catch (Exception ex) { HMessage.ShowException(this, "The GDI runtime could not be opened.", "HWorld.Example", ex); }
        }

        private void OpenConsole()
        {
            try
            {
                var world = CreateTestWorld();
                var player = world.Actors[0];
                Task.Run(delegate { ConsoleWorldRunner.Run(world, player); });
            }
            catch (Exception ex) { HMessage.ShowException(this, "The console runtime could not be opened.", "HWorld.Example", ex); }
        }

        private void OpenMultiActorLab()
        {
            try { new MultiActorLabForm().Show(this); }
            catch (Exception ex) { HMessage.ShowException(this, "The multi-actor laboratory could not be opened.", "HWorld.Example", ex); }
        }

        private void OpenDecisionLab()
        {
            try { new DecisionSchedulingLabForm().Show(this); }
            catch (Exception ex) { HMessage.ShowException(this, "The decision scheduling laboratory could not be opened.", "HWorld.Example", ex); }
        }

        private void OpenHAgentConfiguration()
        {
            try { HAgentConfiguration.Show(this); }
            catch (Exception ex) { HMessage.ShowException(this, "The HAgent configuration could not be opened.", "HWorld.Example", ex); }
        }

        private async Task OpenHAgentWorldAsync()
        {
            try
            {
                UseWaitCursor = true;
                var form = await HAgentWorldLabForm.CreateAsync();
                UseWaitCursor = false;
                form.Show(this);
            }
            catch (Exception ex)
            {
                UseWaitCursor = false;
                HMessage.ShowException(this, "The HAgent world laboratory could not be opened.", "HWorld.Example", ex);
            }
        }
    }
}
