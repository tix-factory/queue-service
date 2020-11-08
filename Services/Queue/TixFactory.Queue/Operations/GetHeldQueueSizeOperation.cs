using System;
using System.Threading;
using System.Threading.Tasks;
using TixFactory.Operations;
using TixFactory.Queue.Entities;

namespace TixFactory.Queue
{
	internal class GetHeldQueueSizeOperation : IAsyncOperation<string, long>
	{
		private readonly IQueueItemEntityFactory _QueueItemEntityFactory;

		public GetHeldQueueSizeOperation(IQueueItemEntityFactory queueItemEntityFactory)
		{
			_QueueItemEntityFactory = queueItemEntityFactory ?? throw new ArgumentNullException(nameof(queueItemEntityFactory));
		}

		public async Task<(long output, OperationError error)> Execute(string queueName, CancellationToken cancellationToken)
		{
			var queueSize = _QueueItemEntityFactory.GetHeldQueueSize(queueName);
			return (queueSize, null);
		}
	}
}
