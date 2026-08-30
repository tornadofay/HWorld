namespace HWorld.Core.World
{
    /// <summary>
    /// Supplies behavior decisions for one actor without coupling the world to an LLM or provider.
    /// Controllers only enqueue validated world actions; the world remains authoritative over execution.
    /// </summary>
    public interface IWorldActorController
    {
        void Update(WorldActorControllerContext context);
    }
}