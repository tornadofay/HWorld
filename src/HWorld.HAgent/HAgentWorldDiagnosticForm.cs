using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAgent.Abstractions;
using HAgent.Models;
using HAgent.Providers.OpenAICompatible;
using HAgent.Runtime;
using HAgent.Storage.File;
using HWorld.Core.Geometry;
using HWorld.Core.World;
using HWorld.WinForms;
using HWorld.WinForms.Helpers.Button;
using HWorld.WinForms.Rendering;

namespace HWorld.HAgent
{
    public sealed class HAgentWorldDiagnosticForm : Form
    {
        private readonly HAgentClient _client;
        private readonly AgentRuntimeInstance _runtimeInstance;
        private readonly HAgentDecisionProvider _provider;
        private readonly World _world;
        private readonly WorldActor _actor;
        private readonly WorldActorDecisionScheduler _scheduler;
        private readonly WorldGeometryCamera _camera;
        private readonly List<WorldGeometryObservation> _observations = new List<WorldGeometryObservation>(32);
        private readonly System.Windows.Forms.Timer _timer;
        private readonly GdiWorldCanvas _canvas;
        private readonly RichTextBox _trace;
        private readonly Label _state;
        private readonly Label _position;
        private bool _closing;
        private bool _running = true;
        private WorldPoint _lastPosition;
        private int _lastPendingCount;
        private bool _lastActionActive;

        private HAgentWorldDiagnosticForm(HAgentClient client, AgentRuntimeInstance runtimeInstance, string agentName)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _runtimeInstance = runtimeInstance ?? throw new ArgumentNullException(nameof(runtimeInstance));
            _world = BuildWorld();
            _actor = _world.Actors[0];
            _actor.Name = agentName;
            _lastPosition = _actor.Position;

            _provider = new HAgentDecisionProvider(_client, _runtimeInstance);
            _provider.ExecutionCompleted += OnExecutionCompleted;
            _scheduler = new WorldActorDecisionScheduler(_world, 1);
            _scheduler.DecisionLifecycle += OnDecisionLifecycle;
            _scheduler.ObservationFactory = BuildObservation;
            _scheduler.Register(_actor, _provider, new WorldActorDecisionOptions
            {
                DecisionCadenceSeconds = 0.25,
                DecisionTimeout = TimeSpan.FromSeconds(20),
                SchedulingMode = WorldDecisionSchedulingMode.Asynchronous,
                StartImmediately = true
            });

