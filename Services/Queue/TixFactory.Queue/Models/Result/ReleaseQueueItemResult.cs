namespace TixFactory.Queue
{
	public enum ReleaseQueueItemResult
	{
		Unknown = 0,
		Released = 1,
		Removed = 2,
		InvalidLeaseHolder = 3,
	}
}
