using System;
using TixFactory.Operations;
using TixFactory.Queue.Entities;

namespace TixFactory.Queue
{
	internal class GetQueueSizeOperation : IOperation<string, long>
	{
		private readonly IQueueItemEntityFactory _QueueItemEntityFactory;

		public GetQueueSizeOperation(IQueueItemEntityFactory queueItemEntityFactory)
		{
			_QueueItemEntityFactory = queueItemEntityFactory ?? throw new ArgumentNullException(nameof(queueItemEntityFactory));
		}

		public (long output, OperationError error) Execute(string queueName)
		{
			var queueSize = _QueueItemEntityFactory.GetQueueSize(queueName);
			return (queueSize, null);
		}
	}
}
