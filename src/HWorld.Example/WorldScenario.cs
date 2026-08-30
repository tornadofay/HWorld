using System;
using HWorld.Core.Geometry;
using HWorld.Core.World;

namespace HWorld.Example
{
    internal sealed class WorldScenario
    {
        public WorldScenario(World world, WorldActor player, string title, string story, int? seed)
        {
            World = world ?? throw new ArgumentNullException(nameof(world));
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Title = title ?? string.Empty;
            Story = story ?? string.Empty;
            Seed = seed;
        }

        public World World { get; }
        public WorldActor Player { get; }
        public string Title { get; }
        public string Story { get; }
        public int? Seed { get; }
    }

    internal static class WorldScenarioFactory
    {
        public static WorldScenario CreateHandBuilt()
        {
            var world = new World(240, 150);
            var player = world.AddActor(new WorldPoint(24, 24), speed: 14);
            player.Name = "You";

            AddWall(world, 5, 5, 230, 3);
            AddWall(world, 5, 142, 230, 3);
            AddWall(world, 5, 5, 3, 140);
            AddWall(world, 232, 5, 3, 140);

            var house = world.AddItem(new WorldPoint(72, 30), 34, 24, true);
            house.Kind = "structure"; house.Name = "Old structure"; house.Shape = WorldShapeKind.House;
            house.Interactable = true; house.InteractionLabel = "Inspect house";

            var tree = world.AddItem(new WorldPoint(32, 82), 7, 10, false);
            tree.Kind = "nature"; tree.Name = "Tree"; tree.Shape = WorldShapeKind.Tree;
            tree.Interactable = true; tree.InteractionLabel = "Examine tree";

            var rock = world.AddItem(new WorldPoint(145, 96), 9, 7, true);
            rock.Kind = "obstacle"; rock.Name = "Rock"; rock.Shape = WorldShapeKind.Rock;

            var landmark = world.AddItem(new WorldPoint(183, 48), 10, 10, false);
            landmark.Kind = "landmark"; landmark.Name = "Unknown landmark"; landmark.Shape = WorldShapeKind.Star;
            landmark.Interactable = true; landmark.InteractionLabel = "Examine landmark";

            var flower = world.AddItem(new WorldPoint(118, 78), 7, 7, false);
            flower.Kind = "nature"; flower.Name = "Flower"; flower.Shape = WorldShapeKind.Flower;
            flower.Interactable = true; flower.InteractionLabel = "Examine flower";

            var pillar = world.AddItem(new WorldPoint(198, 94), 8, 16, true);
            pillar.Kind = "structure"; pillar.Name = "Stone pillar"; pillar.Shape = WorldShapeKind.Pillar;
            pillar.Interactable = true; pillar.InteractionLabel = "Inspect pillar";

            return new WorldScenario(
                world,
                player,
                "First World",
                "A small handcrafted world. Build it, then walk through what you created.",
                null);
        }

        public static WorldScenario CreateSeeded(int seed)
        {
            var random = new Random(seed);
            var world = new World(360, 220);
            var player = world.AddActor(new WorldPoint(30, 30), speed: 16);
            player.Name = "You";

            AddWall(world, 4, 4, 352, 3);
            AddWall(world, 4, 213, 352, 3);
            AddWall(world, 4, 4, 3, 212);
            AddWall(world, 353, 4, 3, 212);

            int obstacles = 28 + random.Next(18);
            for (int i = 0; i < obstacles; i++)
            {
                double x = 15 + random.NextDouble() * 325;
                double y = 15 + random.NextDouble() * 185;
                double w = 6 + random.NextDouble() * 18;
                double h = 6 + random.NextDouble() * 16;

                if (Math.Abs(x - player.Position.X) < 25 && Math.Abs(y - player.Position.Y) < 25) { i--; continue; }

                var item = world.AddItem(new WorldPoint(x, y), w, h, random.NextDouble() < 0.65);
                int visualKind = random.Next(5);
                switch (visualKind)
                {
                    case 0:
                        item.Kind = "nature"; item.Name = "Unknown growth"; item.Shape = random.Next(2) == 0 ? WorldShapeKind.Tree : WorldShapeKind.Flower; break;
                    case 1:
                        item.Kind = "obstacle"; item.Name = "Stone formation"; item.Shape = random.Next(2) == 0 ? WorldShapeKind.Rock : WorldShapeKind.Hexagon; break;
                    case 2:
                        item.Kind = "structure"; item.Name = "Ruined structure"; item.Shape = random.Next(2) == 0 ? WorldShapeKind.House : WorldShapeKind.Pillar; item.Solid = true; break;
                    case 3:
                        item.Kind = "landmark"; item.Name = "Strange landmark"; item.Shape = random.Next(2) == 0 ? WorldShapeKind.Star : WorldShapeKind.Diamond; break;
                    default:
                        item.Kind = "object"; item.Name = "Unfamiliar object"; item.Shape = random.Next(2) == 0 ? WorldShapeKind.Ellipse : WorldShapeKind.Triangle; break;
                }
                item.RotationDegrees = random.Next(0, 4) * 15;
                item.VisualVariant = (byte)random.Next(4);
                if (!item.Solid && random.NextDouble() < 0.18)
                {
                    item.Interactable = true;
                    item.InteractionLabel = "Examine object";
                }
            }

            int clearings = 3 + random.Next(3);
            for (int i = 0; i < clearings; i++)
            {
                double x = 45 + random.NextDouble() * 245;
                double y = 40 + random.NextDouble() * 130;
                var item = world.AddItem(new WorldPoint(x, y), 18, 18, false);
                item.Kind = "resource";
                item.Name = "Untyped resource";
                item.Shape = random.Next(2) == 0 ? WorldShapeKind.Ellipse : WorldShapeKind.Star;
                item.VisualVariant = (byte)random.Next(4);
                item.Interactable = true;
                item.InteractionLabel = "Examine resource";
            }

            string story = "Seed " + seed + " generated an unknown landscape. The shapes have no fixed meaning to the player; exploration gives the world context.";
            return new WorldScenario(world, player, "Seeded World " + seed, story, seed);
        }

        private static void AddWall(World world, double x, double y, double width, double height)
        {
            var wall = world.AddItem(new WorldPoint(x, y), width, height, true);
            wall.Kind = "wall";
            wall.Name = "Boundary";
            wall.Shape = WorldShapeKind.Rectangle;
        }
    }
}