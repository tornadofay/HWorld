using System;
using HWorld.Core.Geometry;

namespace HWorld.Core.World
{
    public static class WorldInteraction
    {
        public static WorldInteractionResult TryInteract(World world, Guid actorId, Guid itemId, double reach = 2.5)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (reach < 0) throw new ArgumentOutOfRangeException(nameof(reach));

            var actor = world.FindActor(actorId);
            if (actor == null) return WorldInteractionResult.ActorNotFound;

            WorldItem item = null;
            for (int i = 0; i < world.Items.Count; i++)
            {
                if (world.Items[i].Id == itemId)
                {
                    item = world.Items[i];
                    break;
                }
            }

            if (item == null) return WorldInteractionResult.ItemNotFound;
            if (!item.Interactable) return WorldInteractionResult.ItemNotInteractable;

            var itemCenter = new WorldPoint(item.Position.X + item.Width * 0.5, item.Position.Y + item.Height * 0.5);
            var dx = itemCenter.X - actor.Position.X;
            var dy = itemCenter.Y - actor.Position.Y;
            if ((dx * dx + dy * dy) > reach * reach) return WorldInteractionResult.OutOfReach;

            return WorldInteractionResult.Succeeded;
        }
    }
}
