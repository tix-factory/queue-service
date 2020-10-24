namespace TixFactory.Queue.Service
{
	public interface ISettings : Configuration.ISettings
	{
		string QueueConnectionString { get; }
	}
}
