export default class {
	constructor(databaseConnection, queueEntityFactory) {
		this.databaseConnection = databaseConnection;
		this.queueEntityFactory = queueEntityFactory;
	}

	insertQueueItem(queueName, data) {
		return new Promise(async (resolve, reject) => {
			try {
				const queueId = await this.queueEntityFactory.getOrCreateQueueIdByName(queueName);
				const queueItemId = await this.databaseConnection.executeInsertStoredProcedure("InsertQueueItem", {
					_QueueID: queueId,
					_Data: data
				});

				resolve(queueItemId);
			} catch (e) {
				reject(e);
			}
		});
	}

	leaseQueueItem(queueName, expirationInMilliseconds) {
		return new Promise(async (resolve, reject) => {
			try {
				const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
				if (!queueId) {
					resolve(null);
					return;
				}

				const currentDate = +new Date;
				const entities = await this.databaseConnection.executeReadStoredProcedure("LeaseQueueItem", {
					_QueueID: queueId,
					_LockExpiration: new Date(currentDate + expirationInMilliseconds)
				});

				resolve(entities[0]);
			} catch (e) {
				reject(e);
			}
		});
	}

	releaseQueueItem(queueItemId, holderId) {
		return new Promise(async (resolve, reject) => {
			try {
				const rowsModified = await this.databaseConnection.executeWriteStoredProcedure("ReleaseQueueItem", {
					_ID: queueItemId,
					_HolderID: holderId
				});

				resolve(rowsModified === 1);
			} catch (e) {
				reject(e);
			}
		});
	}

	deleteQueueItem(queueItemId, holderId) {
		return new Promise(async (resolve, reject) => {
			try {
				const rowsModified = await this.databaseConnection.executeWriteStoredProcedure("DeleteQueueItem", {
					_ID: queueItemId,
					_HolderID: holderId
				});

				resolve(rowsModified === 1);
			} catch (e) {
				reject(e);
			}
		});
	}

	clearQueue(queueName) {
		return new Promise(async (resolve, reject) => {
			try {
				const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
				if (!queueId) {
					resolve(0);
					return;
				}
	
				const rowsModified = await this.databaseConnection.executeWriteStoredProcedure("ClearQueue", {
					_QueueID: queueId
				});
	
				resolve(rowsModified);
			} catch (e) {
				reject(e);
			}
		});
	}

	getQueueSize(queueName) {
		return new Promise(async (resolve, reject) => {
			try {
				const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
				if (!queueId) {
					resolve(0);
					return;
				}

				const count = await this.databaseConnection.executeCountStoredProcedure("GetQueueSize", {
					_QueueID: queueId
				});

				resolve(count);
			} catch (e) {
				reject(e);
			}
		});
	}

	getHeldQueueSize(queueName) {
		return new Promise(async (resolve, reject) => {
			try {
				const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
				if (!queueId) {
					resolve(0);
					return;
				}

				const count = await this.databaseConnection.executeCountStoredProcedure("GetHeldQueueSize", {
					_QueueID: queueId
				});

				resolve(count);
			} catch (e) {
				reject(e);
			}
		});
	}
};
