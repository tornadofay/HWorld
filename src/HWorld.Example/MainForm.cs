using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using HWorld.Core.Geometry;
using HWorld.Core.World;
using HWorld.WinForms.Helpers;
using HWorld.WinForms.Helpers.Button;

namespace HWorld.Example
{
    internal sealed class MainForm : Form
    {
        private readonly WorldCanvas _canvas;
        private readonly Timer _timer;
        private Label _timeValue, _itemsValue, _statusValue, _zoomValue;
        private TextBox _seedBox, _nameBox, _kindBox;
        private ComboBox _toolBox, _shapeBox;
        private CheckBox _solidBox, _propertySolidBox;
        private NumericUpDown _widthBox, _heightBox, _rotationBox;
        private Label _modeValue, _playerValue, _seedValue, _storyValue, _hintValue;
        private Panel _propertiesPanel;
        private bool _updatingProperties;
        private WorldScenario _scenario;
        private bool _running, _up, _down, _left, _right;
        private string _worldPath;
        private bool _dirty;

        public MainForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            BackColor = Color.FromArgb(11, 14, 18);
            ForeColor = Color.FromArgb(235, 239, 244);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = BackColor, ColumnCount = 2, RowCount = 2, Padding = new Padding(10) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(root);

            var header = new Header { Dock = DockStyle.Fill, Title = "HWorld", Subtitle = "World playground  •  Build, explore, and experiment", AllowMove = false, AllowMinimize = true, AllowClose = true, AllowHelp = true };
            header.PerformOnClose += delegate { Close(); };
            header.PerformOnHelp += delegate { HMessage.ShowInformation(this, "Build: choose a vector shape and click the world. Select an existing object to edit it.\r\nPlay: WASD / arrow keys.\r\nDelete: selected object. Q / E: rotate selected object.\r\nSave/Open/New: use the buttons or Ctrl+S / Ctrl+O / Ctrl+N.", "HWorld controls"); };
            root.Controls.Add(header, 0, 0); root.SetColumnSpan(header, 2);
            root.Controls.Add(BuildSidebar(), 0, 1);

            _canvas = new WorldCanvas { Dock = DockStyle.Fill, Margin = new Padding(10, 0, 0, 0), Mode = CanvasMode.Observe };
            _canvas.SelectionChanged += delegate { LoadSelectedProperties(); };
            root.Controls.Add(_canvas, 1, 1);

            _timer = new Timer { Interval = 33 }; _timer.Tick += OnTick;
            KeyDown += OnKeyDown; KeyUp += OnKeyUp;
            FormClosing += OnFormClosing;
            _canvas.WorldEdited += delegate { MarkDirty(); UpdateStatus(); };

            LoadHandBuiltWorld(false);
        }

