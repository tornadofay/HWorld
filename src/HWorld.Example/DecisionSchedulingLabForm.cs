using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HWorld.Core.Geometry;
using HWorld.Core.World;
using HWorld.WinForms.Helpers;
using HWorld.WinForms.Helpers.Button;

namespace HWorld.Example
{
    internal sealed class DecisionSchedulingLabForm : Form
    {
        private readonly World _world;
        private readonly WorldActor _fast;
        private readonly WorldActor _slow;
        private readonly WorldActorDecisionScheduler _scheduler;
        private readonly Timer _timer;
        private readonly Label _status;
        private readonly Label _fastState;
        private readonly Label _slowState;
        private readonly MultiActorOverview _worldView;
        private bool _running = true;

        public DecisionSchedulingLabForm()
        {
            _world = new World(160, 90);
            AddBoundary(_world, 2, 2, 156, 3);
            AddBoundary(_world, 2, 85, 156, 3);
            AddBoundary(_world, 2, 2, 3, 86);
            AddBoundary(_world, 155, 2, 3, 86);

            _fast = _world.AddActor(new WorldPoint(28, 24), width: 5, height: 5, speed: 10);
            _fast.Name = "Fast decision";
            _slow = _world.AddActor(new WorldPoint(28, 62), width: 5, height: 5, speed: 10);
            _slow.Name = "Slow decision";

            _scheduler = new WorldActorDecisionScheduler(_world, 2);
            _scheduler.DecisionLifecycle += OnDecisionLifecycle;
            _scheduler.Register(_fast, new TimedMoveProvider(TimeSpan.FromMilliseconds(100)), new WorldActorDecisionOptions
            {
                DecisionCadenceSeconds = 0.8,
                DecisionTimeout = TimeSpan.FromSeconds(2),
                SchedulingMode = WorldDecisionSchedulingMode.Asynchronous,
                StartImmediately = true
            });
            _scheduler.Register(_slow, new TimedMoveProvider(TimeSpan.FromMilliseconds(900)), new WorldActorDecisionOptions
            {
                DecisionCadenceSeconds = 0.8,
                DecisionTimeout = TimeSpan.FromSeconds(2),
                SchedulingMode = WorldDecisionSchedulingMode.Asynchronous,
                StartImmediately = true
            });

            Text = "HWorld Decision Scheduling Laboratory";
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(11, 14, 18);
            ForeColor = Color.FromArgb(235, 239, 244);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(10), BackColor = BackColor };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(root);

            var header = new Header
            {
                Dock = DockStyle.Fill,
                Title = "HWorld Decision Scheduling Laboratory",
                Subtitle = "simulation time continues while actors receive decisions at different real-world latencies",
                AllowMove = false,
                AllowMinimize = true,
                AllowClose = true,
                AllowHelp = true
            };
            header.PerformOnClose += delegate { Close(); };
            header.PerformOnHelp += delegate
            {
                HMessage.ShowInformation(this,
                    "Fast decision: 100 ms provider latency.\r\nSlow decision: 900 ms provider latency.\r\n\r\nThe world keeps advancing at 30 Hz. Decision results are applied only on the simulation thread, and cancelled or timed-out requests cannot inject late actions.",
                    "Decision Scheduling");
            };
            root.Controls.Add(header, 0, 0);

