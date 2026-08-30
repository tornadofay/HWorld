using System;
using System.Runtime.InteropServices;
using HWorld.Core.World;

namespace HWorld.Console
{
    public static class ConsoleWorldRunner
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        public static void Run(World world, WorldActor player)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (player == null) throw new ArgumentNullException(nameof(player));

            bool ownsConsole = !HasConsole();
            if (ownsConsole) AllocConsole();

            try
            {
                System.Console.CursorVisible = false;
                var running = true;
                var watch = System.Diagnostics.Stopwatch.StartNew();
                double previous = 0;

                while (running)
                {
                    while (System.Console.KeyAvailable)
                    {
                        var key = System.Console.ReadKey(true).Key;
                        if (key == ConsoleKey.Escape) running = false;
                        else if (key == ConsoleKey.W || key == ConsoleKey.UpArrow) world.MoveActor(player.Id, 0, -1, 1.0 / 12.0);
                        else if (key == ConsoleKey.S || key == ConsoleKey.DownArrow) world.MoveActor(player.Id, 0, 1, 1.0 / 12.0);
                        else if (key == ConsoleKey.A || key == ConsoleKey.LeftArrow) world.MoveActor(player.Id, -1, 0, 1.0 / 12.0);
                        else if (key == ConsoleKey.D || key == ConsoleKey.RightArrow) world.MoveActor(player.Id, 1, 0, 1.0 / 12.0);
                    }

                    double now = watch.Elapsed.TotalSeconds;
                    double dt = Math.Min(0.1, now - previous);
                    previous = now;
                    if (dt > 0) world.Update(dt);
                    Render(world, player);
                    System.Threading.Thread.Sleep(70);
                }
            }
            finally
            {
                System.Console.CursorVisible = true;
                if (ownsConsole) FreeConsole();
            }
        }

        private static void Render(World world, WorldActor player)
        {
            const int width = 84;
            const int height = 28;
            var buffer = new char[height, width];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++) buffer[y, x] = '.';

            var viewWidth = Math.Min(world.Width, width - 2);
            var viewHeight = Math.Min(world.Height, height - 2);
            double left = Math.Max(0, player.Position.X - viewWidth / 2.0);
            double top = Math.Max(0, player.Position.Y - viewHeight / 2.0);
            left = Math.Min(left, Math.Max(0, world.Width - viewWidth));
            top = Math.Min(top, Math.Max(0, world.Height - viewHeight));

            for (int i = 0; i < world.Items.Count; i++)
            {
                var item = world.Items[i];
                int x = 1 + (int)Math.Floor(item.Position.X - left);
                int y = 1 + (int)Math.Floor(item.Position.Y - top);
                if (x < 0 || x >= width || y < 0 || y >= height) continue;
                buffer[y, x] = GetGlyph(item.Shape, item.Solid);
            }

            int px = 1 + (int)Math.Round(player.Position.X - left);
            int py = 1 + (int)Math.Round(player.Position.Y - top);
            if (px >= 0 && px < width && py >= 0 && py < height) buffer[py, px] = '@';

            System.Console.SetCursorPosition(0, 0);
            System.Console.WriteLine("HWorld.Console  |  WASD / arrows = move  |  Esc = close".PadRight(width));
            for (int y = 0; y < height; y++)
            {
                System.Console.Write('+');
                for (int x = 0; x < width - 2; x++) System.Console.Write(buffer[y, x]);
                System.Console.WriteLine('+');
            }
            System.Console.WriteLine(("Player: " + player.Position.X.ToString("0.0") + ", " + player.Position.Y.ToString("0.0") + "  Time: " + world.SimulationTime.ToString("0.00") + " s").PadRight(width));
        }

        private static char GetGlyph(Core.World.WorldShapeKind shape, bool solid)
        {
            if (solid) return '#';
            switch (shape)
            {
                case Core.World.WorldShapeKind.Tree: return '♣';
                case Core.World.WorldShapeKind.House: return '⌂';
                case Core.World.WorldShapeKind.Rock: return '◆';
                case Core.World.WorldShapeKind.Flower: return '✿';
                case Core.World.WorldShapeKind.Star: return '★';
                case Core.World.WorldShapeKind.Diamond: return '◇';
                case Core.World.WorldShapeKind.Triangle: return '▲';
                case Core.World.WorldShapeKind.Ellipse: return '●';
                default: return 'o';
            }
        }

        private static bool HasConsole()
        {
            return GetConsoleWindow() != IntPtr.Zero;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
    }
}
