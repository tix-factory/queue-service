export default class {
	constructor(databaseConnection, queueEntityFactory) {
		this.databaseConnection = databaseConnection;
		this.queueEntityFactory = queueEntityFactory;
	}

	leaseQueueItem(queueName, expirationInMilliseconds) {
		return new Promise(async (resolve, reject) => {
			try {
				const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
				if (!queueId) {
					resolve(null);
					return;
				}

				const connection = await this.databaseConnection.getConnection();
				const currentDate = +new Date;
				const entities = await connection.executeReadStoredProcedure("LeaseQueueItem", {
					_QueueID: queueId,
					_LockExpiration: new Date(currentDate + expirationInMilliseconds)
				});

				resolve(entities[0]);
			} catch (e) {
				reject(e);
			}
		});
	}
};
