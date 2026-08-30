using System;
using HWorld.Core.Geometry;
using HWorld.Core.World;

namespace HWorld.Console
{
    internal static class Program
    {
        private static void Main()
        {
            var world = new World(40, 20);
            var player = world.AddItem(new WorldPoint(10, 8));
            world.AddItem(new WorldPoint(20, 8), 2, 4, solid: true);
            world.AddItem(new WorldPoint(30, 12), 1, 1);

            Render(world, player);
        }

        private static void Render(World world, WorldItem player)
        {
            var width = (int)world.Width;
            var height = (int)world.Height;
            var grid = new char[height, width];

            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    grid[y, x] = '.';

            foreach (var item in world.Items)
            {
                var x = (int)Math.Round(item.Position.X);
                var y = (int)Math.Round(item.Position.Y);
                if (x < 0 || x >= width || y < 0 || y >= height)
                    continue;

                grid[y, x] = item == player ? 'A' : (item.Solid ? '#' : 'o');
            }

            System.Console.Clear();
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                    System.Console.Write(grid[y, x]);
                System.Console.WriteLine();
            }

            System.Console.WriteLine();
            System.Console.WriteLine("HWorld core proof-of-life | time: {0:0.000}s", world.SimulationTime);
        }
    }
}
