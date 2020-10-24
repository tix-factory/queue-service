using System;
using Microsoft.AspNetCore.Mvc;
using TixFactory.Http.Service;

namespace TixFactory.Queue.Service.Controllers
{
	[Microsoft.AspNetCore.Components.Route("v1/[action]")]
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
		public IActionResult AddQueueItem([FromBody] AddQueueItemRequest request)
		{
			return _OperationExecuter.Execute(_QueueOperations.AddQueueItemOperation, request);
		}

		[HttpPost]
		public IActionResult ClearQueue([FromBody] RequestPayload<string> request)
		{
			return _OperationExecuter.Execute(_QueueOperations.ClearQueueOperation, request.Data);
		}
	}
}