        private Control BuildSidebar()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 25, 32), Padding = new Padding(16), AutoScroll = true };
            var title = MakeLabel("WORLD LAB", 12f, FontStyle.Bold, Color.FromArgb(246, 248, 250)); title.Dock = DockStyle.Top; title.Height = 26; panel.Controls.Add(title);
            var source = MakeLabel("Create worlds, edit vector objects, then play inside them.", 8.7f, FontStyle.Regular, Color.FromArgb(146, 158, 171)); source.Dock = DockStyle.Top; source.Height = 38; panel.Controls.Add(source);

            var newWorld = MakeButton("New hand-built world"); newWorld.Click += delegate { LoadHandBuiltWorld(true); }; panel.Controls.Add(newWorld);
            var save = MakeButton("Save world"); save.Click += delegate { SaveWorld(); }; panel.Controls.Add(save);
            var open = MakeButton("Open world"); open.Click += delegate { OpenWorld(); }; panel.Controls.Add(open);

            var seedRow = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(0, 5, 0, 5) }; panel.Controls.Add(seedRow);
            _seedBox = new TextBox { Text = "20260830", Dock = DockStyle.Left, Width = 145, Font = new Font("Segoe UI", 10f), BackColor = Color.FromArgb(12, 16, 21), ForeColor = Color.FromArgb(230, 234, 239), BorderStyle = BorderStyle.FixedSingle }; seedRow.Controls.Add(_seedBox);
            var random = MakeButton("Generate"); random.Width = 110; random.Height = 34; random.Location = new Point(153, 5); random.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; random.Click += delegate { GenerateSeededWorld(); }; seedRow.Controls.Add(random);

            panel.Controls.Add(MakeSectionLabel("MODE"));
            var build = MakeButton("Build"); build.Click += delegate { SetMode(CanvasMode.Build); }; panel.Controls.Add(build);
            var play = MakeButton("Play as me"); play.Click += delegate { SetMode(CanvasMode.Play); }; panel.Controls.Add(play);
            var observe = MakeButton("Observe"); observe.Click += delegate { SetMode(CanvasMode.Observe); }; panel.Controls.Add(observe);

            panel.Controls.Add(MakeSectionLabel("VECTOR OBJECT"));
            _toolBox = new ComboBox { Dock = DockStyle.Top, Height = 34, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(12, 16, 21), ForeColor = Color.FromArgb(230, 234, 239), FlatStyle = FlatStyle.Flat };
            foreach (var shape in Enum.GetValues(typeof(WorldShapeKind))) _toolBox.Items.Add(shape);
            _toolBox.SelectedItem = WorldShapeKind.Rectangle; _toolBox.SelectedIndexChanged += delegate { ApplyTool(); }; panel.Controls.Add(_toolBox);
            _solidBox = new CheckBox { Text = "Solid / collidable", Dock = DockStyle.Top, Height = 32, Checked = false, ForeColor = Color.FromArgb(205, 213, 222), BackColor = Color.Transparent }; _solidBox.CheckedChanged += delegate { ApplyTool(); }; panel.Controls.Add(_solidBox);
            var fit = MakeButton("Center / fit world"); fit.Click += delegate { _canvas.ResetView(); }; panel.Controls.Add(fit);

            panel.Controls.Add(MakeSectionLabel("SELECTED OBJECT")); _propertiesPanel = BuildPropertiesPanel(); panel.Controls.Add(_propertiesPanel);
            panel.Controls.Add(MakeSectionLabel("WORLD"));
            _modeValue = AddMetric(panel, "Mode"); _timeValue = AddMetric(panel, "Simulation time"); _itemsValue = AddMetric(panel, "Objects"); _playerValue = AddMetric(panel, "Player"); _seedValue = AddMetric(panel, "Seed"); _zoomValue = AddMetric(panel, "Zoom"); _statusValue = AddMetric(panel, "Status");
            panel.Controls.Add(MakeSectionLabel("STORY")); _storyValue = MakeLabel("—", 8.5f, FontStyle.Regular, Color.FromArgb(173, 182, 193)); _storyValue.Dock = DockStyle.Top; _storyValue.Height = 72; _storyValue.AutoEllipsis = false; panel.Controls.Add(_storyValue);
            _hintValue = MakeLabel("", 8.2f, FontStyle.Regular, Color.FromArgb(119, 132, 145)); _hintValue.Dock = DockStyle.Top; _hintValue.Height = 74; panel.Controls.Add(_hintValue);
            return panel;
        }

        private Panel BuildPropertiesPanel()
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 312, BackColor = Color.FromArgb(14, 18, 24), Padding = new Padding(10) };
            _nameBox = MakePropertyTextBox(); AddProperty(panel, "Name", _nameBox);
            _kindBox = MakePropertyTextBox(); AddProperty(panel, "Kind", _kindBox);
            _shapeBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(12, 16, 21), ForeColor = Color.FromArgb(230, 234, 239), FlatStyle = FlatStyle.Flat }; foreach (var shape in Enum.GetValues(typeof(WorldShapeKind))) _shapeBox.Items.Add(shape); AddProperty(panel, "Shape", _shapeBox);
            _widthBox = MakeNumber(0.5m, 1000m, 0.5m); AddProperty(panel, "Width", _widthBox);
            _heightBox = MakeNumber(0.5m, 1000m, 0.5m); AddProperty(panel, "Height", _heightBox);
            _rotationBox = MakeNumber(-360m, 360m, 1m); AddProperty(panel, "Rotation", _rotationBox);
            _propertySolidBox = new CheckBox { Text = "Solid / collidable", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(205, 213, 222), BackColor = Color.Transparent }; AddProperty(panel, "", _propertySolidBox);
            var apply = MakeButton("Apply changes"); apply.Height = 34; apply.Click += delegate { ApplySelectedProperties(); }; panel.Controls.Add(apply);
            var del = MakeButton("Delete selected"); del.Height = 34; del.Click += delegate { DeleteSelected(); }; panel.Controls.Add(del);
            ClearPropertyControls(); return panel;
        }

        private TextBox MakePropertyTextBox() { return new TextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(12, 16, 21), ForeColor = Color.FromArgb(230, 234, 239), BorderStyle = BorderStyle.FixedSingle }; }
        private NumericUpDown MakeNumber(decimal min, decimal max, decimal increment) { return new NumericUpDown { Dock = DockStyle.Fill, Minimum = min, Maximum = max, Increment = increment, DecimalPlaces = 1, BackColor = Color.FromArgb(12, 16, 21), ForeColor = Color.FromArgb(230, 234, 239), BorderStyle = BorderStyle.FixedSingle }; }
        private void AddProperty(Control parent, string caption, Control value) { var row = new Panel { Dock = DockStyle.Top, Height = 30 }; var label = MakeLabel(caption, 7.5f, FontStyle.Regular, Color.FromArgb(126, 139, 153)); label.Dock = DockStyle.Left; label.Width = 70; row.Controls.Add(label); value.Location = new Point(74, 2); value.Width = 190; value.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; value.Height = 26; row.Controls.Add(value); parent.Controls.Add(row); parent.Controls.SetChildIndex(row, 0); }
        private void ClearPropertyControls() { _updatingProperties = true; _nameBox.Clear(); _kindBox.Clear(); _shapeBox.SelectedItem = null; _widthBox.Value = 1; _heightBox.Value = 1; _rotationBox.Value = 0; _propertySolidBox.Checked = false; _updatingProperties = false; }
        private void LoadSelectedProperties() { var item = _canvas.SelectedItem; if (item == null) { ClearPropertyControls(); return; } _updatingProperties = true; _nameBox.Text = item.Name ?? string.Empty; _kindBox.Text = item.Kind ?? string.Empty; _shapeBox.SelectedItem = item.Shape; _widthBox.Value = ClampDecimal((decimal)item.Width, _widthBox.Minimum, _widthBox.Maximum); _heightBox.Value = ClampDecimal((decimal)item.Height, _heightBox.Minimum, _heightBox.Maximum); _rotationBox.Value = ClampDecimal((decimal)item.RotationDegrees, _rotationBox.Minimum, _rotationBox.Maximum); _propertySolidBox.Checked = item.Solid; _updatingProperties = false; }
        private void ApplySelectedProperties() { var item = _canvas.SelectedItem; if (item == null || _updatingProperties) return; item.Name = _nameBox.Text.Trim(); item.Kind = string.IsNullOrWhiteSpace(_kindBox.Text) ? "object" : _kindBox.Text.Trim(); if (_shapeBox.SelectedItem is WorldShapeKind) item.Shape = (WorldShapeKind)_shapeBox.SelectedItem; item.Width = (double)_widthBox.Value; item.Height = (double)_heightBox.Value; item.RotationDegrees = (double)_rotationBox.Value; item.Solid = _propertySolidBox.Checked; _canvas.ClampSelectedItemToWorld(); WorldItemEdited(); }
        private void WorldItemEdited() { MarkDirty(); _canvas.Invalidate(); UpdateStatus(); }
        private void DeleteSelected() { if (_canvas.SelectedItem == null) return; _canvas.DeleteSelectedItem(); ClearPropertyControls(); MarkDirty(); UpdateStatus(); }
        private static decimal ClampDecimal(decimal value, decimal min, decimal max) { return Math.Max(min, Math.Min(max, value)); }
        private HButton MakeButton(string text) { return new HButton { Text = text, Width = 260, Height = 36, Dock = DockStyle.Top, Margin = new Padding(0, 4, 0, 4), ButtonLeaveBackGroundColor1 = Color.FromArgb(47, 57, 69), ButtonLeaveBackGroundColor2 = Color.FromArgb(30, 37, 46), ButtonEnterBackGroundColor1 = Color.FromArgb(73, 88, 105), ButtonEnterBackGroundColor2 = Color.FromArgb(47, 57, 69), ButtonDownBackGroundColor1 = Color.FromArgb(33, 40, 49), ButtonDownBackGroundColor2 = Color.FromArgb(25, 31, 38), ButtonLeaveForeColor = Color.FromArgb(231, 237, 244), ButtonEnterForeColor = Color.White, ButtonDownForeColor = Color.White }; }
        private Label MakeSectionLabel(string text) { var l = MakeLabel(text, 8f, FontStyle.Bold, Color.FromArgb(104, 193, 228)); l.Dock = DockStyle.Top; l.Height = 24; l.Padding = new Padding(0, 10, 0, 0); return l; }
        private Label AddMetric(Control parent, string caption) { var row = new Panel { Dock = DockStyle.Top, Height = 35 }; parent.Controls.Add(row); parent.Controls.SetChildIndex(row, 0); var l = MakeLabel(caption, 7.8f, FontStyle.Regular, Color.FromArgb(126, 139, 153)); l.Dock = DockStyle.Top; l.Height = 15; row.Controls.Add(l); var v = MakeLabel("—", 9.5f, FontStyle.Bold, Color.FromArgb(225, 231, 237)); v.Dock = DockStyle.Fill; row.Controls.Add(v); return v; }
        private static Label MakeLabel(string text, float size, FontStyle style, Color color) { return new Label { Text = text, Font = new Font("Segoe UI", size, style), ForeColor = color, BackColor = Color.Transparent, AutoEllipsis = true }; }

        private void LoadHandBuiltWorld(bool confirm) { if (confirm && !ConfirmDiscardChanges("Create a new hand-built world? Unsaved changes will be lost.")) return; _timer.Stop(); _running = false; _scenario = WorldScenarioFactory.CreateHandBuilt(); _worldPath = null; _dirty = false; ApplyScenario(); SetMode(CanvasMode.Build); }
        private void GenerateSeededWorld() { if (!ConfirmDiscardChanges("Generate a new world from this seed? Unsaved changes will be lost.")) return; int seed; if (!int.TryParse(_seedBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out seed)) { HMessage.ShowWarning(this, "Enter a valid integer seed.", "Generate world"); _seedBox.Focus(); return; } _timer.Stop(); _running = false; _scenario = WorldScenarioFactory.CreateSeeded(seed); _worldPath = null; _dirty = false; ApplyScenario(); SetMode(CanvasMode.Build); }
        private void ApplyScenario() { _canvas.World = _scenario.World; _canvas.Player = _scenario.Player; _canvas.Mode = CanvasMode.Build; _canvas.ResetView(); _storyValue.Text = _scenario.Story; ApplyTool(); UpdateFormTitle(); UpdateStatus(); }
        private void SetMode(CanvasMode mode) { _canvas.Mode = mode; if (mode == CanvasMode.Play) { _running = true; _timer.Start(); _canvas.Focus(); _canvas.CenterOnPlayer(); _hintValue.Text = "PLAY  •  WASD / arrow keys to move  •  mouse wheel to zoom"; } else if (mode == CanvasMode.Build) { _running = false; _timer.Stop(); _canvas.Focus(); _hintValue.Text = "BUILD  •  click to place the selected vector shape  •  right-click to remove"; } else { _running = false; _timer.Stop(); _hintValue.Text = "OBSERVE  •  simulation paused"; } UpdateStatus(); _canvas.Invalidate(); }
        private void ApplyTool() { if (_toolBox == null || _solidBox == null || _canvas == null) return; var shape = _toolBox.SelectedItem is WorldShapeKind ? (WorldShapeKind)_toolBox.SelectedItem : WorldShapeKind.Rectangle; _canvas.BuildShape = shape; _canvas.BuildKind = shape.ToString().ToLowerInvariant(); _canvas.BuildSolid = _solidBox.Checked || shape == WorldShapeKind.Pillar || shape == WorldShapeKind.House; switch (shape) { case WorldShapeKind.Tree: _canvas.BuildWidth = 10; _canvas.BuildHeight = 14; break; case WorldShapeKind.House: _canvas.BuildWidth = 22; _canvas.BuildHeight = 18; break; case WorldShapeKind.Pillar: _canvas.BuildWidth = 9; _canvas.BuildHeight = 18; break; default: _canvas.BuildWidth = 10; _canvas.BuildHeight = 10; break; } }
        private void OnTick(object sender, EventArgs e) { const double dt = 1.0 / 30.0; if (!_running || _scenario == null) return; double x = 0, y = 0; if (_left) x -= 1; if (_right) x += 1; if (_up) y -= 1; if (_down) y += 1; if (Math.Abs(x) > 0 || Math.Abs(y) > 0) _scenario.World.MoveActor(_scenario.Player.Id, x, y, dt); _scenario.World.Update(dt); MarkDirty(); UpdateStatus(); _canvas.Invalidate(); }
        private void OnKeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) _up = true; if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) _down = true; if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) _left = true; if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) _right = true; if (_canvas.Mode == CanvasMode.Play && e.KeyCode == Keys.Escape) SetMode(CanvasMode.Observe); if (e.Control && e.KeyCode == Keys.S) { SaveWorld(); e.SuppressKeyPress = true; } else if (e.Control && e.KeyCode == Keys.O) { OpenWorld(); e.SuppressKeyPress = true; } else if (e.Control && e.KeyCode == Keys.N) { LoadHandBuiltWorld(true); e.SuppressKeyPress = true; } }
        private void OnKeyUp(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) _up = false; if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) _down = false; if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) _left = false; if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right) _right = false; }

        private void SaveWorld() { if (_scenario == null) return; var path = _worldPath; if (string.IsNullOrWhiteSpace(path)) { using (var dialog = new SaveFileDialog()) { dialog.Title = "Save HWorld"; dialog.Filter = "HWorld files (*.hworld.json)|*.hworld.json|JSON files (*.json)|*.json|All files (*.*)|*.*"; dialog.DefaultExt = "hworld.json"; dialog.AddExtension = true; dialog.FileName = "world.hworld.json"; dialog.InitialDirectory = WorldFileService.PrepareDefaultWorldDirectory(); if (dialog.ShowDialog(this) != DialogResult.OK) return; path = dialog.FileName; } } try { WorldFileService.Save(_scenario.World, path); _worldPath = path; _dirty = false; UpdateFormTitle(); UpdateStatus(); HMessage.ShowSuccess(this, "World saved successfully.\r\n" + path, "Save world"); } catch (Exception ex) { HMessage.ShowException(this, "The world could not be saved.", "Save world", ex); } }
        private void OpenWorld() { if (!ConfirmDiscardChanges("Open another world? Unsaved changes will be lost.")) return; using (var dialog = new OpenFileDialog()) { dialog.Title = "Open HWorld"; dialog.Filter = "HWorld files (*.hworld.json)|*.hworld.json|JSON files (*.json)|*.json|All files (*.*)|*.*"; dialog.InitialDirectory = WorldFileService.PrepareDefaultWorldDirectory(); if (dialog.ShowDialog(this) != DialogResult.OK) return; try { var world = WorldFileService.Load(dialog.FileName); WorldActor player = world.Actors.Count > 0 ? world.Actors[0] : world.AddActor(new WorldPoint(20, 20), speed: 14); _scenario = new WorldScenario(world, player, Path.GetFileNameWithoutExtension(dialog.FileName), "Loaded from " + dialog.FileName + ".", null); _worldPath = dialog.FileName; _dirty = false; ApplyScenario(); SetMode(CanvasMode.Build); } catch (Exception ex) { HMessage.ShowException(this, "The world file could not be opened.", "Open world", ex); } } }
        private bool ConfirmDiscardChanges(string message) { if (!_dirty) return true; return HMessage.ShowQuestion(this, message, "Unsaved changes") == DialogResult.Yes; }
        private void MarkDirty() { _dirty = true; UpdateFormTitle(); }
        private void UpdateFormTitle() { var name = _scenario == null ? "HWorld" : _scenario.Name; Text = "HWorld • " + name + (_dirty ? " *" : ""); }
        private void OnFormClosing(object sender, FormClosingEventArgs e) { if (!ConfirmDiscardChanges("Close HWorld? Unsaved changes will be lost.")) { e.Cancel = true; return; } _timer.Stop(); }
        private void UpdateStatus() { if (_scenario == null || _canvas == null) return; if (_modeValue != null) _modeValue.Text = _canvas.Mode.ToString(); if (_timeValue != null) _timeValue.Text = _scenario.World.SimulationTime.ToString("0.00") + " s"; if (_itemsValue != null) _itemsValue.Text = _scenario.World.Items.Count.ToString(); if (_playerValue != null) _playerValue.Text = string.Format("{0:0.0}, {1:0.0}", _scenario.Player.Position.X, _scenario.Player.Position.Y); if (_seedValue != null) _seedValue.Text = _scenario.Seed.HasValue ? _scenario.Seed.Value.ToString() : "manual"; if (_zoomValue != null) _zoomValue.Text = _canvas.Zoom.ToString("0.00") + "x"; if (_statusValue != null) _statusValue.Text = _running ? "Running" : "Paused"; }
    }
}
