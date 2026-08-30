using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using HWorld.Core.Geometry;

namespace HWorld.Core.World
{
    public static class WorldSerializer
    {
        public static string Serialize(World world)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            var snapshot = Capture(world);
            var serializer = new DataContractJsonSerializer(typeof(WorldSnapshot));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, snapshot);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static World Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("World JSON cannot be empty.", nameof(json));
            var serializer = new DataContractJsonSerializer(typeof(WorldSnapshot));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var snapshot = serializer.ReadObject(stream) as WorldSnapshot;
                if (snapshot == null) throw new InvalidDataException("The world document is invalid.");
                return Restore(snapshot);
            }
        }

        public static WorldSnapshot Capture(World world)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            var snapshot = new WorldSnapshot
            {
                Width = world.Width,
                Height = world.Height,
                SimulationTime = world.SimulationTime
            };

            for (int i = 0; i < world.Items.Count; i++)
            {
                var item = world.Items[i];
                snapshot.Items.Add(new WorldItemSnapshot
                {
                    Id = item.Id,
                    X = item.Position.X,
                    Y = item.Position.Y,
                    RotationDegrees = item.RotationDegrees,
                    Width = item.Width,
                    Height = item.Height,
                    Solid = item.Solid,
                    Kind = item.Kind,
                    Name = item.Name,
                    Shape = item.Shape,
                    VisualVariant = item.VisualVariant,
                    Interactable = item.Interactable,
                    InteractionLabel = item.InteractionLabel
                });
            }

            for (int i = 0; i < world.Actors.Count; i++)
            {
                var actor = world.Actors[i];
                snapshot.Actors.Add(new WorldActorSnapshot
                {
                    Id = actor.Id,
                    X = actor.Position.X,
                    Y = actor.Position.Y,
                    RotationDegrees = actor.RotationDegrees,
                    Width = actor.Width,
                    Height = actor.Height,
                    Speed = actor.Speed,
                    Collides = actor.Collides,
                    Name = actor.Name
                });
            }

            return snapshot;
        }

        public static World Restore(WorldSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var world = new World(snapshot.Width, snapshot.Height);

            if (snapshot.Items != null)
            {
                for (int i = 0; i < snapshot.Items.Count; i++)
                {
                    var data = snapshot.Items[i];
                    var item = world.RestoreItem(new WorldItem(data.Id, new WorldPoint(data.X, data.Y))
                    {
                        RotationDegrees = data.RotationDegrees,
                        Width = data.Width,
                        Height = data.Height,
                        Solid = data.Solid,
                        Kind = data.Kind ?? "object",
                        Name = data.Name ?? "Object",
                        Shape = data.Shape,
                        VisualVariant = data.VisualVariant,
                        Interactable = data.Interactable,
                        InteractionLabel = data.InteractionLabel ?? "Interact"
                    });
                    if (item == null) throw new InvalidOperationException("Failed to restore world item.");
                }
            }

            if (snapshot.Actors != null)
            {
                for (int i = 0; i < snapshot.Actors.Count; i++)
                {
                    var data = snapshot.Actors[i];
                    var actor = world.RestoreActor(new WorldActor(data.Id, new WorldPoint(data.X, data.Y))
                    {
                        RotationDegrees = data.RotationDegrees,
                        Width = data.Width,
                        Height = data.Height,
                        Speed = data.Speed,
                        Collides = data.Collides,
                        Name = data.Name ?? "Actor"
                    });
                    if (actor == null) throw new InvalidOperationException("Failed to restore world actor.");
                }
            }

            world.SetSimulationTime(snapshot.SimulationTime);
            return world;
        }
    }
}