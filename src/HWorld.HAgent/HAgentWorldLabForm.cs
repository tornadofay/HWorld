using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
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
using HWorld.WinForms.Helpers;
using HWorld.WinForms.Helpers.Button;
using HWorld.WinForms.Rendering;

namespace HWorld.HAgent
{
    public sealed class HAgentWorldLabForm : Form
    {
        private readonly HAgentClient _client;
        private readonly AgentRuntimeInstance _runtimeInstance;
        private readonly HAgentDecisionProvider _decisionProvider;
        private readonly World _world;
        private readonly WorldActor _actor;
        private readonly WorldActorDecisionScheduler _scheduler;
        private readonly WorldGeometryCamera _camera;
        private readonly List<WorldGeometryObservation> _observations = new List<WorldGeometryObservation>(32);
        private readonly System.Windows.Forms.Timer _timer;
        private readonly GdiWorldCanvas _canvas;
        private readonly GeometryCameraView _cameraView;
        private RichTextBox _chat;
        private Label _status;
        private Label _position;
        private Label _decisionState;
        private bool _running = true;
        private bool _sensorMode;
        private bool _closing;

        private HAgentWorldLabForm(HAgentClient client, AgentRuntimeInstance runtimeInstance, string agentName)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _runtimeInstance = runtimeInstance ?? throw new ArgumentNullException(nameof(runtimeInstance));

            _world = BuildWorld();
            _actor = _world.Actors[0];
            _actor.Name = agentName;

