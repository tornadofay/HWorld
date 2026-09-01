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

namespace HWorld.HAgent
{
    public sealed class HAgentWorldContinuousAgentLabForm : Form
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
        private readonly CheckBox _continuous;
        private bool _continuousEnabled = true;
        private bool _simulationRunning = true;
        private bool _closing;
        private bool _registered;
        private double _lastX;
        private double _lastY;

        private HAgentWorldContinuousAgentLabForm(HAgentClient client, AgentRuntimeInstance runtimeInstance, string agentName)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _runtimeInstance = runtimeInstance ?? throw new ArgumentNullException(nameof(runtimeInstance));
            _world = BuildWorld();
            _actor = _world.Actors[0];
            _actor.Name = agentName;
            _camera = new WorldGeometryCamera(48, 100) { IncludeActors = true, IncludeSolidState = true };
            _provider = new HAgentDecisionProvider(_client, _runtimeInstance);
            _provider.ExecutionCompleted += OnExecutionCompleted;
            _scheduler = new WorldActorDecisionScheduler(_world, 1);
            _scheduler.DecisionLifecycle += OnDecisionLifecycle;
            _scheduler.ObservationFactory = BuildObservation;

            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(11, 14, 18);
            ForeColor = Color.FromArgb(235, 239, 244);
            KeyPreview = true;
            Text = "HWorld + HAgent Continuous Agent Lab";

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(10), BackColor = BackColor };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 25, 32) };
            header.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "HWorld + HAgent Continuous Agent Lab",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White
            });
            root.Controls.Add(header, 0, 0);

            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = BackColor };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            root.Controls.Add(content, 0, 1);

            var viewport = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 8, 8, 0), BackColor = BackColor };
            _canvas = new GdiWorldCanvas { Dock = DockStyle.Fill, World = _world, Player = _actor, Mode = CanvasMode.Play };
            viewport.Controls.Add(_canvas);
            content.Controls.Add(viewport, 0, 0);

            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(12, 8, 12, 8),
                BackColor = Color.FromArgb(20, 25, 32)
            };
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            content.Controls.Add(right, 1, 0);

            _state = MakeInfoLabel("STATE\r\nstarting…", true);
            _position = MakeInfoLabel("POSITION\r\n—", false);
            right.Controls.Add(_state, 0, 0);
            right.Controls.Add(_position, 0, 1);

            _continuous = new CheckBox
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                Checked = true,
                Text = "Continuous Movement / Re-decide",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Padding = new Padding(2, 0, 0, 0)
            };
            _continuous.CheckedChanged += delegate { SetContinuousMovement(_continuous.Checked); };
            right.Controls.Add(_continuous, 0, 2);

            var pause = MakeButton("Pause Simulation");
            pause.Click += delegate
            {
                _simulationRunning = !_simulationRunning;
                pause.Text = _simulationRunning ? "Pause Simulation" : "Resume Simulation";
                Trace("SYSTEM: simulation " + (_simulationRunning ? "resumed" : "paused"));
            };
            right.Controls.Add(pause, 0, 3);

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

            FormClosing += OnFormClosing;
            KeyDown += delegate(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Escape) Close(); };
            _timer = new System.Windows.Forms.Timer { Interval = 33 };
            _timer.Tick += OnTick;
            _lastX = _actor.Position.X;
            _lastY = _actor.Position.Y;
            _timer.Start();
            _canvas.CenterOnPlayer();
            RegisterScheduler();

            Trace("SYSTEM: runtime=" + _runtimeInstance.InstanceId + " actor=" + _actor.Name);
            Trace("SYSTEM: continuous movement=" + _continuousEnabled);
            Trace("SYSTEM: scheduler cadence=0.25s timeout=20s");
        }

        public static async Task<HAgentWorldContinuousAgentLabForm> CreateAsync()
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
            var agents = await store.GetAgentsAsync(CancellationToken.None);
            AiAgent selected = null;
            for (int i = 0; i < agents.Count; i++) if (agents[i].Enabled) { selected = agents[i]; break; }
            if (selected == null) throw new InvalidOperationException("No enabled HAgent is configured. Open HAgent Config first.");
            var client = new HAgentClient(store, secrets, adapters);
            var instance = AgentRuntimeInstance.Create(selected, AgentRuntimeScope.Session);
            return new HAgentWorldContinuousAgentLabForm(client, instance, selected.Name);
        }

        private void RegisterScheduler()
        {
            if (_registered || !_continuousEnabled || _closing) return;
            _scheduler.Register(_actor, _provider, new WorldActorDecisionOptions
            {
                DecisionCadenceSeconds = 0.25,
                DecisionTimeout = TimeSpan.FromSeconds(20),
                SchedulingMode = WorldDecisionSchedulingMode.Asynchronous,
                StartImmediately = true
            });
            _registered = true;
            Trace("SYSTEM: decision scheduler registered");
        }

        private void SetContinuousMovement(bool enabled)
        {
            if (_continuousEnabled == enabled) return;
            _continuousEnabled = enabled;
            if (!enabled)
            {
                if (_registered)
                {
                    _scheduler.Unregister(_actor.Id);
                    _registered = false;
                }
                Trace("SYSTEM: continuous movement OFF; current action may finish, no further HAgent decisions will be requested.");
            }
            else
            {
                RegisterScheduler();
                Trace("SYSTEM: continuous movement ON; HAgent will continue deciding whenever the actor becomes free.");
            }
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
                lines.Add(string.Format(CultureInfo.InvariantCulture, "- id={0}; dx={1:0.0}; dy={2:0.0}; distance={3:0.0}; bearing={4:0.0}deg; size={5:0.0}x{6:0.0}; solid={7}", o.EntityId.ToString("N"), o.RelativeX, o.RelativeY, o.Distance, o.BearingDegrees, o.Width, o.Height, o.Solid));
            }
            if (_observations.Count == 0) lines.Add("- none");
            lines.Add("Allowed actions: move(directionX,directionY,durationSeconds), turn(angleDegrees), wait(durationSeconds).");
            lines.Add("Choose freely among four cardinal movement directions: UP (0,-1), DOWN (0,1), LEFT (-1,0), or RIGHT (1,0). Prefer MOVE when movement is possible. Explore safely and avoid solid obstacles.");
            return string.Join(Environment.NewLine, lines);
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_simulationRunning || _closing) return;
            _world.Update(1.0 / 30.0);
            _scheduler.Update(_world.SimulationTime);
            var p = _actor.Position;
            if (Math.Abs(p.X - _lastX) > 0.000001 || Math.Abs(p.Y - _lastY) > 0.000001)
            {
                Trace(string.Format(CultureInfo.InvariantCulture, "WORLD MOVE: ({0:0.00},{1:0.00}) -> ({2:0.00},{3:0.00}) delta=({4:0.00},{5:0.00})", _lastX, _lastY, p.X, p.Y, p.X - _lastX, p.Y - _lastY));
                _lastX = p.X;
                _lastY = p.Y;
            }
            _position.Text = string.Format(CultureInfo.InvariantCulture, "POSITION\r\n{0:0.00}, {1:0.00}\r\nspeed {2:0.00}", p.X, p.Y, _actor.Speed);
            _state.Text = string.Format(CultureInfo.InvariantCulture, "STATE\r\nrequests {0}/{1}\r\npending {2}\r\naction active {3}\r\ncontinuous {4}\r\nworld {5:0.00}s", _scheduler.ActiveRequestCount, _scheduler.MaxConcurrentRequests, _actor.PendingActionCount, _actor.IsActionActive, _continuousEnabled, _world.SimulationTime);
            _canvas.Invalidate();
        }

        private void OnDecisionLifecycle(object sender, WorldActorDecisionEvent e)
        {
            if (_closing || IsDisposed || !IsHandleCreated) return;
            try
            {
                BeginInvoke((Action)delegate
                {
                    if (_closing || IsDisposed) return;
                    Trace(string.Format(CultureInfo.InvariantCulture, "DECISION {0}: request={1} elapsed={2:0.000}s error={3}", e.Outcome, e.RequestId, e.ElapsedSeconds, e.Error ?? string.Empty));
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
                    var message = execution.Messages[i];
                    builder.AppendLine((message.Role ?? "message").ToUpperInvariant() + ":");
                    builder.AppendLine(message.Content ?? string.Empty);
                }
            if (execution.Response != null)
            {
                builder.AppendLine("ASSISTANT:");
                builder.AppendLine(execution.Response.Text ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(execution.Response.Reasoning))
                {
                    builder.AppendLine("REASONING:");
                    builder.AppendLine(execution.Response.Reasoning);
                }
                if (!string.IsNullOrWhiteSpace(execution.Response.StructuredOutputJson))
                {
                    builder.AppendLine("STRUCTURED OUTPUT:");
                    builder.AppendLine(execution.Response.StructuredOutputJson);
                }
            }
            else if (execution.Error != null)
            {
                builder.AppendLine("ERROR:");
                builder.AppendLine(execution.Error.ToString());
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
            if (_registered)
            {
                try { _scheduler.Unregister(_actor.Id); } catch (Exception) { }
                _registered = false;
            }
            _scheduler.Dispose();
            _provider.ExecutionCompleted -= OnExecutionCompleted;
            _provider.Dispose();
        }

        private static Label MakeInfoLabel(string text, bool bold)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                Text = text,
                ForeColor = bold ? Color.White : Color.FromArgb(205, 215, 225),
                Font = new Font("Segoe UI", bold ? 9.5f : 9f, bold ? FontStyle.Bold : FontStyle.Regular),
                Padding = new Padding(2, 2, 2, 2)
            };
        }

        private static Button MakeButton(string text)
        {
            return new Button
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                Text = text,
                Height = 34,
                Margin = new Padding(2),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.System
            };
        }

        private static void AddBoundary(World world, double x, double y, double width, double height)
        {
            var wall = world.AddItem(new WorldPoint(x, y), width, height, true);
            wall.Kind = "wall";
            wall.Name = "Boundary";
        }
    }
}
