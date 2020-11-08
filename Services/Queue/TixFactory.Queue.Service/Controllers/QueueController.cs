using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TixFactory.Http.Service;

namespace TixFactory.Queue.Service.Controllers
{
	[Route("v1/[action]")]
	public class QueueController : Controller
	{
		private readonly IQueueOperations _QueueOperations;
		private readonly IOperationExecuter _OperationExecuter;

		public QueueController(IQueueOperations queueOperations, IOperationExecuter operationExecuter)
		{
			_QueueOperations = queueOperations ?? throw new ArgumentNullException(nameof(queueOperations));
			_OperationExecuter = operationExecuter ?? throw new ArgumentNullException(nameof(operationExecuter));
		}

		[HttpPost]
		public Task<IActionResult> AddQueueItem([FromBody] AddQueueItemRequest request, CancellationToken cancellationToken)
		{
			return _OperationExecuter.ExecuteAsync(_QueueOperations.AddQueueItemOperation, request, cancellationToken);
		}

		[HttpPost]
		public Task<IActionResult> ClearQueue([FromBody] RequestPayload<string> request, CancellationToken cancellationToken)
		{
			return _OperationExecuter.ExecuteAsync(_QueueOperations.ClearQueueOperation, request.Data, cancellationToken);
		}

		[HttpPost]
		public Task<IActionResult> LeaseQueueItem([FromBody] LeaseQueueItemRequest request, CancellationToken cancellationToken)
		{
			return _OperationExecuter.ExecuteAsync(_QueueOperations.LeaseQueueItemOperation, request, cancellationToken);
		}

		[HttpPost]
		public Task<IActionResult> ReleaseQueueItem([FromBody] ReleaseQueueItemRequest request, CancellationToken cancellationToken)
		{
			return _OperationExecuter.ExecuteAsync(_QueueOperations.ReleaseQueueItemOperation, request, cancellationToken);
		}

		[HttpPost]
		public Task<IActionResult> RemoveQueueItem([FromBody] ReleaseQueueItemRequest request, CancellationToken cancellationToken)
		{
			return _OperationExecuter.ExecuteAsync(_QueueOperations.RemoveQueueItemOperation, request, cancellationToken);
		}

		[HttpPost]
		public Task<IActionResult> GetQueueSize([FromBody] RequestPayload<string> request, CancellationToken cancellationToken)
		{
			return _OperationExecuter.ExecuteAsync(_QueueOperations.GetQueueSizeOperation, request.Data, cancellationToken);
		}

		[HttpPost]
		public Task<IActionResult> GetHeldQueueSize([FromBody] RequestPayload<string> request, CancellationToken cancellationToken)
		{
			return _OperationExecuter.ExecuteAsync(_QueueOperations.GetHeldQueueSizeOperation, request.Data, cancellationToken);
		}
	}
}