            var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = BackColor };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72f));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28f));
            root.Controls.Add(body, 0, 1);

            _worldView = new MultiActorOverview { Dock = DockStyle.Fill, World = _world, ActorA = _fast, ActorB = _slow, Margin = new Padding(0, 8, 8, 0) };
            body.Controls.Add(_worldView, 0, 0);

            var side = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 8, BackColor = Color.FromArgb(20, 25, 32), Padding = new Padding(14) };
            side.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            side.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            side.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            side.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            side.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            side.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            side.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            side.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            body.Controls.Add(side, 1, 0);

            side.Controls.Add(new Label { Text = "SCHEDULER", Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 11f, FontStyle.Bold) }, 0, 0);
            _fastState = MakeStateLabel();
            _slowState = MakeStateLabel();
            side.Controls.Add(_fastState, 0, 1);
            side.Controls.Add(_slowState, 0, 2);

            var pause = new HButton { Text = "Pause", Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 4) };
            pause.Click += delegate { _running = !_running; pause.Text = _running ? "Pause" : "Resume"; };
            side.Controls.Add(pause, 0, 3);

            var cancel = new HButton { Text = "Cancel decisions", Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 4) };
            cancel.Click += delegate { _scheduler.CancelAll(); };
            side.Controls.Add(cancel, 0, 4);

            _status = new Label { Dock = DockStyle.Fill, ForeColor = Color.FromArgb(190, 200, 210), Font = new Font("Segoe UI", 8.5f), TextAlign = ContentAlignment.MiddleLeft };
            side.Controls.Add(_status, 0, 5);
            side.Controls.Add(new Label { Dock = DockStyle.Fill, ForeColor = Color.FromArgb(150, 160, 172), Font = new Font("Segoe UI", 8.2f), Text = "Real latency is measured independently of simulation time.\r\nDecision providers never mutate the world directly.\r\nThis lab does not use HAgent." }, 0, 6);
            side.Controls.Add(new Label { Dock = DockStyle.Fill, ForeColor = Color.FromArgb(120, 132, 145), Font = new Font("Segoe UI", 8f), Text = "Phase 4", TextAlign = ContentAlignment.MiddleLeft }, 0, 7);

            _timer = new Timer { Interval = 33 };
            _timer.Tick += OnTick;
            FormClosing += delegate { _timer.Stop(); _scheduler.Dispose(); };
            _timer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_running) return;
            const double dt = 1.0 / 30.0;
            _world.Update(dt);
            _scheduler.Update(_world.SimulationTime);
            _fastState.Text = FormatState(_fast, "FAST");
            _slowState.Text = FormatState(_slow, "SLOW");
            _status.Text = string.Format("World time {0:0.00}s\r\nActive decisions {1}/{2}", _world.SimulationTime, _scheduler.ActiveRequestCount, _scheduler.MaxConcurrentRequests);
            _worldView.Invalidate();
        }

        private void OnDecisionLifecycle(object sender, WorldActorDecisionEvent e)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke((Action)delegate
            {
                if (IsDisposed) return;
                _status.Text = string.Format("World {0:0.00}s • {1} • {2} • latency {3:0.000}s", _world.SimulationTime, e.Outcome, e.ActorId == _fast.Id ? "FAST" : "SLOW", e.ElapsedSeconds);
            });
        }

        private static string FormatState(WorldActor actor, string label)
        {
            return string.Format("{0}\r\nposition {1:0.0}, {2:0.0}\r\naction {3}", label, actor.Position.X, actor.Position.Y, actor.IsActionActive ? "executing" : "idle");
        }

        private static Label MakeStateLabel()
        {
            return new Label { Dock = DockStyle.Fill, ForeColor = Color.FromArgb(205, 215, 225), Font = new Font("Segoe UI", 8.5f), BackColor = Color.Transparent };
        }

        private static void AddBoundary(World world, double x, double y, double width, double height)
        {
            var wall = world.AddItem(new WorldPoint(x, y), width, height, true);
            wall.Kind = "wall";
            wall.Name = "Boundary";
        }
    }

    internal sealed class TimedMoveProvider : IWorldActorDecisionProvider
    {
        private readonly TimeSpan _delay;

        public TimedMoveProvider(TimeSpan delay)
        {
            _delay = delay;
        }

        public async Task<WorldActorAction> DecideAsync(WorldActorDecisionContext context, CancellationToken cancellationToken)
        {
            await Task.Delay(_delay, cancellationToken).ConfigureAwait(false);
            var right = context.Position.X < 115;
            return new WorldActorAction(WorldActorActionKind.Move, right ? 1 : -1, 0, 0.6);
        }
    }
}
