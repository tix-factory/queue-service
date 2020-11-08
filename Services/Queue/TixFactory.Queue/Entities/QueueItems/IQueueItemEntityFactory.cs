using System;
using System.Threading;
using System.Threading.Tasks;

namespace TixFactory.Queue.Entities
{
	internal interface IQueueItemEntityFactory
	{
		Task CreateQueueItem(string queueName, string data, CancellationToken cancellationToken);

		Task<QueueItem> LeaseQueueItem(string queueName, TimeSpan leaseExpiry, CancellationToken cancellationToken);

		Task<long> GetQueueSize(string queueName, CancellationToken cancellationToken);

		Task<long> GetHeldQueueSize(string queueName, CancellationToken cancellationToken);

		Task<int> ClearQueue(string queueName, CancellationToken cancellationToken);

		Task<bool> ReleaseQueueItem(long id, Guid holderId, CancellationToken cancellationToken);

		Task<bool> DeleteQueueItem(long id, Guid holderId, CancellationToken cancellationToken);
	}
}
