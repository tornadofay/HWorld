using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using HWorld.Core.Geometry;
using HWorld.Core.World;
using HWorld.WinForms.Helpers;
using HWorld.WinForms.Helpers.Button;
using HWorld.WinForms.Rendering;

namespace HWorld.WinForms
{
    public sealed class WorldDesignerForm : Form
    {
        private readonly GdiWorldCanvas _canvas;
        private readonly Timer _timer;
        private TextBox _nameBox, _kindBox;
        private ComboBox _toolBox, _shapeBox;
        private CheckBox _solidBox;
        private NumericUpDown _widthBox, _heightBox, _rotationBox;
        private Label _selectionValue, _itemsValue, _timeValue, _dirtyValue;
        private bool _updating;
        private bool _dirty;

        public WorldDesignerForm(World world = null)
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            BackColor = Color.FromArgb(11, 14, 18);
            ForeColor = Color.FromArgb(235, 239, 244);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = BackColor, ColumnCount = 2, RowCount = 2, Padding = new Padding(10) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(root);

            var header = new Header
            {
                Dock = DockStyle.Fill,
                Title = "HWorld World Designer",
                Subtitle = "Design the world used by the host application",
                AllowMove = false,
                AllowMinimize = true,
                AllowClose = true,
                AllowHelp = true
            };
            header.PerformOnClose += delegate { Close(); };
            header.PerformOnHelp += delegate { HMessage.ShowInformation(this, "Build: select a vector shape and click the world. Select objects to edit them. Delete removes the selected object. Q/E rotate. Save/Open/New manage the world file.", "World Designer"); };
            root.Controls.Add(header, 0, 0);
            root.SetColumnSpan(header, 2);

            root.Controls.Add(BuildSidebar(), 0, 1);

            _canvas = new GdiWorldCanvas { Dock = DockStyle.Fill, Margin = new Padding(10, 0, 0, 0), Mode = CanvasMode.Build };
            _canvas.SelectionChanged += delegate { LoadSelection(); };
            _canvas.WorldEdited += delegate { _dirty = true; UpdateStatus(); UpdateTitle(); };
            root.Controls.Add(_canvas, 1, 1);

            _timer = new Timer { Interval = 33 };
            _timer.Tick += delegate { _canvas.Invalidate(); };
            KeyDown += OnKeyDown;
            FormClosing += OnFormClosing;

            _canvas.World = world ?? CreateEmptyWorld();
            _canvas.Player = FindOrCreatePlayer(_canvas.World);
            _canvas.BuildShape = WorldShapeKind.Rectangle;
            _canvas.BuildKind = "object";
            _canvas.BuildSolid = false;
            _canvas.BuildWidth = 10;
            _canvas.BuildHeight = 10;
            ClearSelection();
            UpdateStatus();
            UpdateTitle();
        }

        public World World { get { return _canvas.World; } }

