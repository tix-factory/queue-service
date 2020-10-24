using System;
using System.Runtime.Serialization;

namespace TixFactory.Queue.Entities
{
	[DataContract(Name = "queue-items")]
	internal class QueueItem
	{
		[DataMember(Name = "ID")]
		public long Id { get; set; }

		[DataMember(Name = "QueueID")]
		public long QueueId { get; set; }

		[DataMember(Name = "Data")]
		public string Data { get; set; }

		[DataMember(Name = "HolderID")]
		public Guid? HolderId { get; set; }

		[DataMember(Name = "LockExpiration")]
		public DateTime LockExpiration { get; set; }

		[DataMember(Name = "Updated")]
		public DateTime Updated { get; set; }

		[DataMember(Name = "Created")]
		public DateTime Created { get; set; }
	}
}