using System;
using TixFactory.Operations;
using TixFactory.Queue.Entities;

namespace TixFactory.Queue
{
	internal class GetHeldQueueSizeOperation : IOperation<string, long>
	{
		private readonly IQueueItemEntityFactory _QueueItemEntityFactory;

		public GetHeldQueueSizeOperation(IQueueItemEntityFactory queueItemEntityFactory)
		{
			_QueueItemEntityFactory = queueItemEntityFactory ?? throw new ArgumentNullException(nameof(queueItemEntityFactory));
		}

		public (long output, OperationError error) Execute(string queueName)
		{
			var queueSize = _QueueItemEntityFactory.GetHeldQueueSize(queueName);
			return (queueSize, null);
		}
	}
}
