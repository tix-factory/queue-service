using System.Threading;
using System.Threading.Tasks;

namespace TixFactory.Queue.Entities
{
	internal interface IQueueEntityFactory
	{
		Task<Queue> GetOrCreateQueue(string name, CancellationToken cancellationToken);

		Task<Queue> GetQueueByName(string name, CancellationToken cancellationToken);

		Task DeleteQueue(Queue queue, CancellationToken cancellationToken);
	}
}
