using TixFactory.Operations;

namespace TixFactory.Queue
{
	public interface IQueueOperations
	{
		IOperation<AddQueueItemRequest, AddQueueItemResult> AddQueueItemOperation { get; }
	}
}