        private Control BuildSidebar()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 25, 32), Padding = new Padding(16), AutoScroll = true };
            AddTop(panel, MakeLabel("WORLD DESIGNER", 12f, FontStyle.Bold, Color.FromArgb(246, 248, 250)), 28);
            AddTop(panel, MakeLabel("Reusable GDI+ designer control hosted by HWorld.WinForms.", 8.5f, FontStyle.Regular, Color.FromArgb(146, 158, 171)), 40);

            var newButton = MakeButton("New world");
            newButton.Click += delegate { NewWorld(); };
            panel.Controls.Add(newButton);
            var save = MakeButton("Save world");
            save.Click += delegate { SaveWorld(); };
            panel.Controls.Add(save);
            var open = MakeButton("Open world");
            open.Click += delegate { OpenWorld(); };
            panel.Controls.Add(open);
            var fit = MakeButton("Center / fit world");
            fit.Click += delegate { _canvas.ResetView(); };
            panel.Controls.Add(fit);

            AddTop(panel, MakeSectionLabel("VECTOR TOOL"), 24);
            _toolBox = new ComboBox { Dock = DockStyle.Top, Height = 34, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(12, 16, 21), ForeColor = Color.FromArgb(230, 234, 239), FlatStyle = FlatStyle.Flat };
            foreach (var shape in Enum.GetValues(typeof(WorldShapeKind))) _toolBox.Items.Add(shape);
            _toolBox.SelectedItem = WorldShapeKind.Rectangle;
            _toolBox.SelectedIndexChanged += delegate { ApplyTool(); };
            panel.Controls.Add(_toolBox);

            _solidBox = new CheckBox { Text = "Solid / collidable", Dock = DockStyle.Top, Height = 32, Checked = false, ForeColor = Color.FromArgb(205, 213, 222), BackColor = Color.Transparent };
            _solidBox.CheckedChanged += delegate { ApplyTool(); };
            panel.Controls.Add(_solidBox);

            AddTop(panel, MakeSectionLabel("SELECTED OBJECT"), 24);
            _nameBox = MakeTextBox(); AddProperty(panel, "Name", _nameBox);
            _kindBox = MakeTextBox(); AddProperty(panel, "Kind", _kindBox);
            _shapeBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(12, 16, 21), ForeColor = Color.FromArgb(230, 234, 239), FlatStyle = FlatStyle.Flat };
            foreach (var shape in Enum.GetValues(typeof(WorldShapeKind))) _shapeBox.Items.Add(shape);
            AddProperty(panel, "Shape", _shapeBox);
            _widthBox = MakeNumber(0.5m, 1000m, 0.5m); AddProperty(panel, "Width", _widthBox);
            _heightBox = MakeNumber(0.5m, 1000m, 0.5m); AddProperty(panel, "Height", _heightBox);
            _rotationBox = MakeNumber(-360m, 360m, 1m); AddProperty(panel, "Rotation", _rotationBox);
            var apply = MakeButton("Apply selected changes");
            apply.Click += delegate { ApplySelected(); };
            panel.Controls.Add(apply);
            var delete = MakeButton("Delete selected");
            delete.Click += delegate { DeleteSelected(); };
            panel.Controls.Add(delete);

            AddTop(panel, MakeSectionLabel("STATUS"), 24);
            _selectionValue = AddMetric(panel, "Selection");
            _itemsValue = AddMetric(panel, "Objects");
            _timeValue = AddMetric(panel, "Simulation time");
            _dirtyValue = AddMetric(panel, "Changes");
            return panel;
        }

        private static World CreateEmptyWorld()
        {
            var world = new World(240, 150);
            world.AddActor(new WorldPoint(30, 30), speed: 14).Name = "Player";
            return world;
        }

        private static WorldActor FindOrCreatePlayer(World world)
        {
            if (world.Actors.Count > 0) return world.Actors[0];
            var actor = world.AddActor(new WorldPoint(30, 30), speed: 14);
            actor.Name = "Player";
            return actor;
        }

        private void ApplyTool()
        {
            if (_toolBox == null) return;
            var shape = _toolBox.SelectedItem is WorldShapeKind ? (WorldShapeKind)_toolBox.SelectedItem : WorldShapeKind.Rectangle;
            _canvas.BuildShape = shape;
            _canvas.BuildKind = shape.ToString().ToLowerInvariant();
            _canvas.BuildSolid = _solidBox != null && (_solidBox.Checked || shape == WorldShapeKind.House || shape == WorldShapeKind.Pillar);
            switch (shape)
            {
                case WorldShapeKind.Tree: _canvas.BuildWidth = 10; _canvas.BuildHeight = 14; break;
                case WorldShapeKind.House: _canvas.BuildWidth = 22; _canvas.BuildHeight = 18; break;
                case WorldShapeKind.Pillar: _canvas.BuildWidth = 9; _canvas.BuildHeight = 18; break;
                default: _canvas.BuildWidth = 10; _canvas.BuildHeight = 10; break;
            }
        }

        private void LoadSelection()
        {
            var item = _canvas.SelectedItem;
            if (item == null) { ClearSelection(); return; }
            _updating = true;
            _nameBox.Text = item.Name ?? string.Empty;
            _kindBox.Text = item.Kind ?? string.Empty;
            _shapeBox.SelectedItem = item.Shape;
            _widthBox.Value = Clamp((decimal)item.Width, _widthBox.Minimum, _widthBox.Maximum);
            _heightBox.Value = Clamp((decimal)item.Height, _heightBox.Minimum, _heightBox.Maximum);
            _rotationBox.Value = Clamp((decimal)item.RotationDegrees, _rotationBox.Minimum, _rotationBox.Maximum);
            _solidBox.Checked = item.Solid;
            _updating = false;
            _selectionValue.Text = item.Name ?? item.Id.ToString();
        }

        private void ClearSelection()
        {
            _updating = true;
            _nameBox.Clear(); _kindBox.Clear(); _shapeBox.SelectedItem = null;
            _widthBox.Value = 1; _heightBox.Value = 1; _rotationBox.Value = 0; _solidBox.Checked = false;
            _updating = false;
            _selectionValue.Text = "None";
        }

        private void ApplySelected()
        {
            var item = _canvas.SelectedItem;
            if (item == null || _updating) return;
            item.Name = _nameBox.Text.Trim();
            item.Kind = string.IsNullOrWhiteSpace(_kindBox.Text) ? "object" : _kindBox.Text.Trim();
            if (_shapeBox.SelectedItem is WorldShapeKind) item.Shape = (WorldShapeKind)_shapeBox.SelectedItem;
            item.Width = (double)_widthBox.Value;
            item.Height = (double)_heightBox.Value;
            item.RotationDegrees = (double)_rotationBox.Value;
            item.Solid = _solidBox.Checked;
            _canvas.ClampSelectedItemToWorld();
            _canvas.Invalidate();
            _dirty = true;
            UpdateStatus(); UpdateTitle();
        }

        private void DeleteSelected()
        {
            if (_canvas.SelectedItem == null) return;
            if (HMessage.ShowDeleteQuestion(this, "Delete the selected object?", "Delete object") != DialogResult.Yes) return;
            _canvas.DeleteSelectedItem();
            ClearSelection();
        }

        private void NewWorld()
        {
            if (_dirty && HMessage.ShowQuestion(this, "Create a new world? Unsaved changes will be lost.", "New world") != DialogResult.Yes) return;
            _canvas.World = CreateEmptyWorld();
            _canvas.Player = _canvas.World.Actors[0];
            _canvas.ResetView();
            _dirty = false;
            ClearSelection(); UpdateStatus(); UpdateTitle();
        }

        private void SaveWorld()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Save HWorld";
                dialog.Filter = "HWorld files (*.hworld.json)|*.hworld.json|JSON files (*.json)|*.json|All files (*.*)|*.*";
                dialog.DefaultExt = "hworld.json"; dialog.AddExtension = true; dialog.FileName = "world.hworld.json";
                dialog.InitialDirectory = WorldFileService.PrepareDefaultWorldDirectory();
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try { WorldFileService.Save(_canvas.World, dialog.FileName); _dirty = false; UpdateTitle(); UpdateStatus(); HMessage.ShowSuccess(this, "World saved successfully.", "Save world"); }
                catch (Exception ex) { HMessage.ShowException(this, "The world could not be saved.", "Save world", ex); }
            }
        }

        private void OpenWorld()
        {
            if (_dirty && HMessage.ShowQuestion(this, "Open another world? Unsaved changes will be lost.", "Open world") != DialogResult.Yes) return;
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Open HWorld"; dialog.Filter = "HWorld files (*.hworld.json)|*.hworld.json|JSON files (*.json)|*.json|All files (*.*)|*.*";
                dialog.InitialDirectory = WorldFileService.PrepareDefaultWorldDirectory();
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var world = WorldFileService.Load(dialog.FileName);
                    _canvas.World = world; _canvas.Player = FindOrCreatePlayer(world); _canvas.ResetView(); _dirty = false; ClearSelection(); UpdateTitle(); UpdateStatus();
                }
                catch (Exception ex) { HMessage.ShowException(this, "The world file could not be opened.", "Open world", ex); }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S) { SaveWorld(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.O) { OpenWorld(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.N) { NewWorld(); e.SuppressKeyPress = true; }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_dirty && HMessage.ShowQuestion(this, "Close the designer? Unsaved changes will be lost.", "Close designer") != DialogResult.Yes) { e.Cancel = true; return; }
            _timer.Stop();
        }

        private void UpdateTitle() { Text = "HWorld World Designer" + (_dirty ? " *" : ""); }
        private void UpdateStatus()
        {
            _itemsValue.Text = _canvas.World == null ? "0" : _canvas.World.Items.Count.ToString(CultureInfo.InvariantCulture);
            _timeValue.Text = _canvas.World == null ? "0.00 s" : _canvas.World.SimulationTime.ToString("0.00", CultureInfo.InvariantCulture) + " s";
            _dirtyValue.Text = _dirty ? "Unsaved" : "Saved";
            if (_canvas.SelectedItem != null) _selectionValue.Text = _canvas.SelectedItem.Name ?? _canvas.SelectedItem.Id.ToString();
        }

        private static decimal Clamp(decimal value, decimal min, decimal max) { return Math.Max(min, Math.Min(max, value)); }
        private static void AddTop(Control parent, Control control, int height) { control.Dock = DockStyle.Top; control.Height = height; parent.Controls.Add(control); }
        private static Label MakeLabel(string text, float size, FontStyle style, Color color) { return new Label { Text = text, Font = new Font("Segoe UI", size, style), ForeColor = color, BackColor = Color.Transparent, AutoEllipsis = true }; }
        private static Label MakeSectionLabel(string text) { var l = MakeLabel(text, 8f, FontStyle.Bold, Color.FromArgb(104, 193, 228)); l.Dock = DockStyle.Top; l.Height = 24; l.Padding = new Padding(0, 10, 0, 0); return l; }
        private static TextBox MakeTextBox() { return new TextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(12, 16, 21), ForeColor = Color.FromArgb(230, 234, 239), BorderStyle = BorderStyle.FixedSingle }; }
        private static NumericUpDown MakeNumber(decimal min, decimal max, decimal increment) { return new NumericUpDown { Dock = DockStyle.Fill, Minimum = min, Maximum = max, Increment = increment, DecimalPlaces = 1, BackColor = Color.FromArgb(12, 16, 21), ForeColor = Color.FromArgb(230, 234, 239), BorderStyle = BorderStyle.FixedSingle }; }
        private static HButton MakeButton(string text) { return new HButton { Text = text, Width = 270, Height = 36, Dock = DockStyle.Top, Margin = new Padding(0, 4, 0, 4) }; }
        private static void AddProperty(Control parent, string caption, Control value) { var row = new Panel { Dock = DockStyle.Top, Height = 30 }; var label = MakeLabel(caption, 7.5f, FontStyle.Regular, Color.FromArgb(126, 139, 153)); label.Dock = DockStyle.Left; label.Width = 70; row.Controls.Add(label); value.Location = new Point(74, 2); value.Width = 210; value.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; value.Height = 26; row.Controls.Add(value); parent.Controls.Add(row); parent.Controls.SetChildIndex(row, 0); }
        private static Label AddMetric(Control parent, string caption) { var row = new Panel { Dock = DockStyle.Top, Height = 35 }; parent.Controls.Add(row); parent.Controls.SetChildIndex(row, 0); var l = MakeLabel(caption, 7.8f, FontStyle.Regular, Color.FromArgb(126, 139, 153)); l.Dock = DockStyle.Top; l.Height = 15; row.Controls.Add(l); var v = MakeLabel("—", 9.5f, FontStyle.Bold, Color.FromArgb(225, 231, 237)); v.Dock = DockStyle.Fill; row.Controls.Add(v); return v; }
    }
}