            _camera = new WorldGeometryCamera(48, 100)
            {
                IncludeActors = true,
                IncludeSolidState = true
            };

            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(11, 14, 18);
            ForeColor = Color.FromArgb(235, 239, 244);
            KeyPreview = true;

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(10), BackColor = BackColor };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            var header = new Header
            {
                Dock = DockStyle.Fill,
                Title = "HWorld + HAgent Diagnostic",
                Subtitle = "live cognition → action handoff → world execution",
                AllowMove = false,
                AllowMinimize = true,
                AllowClose = true,
                AllowHelp = true
            };
            header.PerformOnClose += delegate { Close(); };
            header.PerformOnHelp += delegate
            {
                HMessage.ShowInformation(this,
                    "This diagnostic shows the complete handoff.\r\n\r\n"
                    + "1. HAgent receives the world observation.\r\n"
                    + "2. HAgent returns a structured decision.\r\n"
                    + "3. HWorld queues and starts the action.\r\n"
                    + "4. HWorld updates the actor position.\r\n"
                    + "5. The next observation is generated.\r\n\r\n"
                    + "HWorld remains authoritative over movement and collision.",
                    "HAgent Diagnostic");
            };
            root.Controls.Add(header, 0, 0);

            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = BackColor };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            root.Controls.Add(content, 0, 1);

            var canvasPanel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 8, 8, 0), BackColor = BackColor };
            _canvas = new GdiWorldCanvas { Dock = DockStyle.Fill, World = _world, Player = _actor, Mode = CanvasMode.Play };
            canvasPanel.Controls.Add(_canvas);
            content.Controls.Add(canvasPanel, 0, 0);

            var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(10, 8, 0, 0), BackColor = Color.FromArgb(20, 25, 32) };
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            content.Controls.Add(right, 1, 0);

            _state = new Label { Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 10f, FontStyle.Bold), Text = "STATE\r\nstarting…" };
            _position = new Label { Dock = DockStyle.Fill, ForeColor = Color.FromArgb(205, 215, 225), Font = new Font("Segoe UI", 9f), Text = "POSITION\r\n—" };
            right.Controls.Add(_state, 0, 0);
            right.Controls.Add(_position, 0, 1);

            var controls = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            var pause = MakeButton("Pause");
            pause.Click += delegate { _running = !_running; pause.Text = _running ? "Pause" : "Resume"; Trace("SYSTEM: running=" + _running); };
            controls.Controls.Add(pause, 0, 0);
            var force = MakeButton("Test Move");
            force.Click += delegate
            {
                try
                {
                    _world.EnqueueMove(_actor.Id, 1, 0, 0.5);
                    Trace("TEST ACTION QUEUED: MOVE RIGHT for 0.5s");
                }
                catch (Exception ex) { Trace("TEST ACTION FAILED: " + ex.Message); }
            };
            controls.Controls.Add(force, 1, 0);
            right.Controls.Add(controls, 0, 2);

            var clear = MakeButton("Clear Trace");
            clear.Click += delegate { _trace.Clear(); };
            right.Controls.Add(clear, 0, 3);

            _trace = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(14, 18, 24),
                ForeColor = Color.FromArgb(220, 225, 232),
                Font = new Font("Consolas", 8.5f),
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            right.Controls.Add(_trace, 0, 4);

            _timer = new System.Windows.Forms.Timer { Interval = 33 };
            _timer.Tick += OnTick;
            FormClosing += OnFormClosing;
            _canvas.CenterOnPlayer();
            _timer.Start();

            Trace("SYSTEM: runtime=" + _runtimeInstance.InstanceId + " actor=" + _actor.Name);
            Trace("SYSTEM: scheduler started; cadence=0.25s; timeout=20s");
        }

        public static async Task<HAgentWorldDiagnosticForm> CreateAsync()
        {
            var applicationName = Process.GetCurrentProcess().ProcessName;
            if (string.IsNullOrWhiteSpace(applicationName)) applicationName = "HWorld";
            var options = new HAgentStorageOptions { ApplicationName = applicationName, RootPath = AppContext.BaseDirectory };
            options.Validate();
            var basePath = options.GetEffectiveRootPath();
            Directory.CreateDirectory(basePath);

            IAiStore store = new FileAiStore(Path.Combine(basePath, "configuration", "settings.json"));
            ISecretStore secrets = new ProtectedDataSecretStore(Path.Combine(basePath, "secrets"));
            var adapters = new IAiProviderAdapter[] { new OpenAICompatibleProviderAdapter() };
            var agents = await store.GetAgentsAsync(CancellationToken.None).ConfigureAwait(false);
            AiAgent selected = null;
            for (int i = 0; i < agents.Count; i++)
            {
                if (agents[i].Enabled) { selected = agents[i]; break; }
            }
            if (selected == null) throw new InvalidOperationException("No enabled HAgent is configured. Open HAgent Config first.");
            var client = new HAgentClient(store, secrets, adapters);
            var instance = AgentRuntimeInstance.Create(selected, AgentRuntimeScope.Session);
            return new HAgentWorldDiagnosticForm(client, instance, selected.Name);
        }

        private World BuildWorld()
        {
            var world = new World(180, 100);
            AddBoundary(world, 2, 2, 176, 3);
            AddBoundary(world, 2, 95, 176, 3);
            AddBoundary(world, 2, 2, 3, 96);
            AddBoundary(world, 175, 2, 3, 96);
            world.AddActor(new WorldPoint(24, 50), 5, 5, 11);
            world.AddItem(new WorldPoint(70, 20), 18, 6, true).Name = "Wall A";
            world.AddItem(new WorldPoint(70, 74), 18, 6, true).Name = "Wall B";
            world.AddItem(new WorldPoint(110, 38), 8, 26, true).Name = "Wall C";
            world.AddItem(new WorldPoint(140, 18), 10, 10, false).Name = "Beacon";
            world.AddItem(new WorldPoint(142, 70), 8, 8, false).Name = "Marker";
            return world;
        }

        private string BuildObservation(WorldActor actor)
        {
            _camera.Observe(_world, actor, _observations);
            var lines = new List<string> { "Visible geometry entities:" };
            for (int i = 0; i < _observations.Count; i++)
            {
                var o = _observations[i];
                lines.Add(string.Format(CultureInfo.InvariantCulture,
                    "- id={0}; dx={1:0.0}; dy={2:0.0}; distance={3:0.0}; bearing={4:0.0}deg; size={5:0.0}x{6:0.0}; solid={7}",
                    o.EntityId.ToString("N"), o.RelativeX, o.RelativeY, o.Distance, o.BearingDegrees, o.Width, o.Height, o.Solid));
            }
            if (_observations.Count == 0) lines.Add("- none");
            lines.Add("Allowed actions: move(directionX,directionY,durationSeconds), turn(angleDegrees), wait(durationSeconds).");
            lines.Add("Choose freely among four cardinal movement directions: UP (0,-1), DOWN (0,1), LEFT (-1,0), or RIGHT (1,0). Explore safely and avoid solid obstacles.");
            return string.Join(Environment.NewLine, lines);
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_running || _closing) return;
            var before = _actor.Position;
            var pendingBefore = _actor.PendingActionCount;
            var activeBefore = _actor.IsActionActive;
            const double dt = 1.0 / 30.0;
            _world.Update(dt);
            var afterWorld = _actor.Position;
            if (Math.Abs(afterWorld.X - before.X) > 0.000001 || Math.Abs(afterWorld.Y - before.Y) > 0.000001)
            {
                Trace(string.Format(CultureInfo.InvariantCulture,
                    "WORLD ACTION EXECUTED: ({0:0.00},{1:0.00}) -> ({2:0.00},{3:0.00}) delta=({4:0.00},{5:0.00}) pending={6} active={7}",
                    before.X, before.Y, afterWorld.X, afterWorld.Y, afterWorld.X - before.X, afterWorld.Y - before.Y,
                    pendingBefore, activeBefore));
            }
            if (pendingBefore != _actor.PendingActionCount || activeBefore != _actor.IsActionActive)
            {
                Trace(string.Format(CultureInfo.InvariantCulture,
                    "ACTION STATE: pending {0}->{1}, active {2}->{3}", pendingBefore, _actor.PendingActionCount, activeBefore, _actor.IsActionActive));
            }
            _scheduler.Update(_world.SimulationTime);
            if (_actor.Position.X != _lastPosition.X || _actor.Position.Y != _lastPosition.Y)
            {
                _lastPosition = _actor.Position;
                _canvas.Invalidate();
            }
            if (_lastPendingCount != _actor.PendingActionCount || _lastActionActive != _actor.IsActionActive)
            {
                _lastPendingCount = _actor.PendingActionCount;
                _lastActionActive = _actor.IsActionActive;
            }
            _position.Text = string.Format(CultureInfo.InvariantCulture,
                "POSITION\r\n{0:0.00}, {1:0.00}\r\nvelocity scale: {2:0.00}",
                _actor.Position.X, _actor.Position.Y, _actor.Speed);
            _state.Text = string.Format(CultureInfo.InvariantCulture,
                "STATE\r\nrequests {0}/{1}\r\npending {2}\r\naction active {3}\r\nworld {4:0.00}s",
                _scheduler.ActiveRequestCount, _scheduler.MaxConcurrentRequests, _actor.PendingActionCount, _actor.IsActionActive, _world.SimulationTime);
        }

        private void OnDecisionLifecycle(object sender, WorldActorDecisionEvent e)
        {
            if (_closing || IsDisposed || !IsHandleCreated) return;
            try
            {
                BeginInvoke((Action)delegate
                {
                    if (_closing || IsDisposed) return;
                    Trace(string.Format(CultureInfo.InvariantCulture,
                        "DECISION {0}: request={1} elapsed={2:0.000}s error={3}",
                        e.Outcome, e.RequestId, e.ElapsedSeconds, e.Error ?? string.Empty));
                });
            }
            catch (InvalidOperationException) { }
        }

        private void OnExecutionCompleted(AgentExecution execution)
        {
            if (_closing || IsDisposed || execution == null) return;
            var builder = new StringBuilder();
            builder.AppendLine("LLM EXECUTION " + execution.Id + " STATE=" + execution.State);
            builder.AppendLine("HOST=" + execution.HostCorrelationId + " RUNTIME=" + execution.RuntimeInstanceId);
            if (execution.Messages != null)
                for (int i = 0; i < execution.Messages.Count; i++)
                {
                    var m = execution.Messages[i];
                    builder.AppendLine((m.Role ?? "message").ToUpperInvariant() + ":");
                    builder.AppendLine(m.Content ?? string.Empty);
                }
            if (execution.Response != null)
            {
                builder.AppendLine("ASSISTANT:");
                builder.AppendLine(execution.Response.Text ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(execution.Response.StructuredOutputJson))
                {
                    builder.AppendLine("STRUCTURED OUTPUT:");
                    builder.AppendLine(execution.Response.StructuredOutputJson);
                }
            }
            Trace(builder.ToString());
        }

        private void Trace(string text)
        {
            if (_trace == null || _trace.IsDisposed) return;
            if (_trace.InvokeRequired)
            {
                try { _trace.BeginInvoke((Action)delegate { Trace(text); }); } catch (InvalidOperationException) { }
                return;
            }
            _trace.AppendText("\r\n" + text + "\r\n");
            _trace.SelectionStart = _trace.TextLength;
            _trace.ScrollToCaret();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            _closing = true;
            _timer.Stop();
            _scheduler.Dispose();
            _provider.ExecutionCompleted -= OnExecutionCompleted;
            _provider.Dispose();
        }

        private static HButton MakeButton(string text)
        {
            return new HButton { Text = text, Dock = DockStyle.Fill, Margin = new Padding(3) };
        }

        private static void AddBoundary(World world, double x, double y, double width, double height)
        {
            var wall = world.AddItem(new WorldPoint(x, y), width, height, true);
            wall.Kind = "wall";
            wall.Name = "Boundary";
        }
    }
}
