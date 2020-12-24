import generateHash from "./generateHash.js";

const countCacheExpiry = 250;

export default class {
	constructor(databaseConnection, queueEntityFactory, queueItemsCollection, configurationClient) {
		this.databaseConnection = databaseConnection;
		this.queueEntityFactory = queueEntityFactory;
		this.configurationClient = configurationClient;
		this.queueItemsCollection = queueItemsCollection;
		this.countCache = {};
		this.heldCountCache = {};
	}

	async isMongoEnabled() {
		const value = await this.configurationClient.getSettingValue("MongoDBEnabled");
		return value === "true";
	}

	async setup() {
		await this.queueItemsCollection.createIndex({
			"holderId": 1
		}, {
			unique: true
		});

		await this.queueItemsCollection.createIndex({
			"queueId": 1,
			"lockExpiration": 1
		}, {});
	}

	async insertQueueItem(queueName, data) {
		const queueId = await this.queueEntityFactory.getOrCreateQueueIdByName(queueName);
		const isMongoEnabled = await this.isMongoEnabled();

		if (isMongoEnabled) {
			if (typeof(data) !== "string") {
				return Promise.reject("InvalidData");
			}
			
			const holderId = generateHash();
			const currentTime = new Date();
			return this.queueItemsCollection.insert({
				queueId: queueId,
				data: data,
				lockExpiration: currentTime,
				holderId: holderId
			});
		}

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

		const isMongoEnabled = await this.isMongoEnabled();
		if (isMongoEnabled) {
			return new Promise((resolve, reject) => {
				const currentTime = new Date();
				const holderId = generateHash();
				this.queueItemsCollection.findOneAndUpdate({
					queueId: queueId,
					lockExpiration: { "$lte": currentTime }
				}, {
					lockExpiration: new Date(currentTime.getTime() + expirationInMilliseconds),
					holderId: holderId
				}).then(entity => {
					if (entity) {
						resolve({
							ID: entity.id,
							QueueID: entity.queueId,
							Data: entity.data,
							HolderID: entity.holderId,
							LockExpiration: entity.lockExpiration,
							Updated: entity.updated,
							Created: entity.created
						});
					} else {
						resolve(null);
					}
				}).catch(reject);
			});
		}

		const currentDate = +new Date;
		const entities = await this.databaseConnection.executeReadStoredProcedure("LeaseQueueItem", {
			_QueueID: queueId,
			_LockExpiration: new Date(currentDate + expirationInMilliseconds)
		});

		return Promise.resolve(entities[0]);
	}

	async releaseQueueItem(queueItemId, holderId) {
		const isMongoEnabled = await this.isMongoEnabled();

		if (isMongoEnabled) {
			const currentTime = new Date();
			return this.queueItemsCollection.updateOne({
				id: queueItemId,
				holderId: holderId,
				lockExpiration: { "$gt": currentTime }
			}, {
				lockExpiration: currentTime
			});
		}

		const rowsModified = await this.databaseConnection.executeWriteStoredProcedure("ReleaseQueueItem", {
			_ID: queueItemId,
			_HolderID: holderId
		});

		return Promise.resolve(rowsModified === 1);
	}

	async deleteQueueItem(queueItemId, holderId) {
		const isMongoEnabled = await this.isMongoEnabled();

		if (isMongoEnabled) {
			const currentTime = new Date();
			return this.queueItemsCollection.deleteOne({
				id: queueItemId,
				holderId: holderId,
				lockExpiration: { "$gt": currentTime }
			});
		}

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

		const isMongoEnabled = await this.isMongoEnabled();
		if (isMongoEnabled) {
			return this.queueItemsCollection.deleteMany({
				queueId: queueId
			});
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
		if (typeof(cachedCount) === "number") {
			return Promise.resolve(cachedCount);
		}

		const isMongoEnabled = await this.isMongoEnabled();
		if (isMongoEnabled) {
			return this.queueItemsCollection.count({
				queueId: queueId
			});
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
		if (typeof(cachedCount) === "number") {
			return Promise.resolve(cachedCount);
		}

		const isMongoEnabled = await this.isMongoEnabled();
		if (isMongoEnabled) {
			const currentTime = new Date();
			return this.queueItemsCollection.count({
				queueId: queueId,
				lockExpiration: { "$gt": currentTime }
			});
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