            _decisionProvider = new HAgentDecisionProvider(_client, _runtimeInstance);
            _decisionProvider.ExecutionCompleted += OnExecutionCompleted;
            _scheduler = new WorldActorDecisionScheduler(_world, 1);
            _scheduler.DecisionLifecycle += OnDecisionLifecycle;
            _scheduler.ObservationFactory = BuildObservation;
            _scheduler.Register(_actor, _decisionProvider, new WorldActorDecisionOptions
            {
                DecisionCadenceSeconds = 1.0,
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
            MinimumSize = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            BackColor = Color.FromArgb(11, 14, 18);
            ForeColor = Color.FromArgb(235, 239, 244);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = BackColor, ColumnCount = 1, RowCount = 2, Padding = new Padding(10) };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(root);

            var header = new Header
            {
                Dock = DockStyle.Fill,
                Title = "HWorld + HAgent",
                Subtitle = "one persistent agent runtime controlling one live world actor",
                AllowMove = false,
                AllowMinimize = true,
                AllowClose = true,
                AllowHelp = true
            };
            header.PerformOnClose += delegate { Close(); };
            header.PerformOnHelp += delegate
            {
                HMessage.ShowInformation(this,
                    "The actor receives a geometry observation and asks HAgent for its next decision.\r\n\r\n"
                    + "HWorld owns movement, collision, simulation time and action application.\r\n"
                    + "The HAgent runtime instance remains alive while this actor lives.\r\n\r\n"
                    + "The full HAgent execution conversation is shown in the trace panel.",
                    "HWorld + HAgent");
            };
            root.Controls.Add(header, 0, 0);

            var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = BackColor };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70f));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            root.Controls.Add(content, 0, 1);

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = BackColor, Margin = new Padding(0, 8, 8, 0) };
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 72f));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 28f));
            content.Controls.Add(left, 0, 0);

            var viewport = new Panel { Dock = DockStyle.Fill, BackColor = BackColor };
            _canvas = new GdiWorldCanvas { Dock = DockStyle.Fill, World = _world, Player = _actor, Mode = CanvasMode.Play };
            _cameraView = new GeometryCameraView { Dock = DockStyle.Fill, World = _world, Observer = _actor, Visible = false };
            _cameraView.Camera.Range = 48;
            _cameraView.Camera.FieldOfViewDegrees = 100;
            viewport.Controls.Add(_canvas);
            viewport.Controls.Add(_cameraView);
            left.Controls.Add(viewport, 0, 0);

            _chat = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(14, 18, 24),
                ForeColor = Color.FromArgb(220, 225, 232),
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 9f),
                DetectUrls = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            var chatPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Color.FromArgb(14, 18, 24), Padding = new Padding(10) };
            chatPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            chatPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            chatPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            chatPanel.Controls.Add(new Label { Text = "LLM CHAT / COGNITION TRACE", Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 10f, FontStyle.Bold) }, 0, 0);
            chatPanel.Controls.Add(_chat, 0, 1);
            var clear = MakeButton("Clear");
            clear.Click += delegate { _chat.Clear(); };
            chatPanel.Controls.Add(clear, 0, 2);
            left.Controls.Add(chatPanel, 0, 1);

            content.Controls.Add(BuildSidePanel(), 1, 0);

            _timer = new System.Windows.Forms.Timer { Interval = 33 };
            _timer.Tick += OnTick;
            FormClosing += OnFormClosing;
            KeyDown += OnKeyDown;
            _timer.Start();
            _canvas.CenterOnPlayer();
            _canvas.Focus();
            AppendChat("SYSTEM: HAgent runtime instance " + _runtimeInstance.InstanceId + " started for actor '" + _actor.Name + "'.\r\n\r\n");
        }

        public static async Task<HAgentWorldLabForm> CreateAsync()
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

            if (selected == null)
                throw new InvalidOperationException("No enabled HAgent is configured. Open HAgent Config and create or enable an agent first.");

            var client = new HAgentClient(store, secrets, adapters);
            var instance = AgentRuntimeInstance.Create(selected, AgentRuntimeScope.Session);
            return new HAgentWorldLabForm(client, instance, selected.Name);
        }

        private TableLayoutPanel BuildSidePanel()
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 8, BackColor = Color.FromArgb(20, 25, 32), Padding = new Padding(14), Margin = new Padding(0, 8, 0, 0) };
            for (int i = 0; i < 8; i++) panel.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 3 ? 56 : i == 6 ? 0 : i == 7 ? 36 : 44));
            panel.RowStyles[0] = new RowStyle(SizeType.Absolute, 30);
            panel.RowStyles[1] = new RowStyle(SizeType.Absolute, 48);
            panel.RowStyles[2] = new RowStyle(SizeType.Absolute, 48);
            panel.RowStyles[3] = new RowStyle(SizeType.Absolute, 56);
            panel.RowStyles[4] = new RowStyle(SizeType.Absolute, 36);
            panel.RowStyles[5] = new RowStyle(SizeType.Absolute, 36);
            panel.RowStyles[6] = new RowStyle(SizeType.Percent, 100);
            panel.RowStyles[7] = new RowStyle(SizeType.Absolute, 36);

            panel.Controls.Add(new Label { Text = "HAGENT ACTOR", Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 11f, FontStyle.Bold) }, 0, 0);
            _status = MakeInfoLabel("Agent\r\n—");
            _position = MakeInfoLabel("Position\r\n—");
            _decisionState = MakeInfoLabel("Decision\r\nstarting…");
            panel.Controls.Add(_status, 0, 1);
            panel.Controls.Add(_position, 0, 2);
            panel.Controls.Add(_decisionState, 0, 3);

            var pause = MakeButton("Pause");
            pause.Click += delegate { _running = !_running; pause.Text = _running ? "Pause" : "Resume"; };
            panel.Controls.Add(pause, 0, 4);
            var sensor = MakeButton("Geometry Eye");
            sensor.Click += delegate { SetSensorMode(!_sensorMode); };
            panel.Controls.Add(sensor, 0, 5);
            panel.Controls.Add(new Label { Dock = DockStyle.Fill, ForeColor = Color.FromArgb(165, 175, 186), Font = new Font("Segoe UI", 8.5f), Text = "HAgent decides asynchronously.\r\nHWorld remains authoritative over actions." }, 0, 6);

            var shutdown = MakeButton("Stop Agent");
            shutdown.Click += delegate
            {
                if (_closing) return;
                _closing = true;
                shutdown.Enabled = false;
                if (IsHandleCreated && !IsDisposed) BeginInvoke((Action)delegate { Close(); });
            };
            panel.Controls.Add(shutdown, 0, 7);
            return panel;
        }

        private World BuildWorld()
        {
            var world = new World(180, 100);
            AddBoundary(world, 2, 2, 176, 3);
            AddBoundary(world, 2, 95, 176, 3);
            AddBoundary(world, 2, 2, 3, 96);
            AddBoundary(world, 175, 2, 3, 96);
            var actor = world.AddActor(new WorldPoint(24, 50), width: 5, height: 5, speed: 11);
            actor.RotationDegrees = 0;
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
            lines.Add("Explore safely. Prefer movement toward visible non-solid objects when practical. Avoid solid obstacles.");
            return string.Join(Environment.NewLine, lines);
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_running || _closing) return;
            const double dt = 1.0 / 30.0;
            _world.Update(dt);
            _scheduler.Update(_world.SimulationTime);
            _position.Text = string.Format(CultureInfo.InvariantCulture, "Position\r\n{0:0.0}, {1:0.0}", _actor.Position.X, _actor.Position.Y);
            _status.Text = "Agent\r\n" + _actor.Name;
            _decisionState.Text = string.Format(CultureInfo.InvariantCulture, "Decision\r\nactive {0}/{1}\r\nworld {2:0.00}s", _scheduler.ActiveRequestCount, _scheduler.MaxConcurrentRequests, _world.SimulationTime);
            if (_sensorMode) _cameraView.RefreshObservation();
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
                    _decisionState.Text = string.Format(CultureInfo.InvariantCulture, "Decision\r\n{0}\r\nlatency {1:0.000}s", e.Outcome, e.ElapsedSeconds);
                    if (e.Outcome == WorldActorDecisionOutcome.Failed || e.Outcome == WorldActorDecisionOutcome.Rejected)
                        AppendChat("WORLD: " + e.Outcome + ": " + e.Error + "\r\n\r\n");
                });
            }
            catch (InvalidOperationException) { }
        }

        private void OnExecutionCompleted(AgentExecution execution)
        {
            if (_closing || IsDisposed || execution == null) return;
            AppendExecutionToChat(execution);
        }

        private void AppendExecutionToChat(AgentExecution execution)
        {
            if (_chat == null || _chat.IsDisposed) return;
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("============================================================");
            builder.AppendLine("EXECUTION " + execution.Id);
            builder.AppendLine("CORRELATION " + execution.CorrelationId);
            builder.AppendLine("HOST CORRELATION " + execution.HostCorrelationId);
            builder.AppendLine("RUNTIME " + execution.RuntimeInstanceId + " / rev " + execution.RuntimeInstanceRevision);
            builder.AppendLine("STATE " + execution.State + "  FAILURE " + execution.FailureKind);
            builder.AppendLine();
            if (execution.Messages != null)
            {
                for (int i = 0; i < execution.Messages.Count; i++)
                {
                    var message = execution.Messages[i];
                    builder.AppendLine((message.Role ?? "message").ToUpperInvariant() + ":");
                    builder.AppendLine(message.Content ?? string.Empty);
                    builder.AppendLine();
                }
            }
            var response = execution.Response;
            if (response != null)
            {
                builder.AppendLine("ASSISTANT:");
                builder.AppendLine(response.Text ?? string.Empty);
                builder.AppendLine();
                if (!string.IsNullOrWhiteSpace(response.Reasoning))
                {
                    builder.AppendLine("REASONING:");
                    builder.AppendLine(response.Reasoning);
                    builder.AppendLine();
                }
                if (!string.IsNullOrWhiteSpace(response.StructuredOutputJson))
                {
                    builder.AppendLine("STRUCTURED OUTPUT:");
                    builder.AppendLine(response.StructuredOutputJson);
                    builder.AppendLine();
                }
                builder.AppendLine("PROVIDER: " + (response.ProviderId ?? string.Empty));
                builder.AppendLine("MODEL: " + (response.Model ?? string.Empty));
            }
            else if (execution.Error != null)
            {
                builder.AppendLine("ERROR:");
                builder.AppendLine(execution.Error.ToString());
            }
            builder.AppendLine();
            var text = builder.ToString();
            if (_chat.InvokeRequired)
            {
                try { _chat.BeginInvoke((Action)delegate { AppendChat(text); }); }
                catch (InvalidOperationException) { }
            }
            else AppendChat(text);
        }

        private void AppendChat(string text)
        {
            if (_chat == null || _chat.IsDisposed) return;
            _chat.AppendText(text ?? string.Empty);
            _chat.SelectionStart = _chat.TextLength;
            _chat.ScrollToCaret();
        }

        private void SetSensorMode(bool enabled)
        {
            _sensorMode = enabled;
            _cameraView.Visible = enabled;
            _canvas.Visible = !enabled;
            if (enabled) _cameraView.RefreshObservation();
            else _canvas.CenterOnPlayer();
            (enabled ? (Control)_cameraView : _canvas).Focus();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            _closing = true;
            _timer.Stop();
            _scheduler.Dispose();
            _decisionProvider.ExecutionCompleted -= OnExecutionCompleted;
            _decisionProvider.Dispose();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2) SetSensorMode(!_sensorMode);
            if (e.KeyCode == Keys.Escape) Close();
        }

        private static HButton MakeButton(string text) { return new HButton { Text = text, Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 4) }; }
        private static Label MakeInfoLabel(string text) { return new Label { Text = text, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(205, 215, 225), Font = new Font("Segoe UI", 8.5f), BackColor = Color.Transparent }; }
        private static void AddBoundary(World world, double x, double y, double width, double height)
        {
            var wall = world.AddItem(new WorldPoint(x, y), width, height, true);
            wall.Kind = "wall";
            wall.Name = "Boundary";
        }
    }
}
