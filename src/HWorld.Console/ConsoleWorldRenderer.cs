using System;
using HWorld.Core.Geometry;
using HWorld.Core.World;

namespace HWorld.Console
{
    internal sealed class ConsoleWorldRenderer
    {
        private readonly char[,] _buffer;
        private readonly ConsoleColor[,] _colors;
        private double _cameraX;
        private double _cameraY;
        private double _unitsPerColumn = 2.0;

        public ConsoleWorldRenderer(int width, int height)
        {
            Width = Math.Max(20, width);
            Height = Math.Max(10, height);
            _buffer = new char[Height, Width];
            _colors = new ConsoleColor[Height, Width];
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
            var right = left + Width * UnitsPerColumn;
            var bottom = top + Height * unitsPerRow;

            for (var x = 0; x < Width; x++)
            {
                var worldX = left + (x + 0.5) * UnitsPerColumn;
                if (worldX >= -UnitsPerColumn && worldX <= world.Width + UnitsPerColumn)
                {
                    SetCell(x, 0, worldX < 0 || worldX > world.Width ? '+' : '─', ConsoleColor.DarkGray);
                    SetCell(x, Height - 1, worldX < 0 || worldX > world.Width ? '+' : '─', ConsoleColor.DarkGray);
                }
            }

            for (var y = 0; y < Height; y++)
            {
                var worldY = top + (y + 0.5) * unitsPerRow;
                if (worldY >= -unitsPerRow && worldY <= world.Height + unitsPerRow)
                {
                    SetCell(0, y, worldY < 0 || worldY > world.Height ? '+' : '│', ConsoleColor.DarkGray);
                    SetCell(Width - 1, y, worldY < 0 || worldY > world.Height ? '+' : '│', ConsoleColor.DarkGray);
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

                var glyph = GetGlyph(item.Shape, item.Solid);
                var color = GetColor(item.Kind, item.Solid);
                SetCell(cellX, cellY, glyph, color);
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
            try
            {
                System.Console.SetCursorPosition(0, 0);
            }
            catch
            {
                System.Console.Clear();
            }

            for (var y = 0; y < Height; y++)
            {
                ConsoleColor activeColor = ConsoleColor.Gray;
                for (var x = 0; x < Width; x++)
                {
                    if (_colors[y, x] != activeColor)
                    {
                        activeColor = _colors[y, x];
                        System.Console.ForegroundColor = activeColor;
                    }
                    System.Console.Write(_buffer[y, x]);
                }
                System.Console.WriteLine();
            }

            System.Console.ForegroundColor = ConsoleColor.Gray;
            System.Console.WriteLine();
            System.Console.WriteLine("HWorld Console | {0:0.00}s | player {1:0.0}, {2:0.0} | objects {3} | actors {4}",
                world.SimulationTime,
                player == null ? 0 : player.Position.X,
                player == null ? 0 : player.Position.Y,
                world.Items.Count,
                world.Actors.Count);
            System.Console.WriteLine("WASD / arrows move | +/- zoom | Q quit");
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
                case WorldShapeKind.Ellipse: return '○';
                case WorldShapeKind.Triangle: return '▲';
                case WorldShapeKind.Diamond: return '◆';
                case WorldShapeKind.Hexagon: return '⬢';
                case WorldShapeKind.Star: return '★';
                case WorldShapeKind.Tree: return '♣';
                case WorldShapeKind.House: return '⌂';
                case WorldShapeKind.Rock: return '●';
                case WorldShapeKind.Flower: return '✿';
                case WorldShapeKind.Pillar: return '▮';
                case WorldShapeKind.Cross: return '✚';
                default: return solid ? '█' : '□';
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
