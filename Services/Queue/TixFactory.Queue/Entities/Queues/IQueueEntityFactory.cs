namespace TixFactory.Queue.Entities
{
	internal interface IQueueEntityFactory
	{
		Queue GetOrCreateQueue(string name);

		Queue GetQueueByName(string name);

		void DeleteQueue(long id);
	}
}
