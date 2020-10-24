using System;
using TixFactory.Operations;
using TixFactory.Queue.Entities;

namespace TixFactory.Queue
{
	internal class ClearQueueOperation : IOperation<string, int>
	{
		private readonly IQueueItemEntityFactory _QueueItemEntityFactory;

		public ClearQueueOperation(IQueueItemEntityFactory queueItemEntityFactory)
		{
			_QueueItemEntityFactory = queueItemEntityFactory ?? throw new ArgumentNullException(nameof(queueItemEntityFactory));
		}

		public (int output, OperationError error) Execute(string queueName)
		{
			var cleared = _QueueItemEntityFactory.ClearQueue(queueName);
			return (cleared, null);
		}
	}
}
