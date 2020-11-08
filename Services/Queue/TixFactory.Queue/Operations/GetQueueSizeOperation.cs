using System;
using System.Threading;
using System.Threading.Tasks;
using TixFactory.Operations;
using TixFactory.Queue.Entities;

namespace TixFactory.Queue
{
	internal class GetQueueSizeOperation : IAsyncOperation<string, long>
	{
		private readonly IQueueItemEntityFactory _QueueItemEntityFactory;

		public GetQueueSizeOperation(IQueueItemEntityFactory queueItemEntityFactory)
		{
			_QueueItemEntityFactory = queueItemEntityFactory ?? throw new ArgumentNullException(nameof(queueItemEntityFactory));
		}

		public async Task<(long output, OperationError error)> Execute(string queueName, CancellationToken cancellationToken)
		{
			var queueSize = await _QueueItemEntityFactory.GetQueueSize(queueName, cancellationToken).ConfigureAwait(false);
			return (queueSize, null);
		}
	}
}
