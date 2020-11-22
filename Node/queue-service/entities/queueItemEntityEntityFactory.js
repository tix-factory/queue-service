export default class {
	constructor(databaseConnection, queueEntityFactory) {
		this.databaseConnection = databaseConnection;
		this.queueEntityFactory = queueEntityFactory;
	}

	insertQueueItem(queueName, data) {
		return new Promise(async (resolve, reject) => {
			try {
				const queueId = await this.queueEntityFactory.getOrCreateQueueIdByName(queueName);
				const connection = await this.databaseConnection.getConnection();
				const queueItemId = await connection.executeInsertStoredProcedure("InsertQueueItem", {
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

	releaseQueueItem(queueItemId, holderId) {
		return new Promise(async (resolve, reject) => {
			try {
				const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
				if (!queueId) {
					resolve(false);
					return;
				}

				const connection = await this.databaseConnection.getConnection();
				const rowsModified = await connection.executeReadStoredProcedure("ReleaseQueueItem", {
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
				const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
				if (!queueId) {
					resolve(false);
					return;
				}

				const connection = await this.databaseConnection.getConnection();
				const rowsModified = await connection.executeReadStoredProcedure("DeleteQueueItem", {
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
		try {
			const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
			if (!queueId) {
				resolve(0);
				return;
			}

			const connection = await this.databaseConnection.getConnection();
			const rowsModified = await connection.executeReadStoredProcedure("ClearQueue", {
				_QueueID: queueId
			});

			resolve(rowsModified);
		} catch (e) {
			reject(e);
		}
	}

	getQueueSize(queueName) {
		try {
			const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
			if (!queueId) {
				resolve(0);
				return;
			}

			const connection = await this.databaseConnection.getConnection();
			const count = await connection.executeCountStoredProcedure("GetQueueSize", {
				_QueueID: queueId
			});

			resolve(count);
		} catch (e) {
			reject(e);
		}
	}

	getHeldQueueSize(queueName) {
		try {
			const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
			if (!queueId) {
				resolve(0);
				return;
			}

			const connection = await this.databaseConnection.getConnection();
			const count = await connection.executeCountStoredProcedure("GetHeldQueueSize", {
				_QueueID: queueId
			});

			resolve(count);
		} catch (e) {
			reject(e);
		}
	}
};
