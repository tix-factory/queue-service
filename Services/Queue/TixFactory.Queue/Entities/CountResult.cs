using System.Runtime.Serialization;

namespace TixFactory.Queue.Entities
{
	[DataContract]
	internal class CountResult
	{
		[DataMember(Name = "Count")]
		public long Count { get; set; }
	}
}
