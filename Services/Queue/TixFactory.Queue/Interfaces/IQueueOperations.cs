using TixFactory.Operations;

namespace TixFactory.Queue
{
	public interface IQueueOperations
	{
		IAsyncOperation<AddQueueItemRequest, AddQueueItemResult> AddQueueItemOperation { get; }

		IAsyncOperation<string, int> ClearQueueOperation { get; }

		IAsyncOperation<LeaseQueueItemRequest, QueueItemResult> LeaseQueueItemOperation { get; }

		IAsyncOperation<ReleaseQueueItemRequest, ReleaseQueueItemResult> ReleaseQueueItemOperation { get; }

		IAsyncOperation<ReleaseQueueItemRequest, ReleaseQueueItemResult> RemoveQueueItemOperation { get; }

		IAsyncOperation<string, long> GetQueueSizeOperation { get; }

		IAsyncOperation<string, long> GetHeldQueueSizeOperation { get; }
	}
}
