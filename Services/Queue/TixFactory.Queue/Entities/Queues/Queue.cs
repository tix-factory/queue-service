using System;
using System.Runtime.Serialization;

namespace TixFactory.Queue.Entities
{
	[DataContract(Name = "queues")]
	internal class Queue
	{
		[DataMember(Name = "ID")]
		public long Id { get; set; }

		[DataMember(Name = "Name")]
		public string Name { get; set; }

		[DataMember(Name = "Updated")]
		public DateTime Updated { get; set; }

		[DataMember(Name = "Created")]
		public DateTime Created { get; set; }
	}
}