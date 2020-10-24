using System;
using TixFactory.Logging;
using TixFactory.Operations;
using TixFactory.Queue.Entities;

namespace TixFactory.Queue
{
	internal class LeaseQueueItemOperation : IOperation<LeaseQueueItemRequest, QueueItemResult>
	{
		private readonly IQueueItemEntityFactory _QueueItemEntityFactory;
		private readonly ILogger _Logger;

		public LeaseQueueItemOperation(IQueueItemEntityFactory queueItemEntityFactory, ILogger logger)
		{
			_QueueItemEntityFactory = queueItemEntityFactory ?? throw new ArgumentNullException(nameof(queueItemEntityFactory));
			_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public (QueueItemResult output, OperationError error) Execute(LeaseQueueItemRequest request)
		{
			var queueItem = _QueueItemEntityFactory.LeaseQueueItem(request.QueueName, request.LeaseExpiry);
			if (queueItem == null)
			{
				return (null, null);
			}

			if (!queueItem.HolderId.HasValue)
			{
				_Logger.Warn($"No {nameof(queueItem.HolderId)} after leasing queue item.\n\tID: {queueItem.Id}");
				return (null, null);
			}

			var result = new QueueItemResult
			{
				Id = queueItem.Id,
				Data = queueItem.Data,
				LeaseId = queueItem.HolderId.Value
			};

			return (result, null);
		}
	}
}
