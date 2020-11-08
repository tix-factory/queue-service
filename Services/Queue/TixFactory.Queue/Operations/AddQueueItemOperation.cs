using System;
using System.Threading;
using System.Threading.Tasks;
using TixFactory.Operations;
using TixFactory.Queue.Entities;

namespace TixFactory.Queue
{
	internal class AddQueueItemOperation : IAsyncOperation<AddQueueItemRequest, AddQueueItemResult>
	{
		private readonly IQueueItemEntityFactory _QueueItemEntityFactory;

		public AddQueueItemOperation(IQueueItemEntityFactory queueItemEntityFactory)
		{
			_QueueItemEntityFactory = queueItemEntityFactory ?? throw new ArgumentNullException(nameof(queueItemEntityFactory));
		}

		public async Task<(AddQueueItemResult output, OperationError error)> Execute(AddQueueItemRequest request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(request.QueueName) || request.QueueName.Length > EntityValidation.MaxQueueNameLength)
			{
				return (default, new OperationError(QueueError.InvalidQueueName));
			}

			if (string.IsNullOrWhiteSpace(request.Data) || request.Data.Length > EntityValidation.MaxQueueDataLength)
			{
				return (default, new OperationError(QueueError.InvalidData));
			}

			_QueueItemEntityFactory.CreateQueueItem(request.QueueName, request.Data);
			return (AddQueueItemResult.Added, null);
		}
	}
}
