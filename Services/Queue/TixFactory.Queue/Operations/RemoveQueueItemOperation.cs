using System;
using TixFactory.Operations;
using TixFactory.Queue.Entities;

namespace TixFactory.Queue
{
	internal class RemoveQueueItemOperation : IOperation<ReleaseQueueItemRequest, ReleaseQueueItemResult>
	{
		private readonly IQueueItemEntityFactory _QueueItemEntityFactory;

		public RemoveQueueItemOperation(IQueueItemEntityFactory queueItemEntityFactory)
		{
			_QueueItemEntityFactory = queueItemEntityFactory ?? throw new ArgumentNullException(nameof(queueItemEntityFactory));
		}

		public (ReleaseQueueItemResult output, OperationError error) Execute(ReleaseQueueItemRequest request)
		{
			var released = _QueueItemEntityFactory.DeleteQueueItem(request.Id, request.LeaseId);
			if (!released)
			{
				return (ReleaseQueueItemResult.InvalidLeaseHolder, null);
			}

			return (ReleaseQueueItemResult.Removed, null);
		}
	}
}
