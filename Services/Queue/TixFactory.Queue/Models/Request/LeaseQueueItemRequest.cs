using System;
using System.Runtime.Serialization;

namespace TixFactory.Queue
{
	[DataContract]
	public class LeaseQueueItemRequest
	{
		[DataMember(Name = "queueName")]
		public string QueueName { get; set; }

		[DataMember(Name = "leaseExpiry")]
		public TimeSpan LeaseExpiry { get; set; }
	}
}
