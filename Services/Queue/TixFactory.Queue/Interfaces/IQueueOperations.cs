using TixFactory.Operations;

namespace TixFactory.Queue
{
	public interface IQueueOperations
	{
		IOperation<AddQueueItemRequest, AddQueueItemResult> AddQueueItemOperation { get; }

		IOperation<string, int> ClearQueueOperation { get; }

		IOperation<LeaseQueueItemRequest, QueueItemResult> LeaseQueueItemOperation { get; }

		IOperation<ReleaseQueueItemRequest, ReleaseQueueItemResult> ReleaseQueueItemOperation { get; }

		IOperation<ReleaseQueueItemRequest, ReleaseQueueItemResult> RemoveQueueItemOperation { get; }

		IOperation<string, long> GetQueueSizeOperation { get; }
	}
}
