using System;
using System.Runtime.Serialization;

namespace TixFactory.Queue
{
	[DataContract]
	public class ReleaseQueueItemRequest
	{
		[DataMember(Name = "id")]
		public long Id { get; set; }

		[DataMember(Name = "leaseId")]
		public Guid LeaseId { get; set; }
	}
}
