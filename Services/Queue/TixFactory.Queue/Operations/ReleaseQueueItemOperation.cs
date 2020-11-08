using System;
using System.Threading;
using System.Threading.Tasks;
using TixFactory.Operations;
using TixFactory.Queue.Entities;

namespace TixFactory.Queue
{
	internal class ReleaseQueueItemOperation : IAsyncOperation<ReleaseQueueItemRequest, ReleaseQueueItemResult>
	{
		private readonly IQueueItemEntityFactory _QueueItemEntityFactory;

		public ReleaseQueueItemOperation(IQueueItemEntityFactory queueItemEntityFactory)
		{
			_QueueItemEntityFactory = queueItemEntityFactory ?? throw new ArgumentNullException(nameof(queueItemEntityFactory));
		}

		public async Task<(ReleaseQueueItemResult output, OperationError error)> Execute(ReleaseQueueItemRequest request, CancellationToken cancellationToken)
		{
			var released = _QueueItemEntityFactory.ReleaseQueueItem(request.Id, request.LeaseId);
			if (!released)
			{
				return (ReleaseQueueItemResult.InvalidLeaseHolder, null);
			}

			return (ReleaseQueueItemResult.Released, null);
		}
	}
}
