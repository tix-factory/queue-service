using System.Runtime.Serialization;

namespace TixFactory.Queue
{
	[DataContract]
	public class AddQueueItemRequest
	{
		[DataMember(Name = "queueName")]
		public string QueueName { get; set; }

		[DataMember(Name = "data")]
		public string Data { get; set; }
	}
}
