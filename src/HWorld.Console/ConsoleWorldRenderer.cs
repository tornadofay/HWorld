using System;
using System.Text;
using HWorld.Core.Geometry;
using HWorld.Core.World;

namespace HWorld.Console
{
    internal sealed class ConsoleWorldRenderer
    {
        private readonly char[,] _buffer;
        private readonly ConsoleColor[,] _colors;
        private readonly StringBuilder _frame;
        private double _cameraX;
        private double _cameraY;
        private double _unitsPerColumn = 2.0;

        public ConsoleWorldRenderer(int width, int height)
        {
            Width = Math.Max(20, width);
            Height = Math.Max(10, height);
            _buffer = new char[Height, Width];
            _colors = new ConsoleColor[Height, Width];
            _frame = new StringBuilder((Width + 2) * (Height + 3));
        }

        public int Width { get; }
        public int Height { get; }

        public double UnitsPerColumn
        {
            get { return _unitsPerColumn; }
            set { _unitsPerColumn = Math.Max(0.25, Math.Min(20.0, value)); }
        }

        public void CenterOn(WorldActor actor)
        {
            if (actor == null) return;
            _cameraX = actor.Position.X;
            _cameraY = actor.Position.Y;
        }

        public void Render(World world, WorldActor player)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));

            ClearBuffer();
            CenterOn(player);

            var unitsPerRow = UnitsPerColumn;
            var halfWidth = Width * UnitsPerColumn * 0.5;
            var halfHeight = Height * unitsPerRow * 0.5;
            var left = _cameraX - halfWidth;
            var top = _cameraY - halfHeight;

            DrawWorldBounds(world, left, top, unitsPerRow);
            DrawItems(world, left, top, unitsPerRow);
            DrawActors(world, left, top, unitsPerRow, player);
            WriteFrame(world, player);
        }

        private void ClearBuffer()
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    _buffer[y, x] = ' ';
                    _colors[y, x] = ConsoleColor.DarkGray;
                }
            }
        }

        private void DrawWorldBounds(World world, double left, double top, double unitsPerRow)
        {
            for (var x = 0; x < Width; x++)
            {
                var worldX = left + (x + 0.5) * UnitsPerColumn;
                if (worldX >= -UnitsPerColumn && worldX <= world.Width + UnitsPerColumn)
                {
                    SetCell(x, 0, worldX < 0 || worldX > world.Width ? '+' : '-', ConsoleColor.DarkGray);
                    SetCell(x, Height - 1, worldX < 0 || worldX > world.Width ? '+' : '-', ConsoleColor.DarkGray);
                }
            }

            for (var y = 0; y < Height; y++)
            {
                var worldY = top + (y + 0.5) * unitsPerRow;
                if (worldY >= -unitsPerRow && worldY <= world.Height + unitsPerRow)
                {
                    SetCell(0, y, worldY < 0 || worldY > world.Height ? '+' : '|', ConsoleColor.DarkGray);
                    SetCell(Width - 1, y, worldY < 0 || worldY > world.Height ? '+' : '|', ConsoleColor.DarkGray);
                }
            }
        }

        private void DrawItems(World world, double left, double top, double unitsPerRow)
        {
            for (var i = 0; i < world.Items.Count; i++)
            {
                var item = world.Items[i];
                var cellX = WorldToCellX(item.Position.X, left);
                var cellY = WorldToCellY(item.Position.Y, top, unitsPerRow);
                if (!IsInside(cellX, cellY)) continue;

                SetCell(cellX, cellY, GetGlyph(item.Shape, item.Solid), GetColor(item.Kind, item.Solid));
            }
        }

        private void DrawActors(World world, double left, double top, double unitsPerRow, WorldActor player)
        {
            for (var i = 0; i < world.Actors.Count; i++)
            {
                var actor = world.Actors[i];
                var cellX = WorldToCellX(actor.Position.X, left);
                var cellY = WorldToCellY(actor.Position.Y, top, unitsPerRow);
                if (!IsInside(cellX, cellY)) continue;

                var isPlayer = ReferenceEquals(actor, player);
                SetCell(cellX, cellY, isPlayer ? '@' : 'A', isPlayer ? ConsoleColor.Cyan : ConsoleColor.Yellow);
            }
        }

        private void WriteFrame(World world, WorldActor player)
        {
            _frame.Clear();
            _frame.Append("\x1b[H");
            _frame.Append("HWorld Console | ");
            _frame.Append(world.SimulationTime.ToString("0.00"));
            _frame.Append("s | player ");
            _frame.Append(player == null ? "0.0, 0.0" : player.Position.X.ToString("0.0") + ", " + player.Position.Y.ToString("0.0"));
            _frame.Append(" | objects ");
            _frame.Append(world.Items.Count);
            _frame.Append(" | actors ");
            _frame.Append(world.Actors.Count);
            _frame.Append("\x1b[K\r\n");

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    _frame.Append(_buffer[y, x]);
                }
                _frame.Append("\r\n");
            }

            _frame.Append("WASD / arrows move | +/- zoom | Q or Esc quit");
            _frame.Append("\x1b[K\r\n");
            _frame.Append("\x1b[0m");

            try
            {
                System.Console.Write(_frame.ToString());
            }
            catch
            {
                // If a host terminal does not accept VT control sequences, fall back to a plain write.
                try
                {
                    System.Console.SetCursorPosition(0, 0);
                    System.Console.Write(_frame.ToString().Replace("\x1b[H", string.Empty).Replace("\x1b[K", string.Empty).Replace("\x1b[0m", string.Empty));
                }
                catch { }
            }
        }

        private int WorldToCellX(double x, double left)
        {
            return (int)Math.Floor((x - left) / UnitsPerColumn);
        }

        private int WorldToCellY(double y, double top, double unitsPerRow)
        {
            return (int)Math.Floor((y - top) / unitsPerRow);
        }

        private bool IsInside(int x, int y)
        {
            return x > 0 && x < Width - 1 && y > 0 && y < Height - 1;
        }

        private void SetCell(int x, int y, char glyph, ConsoleColor color)
        {
            if (!IsInside(x, y)) return;
            _buffer[y, x] = glyph;
            _colors[y, x] = color;
        }

        private static char GetGlyph(WorldShapeKind shape, bool solid)
        {
            switch (shape)
            {
                case WorldShapeKind.Ellipse: return 'o';
                case WorldShapeKind.Triangle: return '^';
                case WorldShapeKind.Diamond: return '*';
                case WorldShapeKind.Hexagon: return 'O';
                case WorldShapeKind.Star: return '*';
                case WorldShapeKind.Tree: return 'T';
                case WorldShapeKind.House: return 'H';
                case WorldShapeKind.Rock: return 'R';
                case WorldShapeKind.Flower: return 'f';
                case WorldShapeKind.Pillar: return '#';
                case WorldShapeKind.Cross: return '+';
                default: return solid ? '#' : 'o';
            }
        }

        private static ConsoleColor GetColor(string kind, bool solid)
        {
            if (solid) return ConsoleColor.DarkRed;
            if (string.Equals(kind, "nature", StringComparison.OrdinalIgnoreCase)) return ConsoleColor.Green;
            if (string.Equals(kind, "resource", StringComparison.OrdinalIgnoreCase)) return ConsoleColor.Blue;
            if (string.Equals(kind, "landmark", StringComparison.OrdinalIgnoreCase)) return ConsoleColor.Magenta;
            if (string.Equals(kind, "structure", StringComparison.OrdinalIgnoreCase)) return ConsoleColor.DarkYellow;
            return ConsoleColor.Gray;
        }
    }
}
