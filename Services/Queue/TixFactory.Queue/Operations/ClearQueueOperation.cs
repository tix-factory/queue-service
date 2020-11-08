using System;
using System.Threading;
using System.Threading.Tasks;
using TixFactory.Operations;
using TixFactory.Queue.Entities;

namespace TixFactory.Queue
{
	internal class ClearQueueOperation : IAsyncOperation<string, int>
	{
		private readonly IQueueItemEntityFactory _QueueItemEntityFactory;

		public ClearQueueOperation(IQueueItemEntityFactory queueItemEntityFactory)
		{
			_QueueItemEntityFactory = queueItemEntityFactory ?? throw new ArgumentNullException(nameof(queueItemEntityFactory));
		}

		public async Task<(int output, OperationError error)> Execute(string queueName, CancellationToken cancellationToken)
		{
			var cleared = await _QueueItemEntityFactory.ClearQueue(queueName, cancellationToken).ConfigureAwait(false);
			return (cleared, null);
		}
	}
}
