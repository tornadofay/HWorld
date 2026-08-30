using System;
using System.Threading;
using HWorld.Core.Geometry;
using HWorld.Core.World;

namespace HWorld.Console
{
    internal static class Program
    {
        private static void Main()
        {
            System.Console.CursorVisible = false;

            var world = CreateDemoWorld();
            var player = world.Actors[0];
            var renderer = new ConsoleWorldRenderer(
                Math.Min(100, Math.Max(40, GetWindowWidth() - 1)),
                Math.Min(32, Math.Max(16, GetWindowHeight() - 5)));

            var running = true;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var previous = stopwatch.Elapsed.TotalSeconds;

            try
            {
                while (running)
                {
                    ProcessInput(world, player, renderer, ref running);

                    var now = stopwatch.Elapsed.TotalSeconds;
                    var delta = Math.Min(0.05, Math.Max(0, now - previous));
                    previous = now;

                    MovePlayer(world, player, delta);
                    world.Update(delta);
                    renderer.Render(world, player);
                    Thread.Sleep(16);
                }
            }
            finally
            {
                System.Console.ForegroundColor = ConsoleColor.Gray;
                System.Console.CursorVisible = true;
                System.Console.Clear();
                System.Console.WriteLine("HWorld Console closed.");
            }
        }

        private static World CreateDemoWorld()
        {
            var world = new World(180, 100);
            var player = world.AddActor(new WorldPoint(40, 50), speed: 24);
            player.Name = "You";

            AddWall(world, 8, 8, 164, 3);
            AddWall(world, 8, 89, 164, 3);
            AddWall(world, 8, 8, 3, 84);
            AddWall(world, 169, 8, 3, 84);
            AddWall(world, 92, 20, 3, 36);
            AddWall(world, 92, 65, 3, 20);

            AddItem(world, new WorldPoint(25, 25), 10, 12, WorldShapeKind.Tree, "nature", false);
            AddItem(world, new WorldPoint(55, 30), 12, 10, WorldShapeKind.Rock, "obstacle", true);
            AddItem(world, new WorldPoint(120, 30), 18, 14, WorldShapeKind.House, "structure", true);
            AddItem(world, new WorldPoint(145, 65), 12, 12, WorldShapeKind.Star, "landmark", false);
            AddItem(world, new WorldPoint(65, 72), 8, 8, WorldShapeKind.Flower, "nature", false);
            AddItem(world, new WorldPoint(125, 75), 10, 10, WorldShapeKind.Diamond, "resource", false);
            return world;
        }

        private static void AddWall(World world, double x, double y, double width, double height)
        {
            var item = world.AddItem(new WorldPoint(x, y), width, height, true);
            item.Kind = "wall";
            item.Name = "Boundary";
            item.Shape = WorldShapeKind.Rectangle;
        }

        private static void AddItem(World world, WorldPoint position, double width, double height, WorldShapeKind shape, string kind, bool solid)
        {
            var item = world.AddItem(position, width, height, solid);
            item.Kind = kind;
            item.Name = shape.ToString();
            item.Shape = shape;
        }

        private static void ProcessInput(World world, WorldActor player, ConsoleWorldRenderer renderer, ref bool running)
        {
            while (System.Console.KeyAvailable)
            {
                var key = System.Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        SetDirection(0, -1);
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        SetDirection(0, 1);
                        break;
                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow:
                        SetDirection(-1, 0);
                        break;
                    case ConsoleKey.D:
                    case ConsoleKey.RightArrow:
                        SetDirection(1, 0);
                        break;
                    case ConsoleKey.Add:
                    case ConsoleKey.OemPlus:
                        renderer.UnitsPerColumn = renderer.UnitsPerColumn / 1.15;
                        break;
                    case ConsoleKey.Subtract:
                    case ConsoleKey.OemMinus:
                        renderer.UnitsPerColumn = renderer.UnitsPerColumn * 1.15;
                        break;
                    case ConsoleKey.Q:
                    case ConsoleKey.Escape:
                        running = false;
                        break;
                }
            }
        }

        private static double _moveX;
        private static double _moveY;

        private static void SetDirection(double x, double y)
        {
            _moveX = x;
            _moveY = y;
        }

        private static void MovePlayer(World world, WorldActor player, double deltaSeconds)
        {
            if (Math.Abs(_moveX) < double.Epsilon && Math.Abs(_moveY) < double.Epsilon)
                return;

            world.MoveActor(player.Id, _moveX, _moveY, deltaSeconds);
        }

        private static int GetWindowWidth()
        {
            try { return System.Console.WindowWidth; }
            catch { return 80; }
        }

        private static int GetWindowHeight()
        {
            try { return System.Console.WindowHeight; }
            catch { return 25; }
        }
    }
}
