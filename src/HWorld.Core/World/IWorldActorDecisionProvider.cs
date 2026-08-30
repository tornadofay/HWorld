using System.Threading;
using System.Threading.Tasks;

namespace HWorld.Core.World
{
    /// <summary>
    /// Produces an action for one actor without giving the decision system direct control
    /// over world state. Implementations may perform asynchronous external work.
    /// </summary>
    public interface IWorldActorDecisionProvider
    {
        Task<WorldActorAction> DecideAsync(WorldActorDecisionContext context, CancellationToken cancellationToken);
    }
}
