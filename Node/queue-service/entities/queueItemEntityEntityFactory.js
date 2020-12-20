const countCacheExpiry = 250;

export default class {
	constructor(databaseConnection, queueEntityFactory) {
		this.databaseConnection = databaseConnection;
		this.queueEntityFactory = queueEntityFactory;
		this.countCache = {};
		this.heldCountCache = {};
	}

	async insertQueueItem(queueName, data) {
		const queueId = await this.queueEntityFactory.getOrCreateQueueIdByName(queueName);
		const queueItemId = await this.databaseConnection.executeInsertStoredProcedure("InsertQueueItem", {
			_QueueID: queueId,
			_Data: data
		});

		return Promise.resolve(queueItemId);
	}

	async leaseQueueItem(queueName, expirationInMilliseconds) {
		const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
		if (!queueId) {
			return Promise.resolve(null);
		}

		const currentDate = +new Date;
		const entities = await this.databaseConnection.executeReadStoredProcedure("LeaseQueueItem", {
			_QueueID: queueId,
			_LockExpiration: new Date(currentDate + expirationInMilliseconds)
		});

		return Promise.resolve(entities[0]);
	}

	async releaseQueueItem(queueItemId, holderId) {
		const rowsModified = await this.databaseConnection.executeWriteStoredProcedure("ReleaseQueueItem", {
			_ID: queueItemId,
			_HolderID: holderId
		});

		return Promise.resolve(rowsModified === 1);
	}

	async deleteQueueItem(queueItemId, holderId) {
		const rowsModified = await this.databaseConnection.executeWriteStoredProcedure("DeleteQueueItem", {
			_ID: queueItemId,
			_HolderID: holderId
		});

		return Promise.resolve(rowsModified === 1);
	}

	async clearQueue(queueName) {
		const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
		if (!queueId) {
			return Promise.resolve(0);
		}

		const rowsModified = await this.databaseConnection.executeWriteStoredProcedure("ClearQueue", {
			_QueueID: queueId
		});

		return Promise.resolve(rowsModified);
	}

	async getQueueSize(queueName) {
		const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
		if (!queueId) {
			return Promise.resolve(0);
		}

		const cachedCount = this.countCache[queueId];
		if (cachedCount) {
			return Promise.resolve(cachedCount);
		}

		const count = await this.databaseConnection.executeCountStoredProcedure("GetQueueSize", {
			_QueueID: queueId
		});

		this.countCache[queueId] = count;
		setTimeout(() => {
			delete this.countCache[queueId];
		}, countCacheExpiry);

		return Promise.resolve(count);
	}

	async getHeldQueueSize(queueName) {
		const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
		if (!queueId) {
			return Promise.resolve(0);
		}

		const cachedCount = this.heldCountCache[queueId];
		if (cachedCount) {
			return Promise.resolve(cachedCount);
		}

		const count = await this.databaseConnection.executeCountStoredProcedure("GetHeldQueueSize", {
			_QueueID: queueId
		});

		this.heldCountCache[queueId] = count;
		setTimeout(() => {
			delete this.heldCountCache[queueId];
		}, countCacheExpiry);

		return Promise.resolve(count);
	}
};
