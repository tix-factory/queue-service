using System;

namespace TixFactory.Queue.Entities
{
	internal interface IQueueItemEntityFactory
	{
		void CreateQueueItem(string queueName, string data);

		QueueItem LeaseQueueItem(string queueName, TimeSpan leaseExpiry);

		long GetQueueSize(string queueName);

		int ClearQueue(string queueName);

		bool ReleaseQueueItem(long id, Guid holderId);

		bool DeleteQueueItem(long id, Guid holderId);
	}
}
