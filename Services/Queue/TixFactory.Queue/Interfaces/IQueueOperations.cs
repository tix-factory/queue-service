using TixFactory.Operations;

namespace TixFactory.Queue
{
	public interface IQueueOperations
	{
		IOperation<AddQueueItemRequest, AddQueueItemResult> AddQueueItemOperation { get; }

		IOperation<string, int> ClearQueueOperation { get; }
	}
}
