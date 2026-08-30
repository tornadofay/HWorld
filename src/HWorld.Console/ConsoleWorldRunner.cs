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

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        public static void Run(World world, WorldActor player)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (player == null) throw new ArgumentNullException(nameof(player));

            bool ownsConsole = GetConsoleWindow() == IntPtr.Zero;
            if (ownsConsole) AllocConsole();

            try
            {
                System.Console.OutputEncoding = System.Text.Encoding.UTF8;
                System.Console.CursorVisible = false;
                bool running = true;
                var watch = System.Diagnostics.Stopwatch.StartNew();
                double previous = watch.Elapsed.TotalSeconds;
                var renderer = new ConsoleWorldRenderer(84, 28);

                while (running)
                {
                    while (System.Console.KeyAvailable)
                    {
                        var key = System.Console.ReadKey(true).Key;
                        const double inputSeconds = 1.0 / 30.0;
                        switch (key)
                        {
                            case ConsoleKey.W:
                            case ConsoleKey.UpArrow:
                                world.MoveActor(player.Id, 0, -1, inputSeconds); break;
                            case ConsoleKey.S:
                            case ConsoleKey.DownArrow:
                                world.MoveActor(player.Id, 0, 1, inputSeconds); break;
                            case ConsoleKey.A:
                            case ConsoleKey.LeftArrow:
                                world.MoveActor(player.Id, -1, 0, inputSeconds); break;
                            case ConsoleKey.D:
                            case ConsoleKey.RightArrow:
                                world.MoveActor(player.Id, 1, 0, inputSeconds); break;
                            case ConsoleKey.Escape:
                            case ConsoleKey.Q:
                                running = false; break;
                            case ConsoleKey.Add:
                            case ConsoleKey.OemPlus:
                                renderer.UnitsPerColumn /= 1.15; break;
                            case ConsoleKey.Subtract:
                            case ConsoleKey.OemMinus:
                                renderer.UnitsPerColumn *= 1.15; break;
                        }
                    }

                    double now = watch.Elapsed.TotalSeconds;
                    double dt = Math.Min(0.05, Math.Max(0, now - previous));
                    previous = now;
                    world.Update(dt);
                    renderer.Render(world, player);
                    System.Threading.Thread.Sleep(16);
                }
            }
            finally
            {
                try { System.Console.CursorVisible = true; System.Console.Clear(); } catch { }
                if (ownsConsole) FreeConsole();
            }
        }
    }
}
