using System.Drawing;
using System.Windows.Forms;
using HWorld.Core.Geometry;
using HWorld.Core.World;

namespace HWorld.WinForms
{
    internal sealed class MainForm : Form
    {
        private readonly World _world;

        public MainForm()
        {
            Text = "HWorld";
            Width = 900;
            Height = 600;
            DoubleBuffered = true;

            _world = new World(100, 60);
            _world.AddItem(new WorldPoint(30, 25));
            _world.AddItem(new WorldPoint(55, 25), 3, 8, solid: true);
            _world.AddItem(new WorldPoint(75, 40));

            Paint += OnPaintWorld;
        }

        private void OnPaintWorld(object sender, PaintEventArgs e)
        {
            var sx = ClientSize.Width / (float)_world.Width;
            var sy = ClientSize.Height / (float)_world.Height;
            var scale = System.Math.Min(sx, sy);

            foreach (var item in _world.Items)
            {
                var x = (float)item.Position.X * scale;
                var y = (float)item.Position.Y * scale;
                var w = (float)item.Width * scale;
                var h = (float)item.Height * scale;
                using (var brush = new SolidBrush(item.Solid ? Color.Black : Color.DodgerBlue))
                    e.Graphics.FillRectangle(brush, x, y, System.Math.Max(2, w), System.Math.Max(2, h));
            }
        }
    }
}
