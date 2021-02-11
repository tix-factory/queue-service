import generateHash from "./generateHash.js";

const countCacheExpiry = 250;

export default class {
	constructor(queueEntityFactory, queueItemsCollection) {
		this.queueEntityFactory = queueEntityFactory;
		this.queueItemsCollection = queueItemsCollection;
		this.countCache = {};
		this.heldCountCache = {};
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
		if (typeof(data) !== "string") {
			return Promise.reject("InvalidData");
		}

		const queueId = await this.queueEntityFactory.getOrCreateQueueIdByName(queueName);
		const deduplicationEnabled = await this.queueEntityFactory.isQueueDeduplicationEnabled(queueId);
		const holderId = generateHash();
		const currentTime = new Date();

		if (deduplicationEnabled) {
			const existingItem = await this.queueItemsCollection.findOne({
				queueId: queueId,
				data: data
			});

			if (existingItem) {
				return Promise.resolve(existingItem.id);
			}
		}

		return this.queueItemsCollection.insert({
			queueId: queueId,
			data: data,
			lockExpiration: currentTime,
			holderId: holderId
		});
	}

	async leaseQueueItem(queueName, expirationInMilliseconds) {
		const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
		if (!queueId) {
			return Promise.resolve(null);
		}

		const currentTime = new Date();
		const holderId = generateHash();
		const entity = await this.queueItemsCollection.findOneAndUpdate({
			queueId: queueId,
			lockExpiration: { "$lte": currentTime }
		}, {
			lockExpiration: new Date(currentTime.getTime() + expirationInMilliseconds),
			holderId: holderId
		});

		if (entity) {
			return Promise.resolve({
				ID: entity.id,
				QueueID: entity.queueId,
				Data: entity.data,
				HolderID: entity.holderId,
				LockExpiration: entity.lockExpiration,
				Updated: entity.updated,
				Created: entity.created
			});
		} else {
			return Promise.resolve(null);
		}
	}

	async releaseQueueItem(queueItemId, holderId) {
		if (typeof(queueItemId) !== "number") {
			return Promise.reject("InvalidLeaseHolder");
		}

		if (typeof(holderId) !== "string") {
			return Promise.reject("InvalidLeaseHolder");
		}

		const currentTime = new Date();
		return this.queueItemsCollection.updateOne({
			id: queueItemId,
			holderId: holderId,
			lockExpiration: { "$gt": currentTime }
		}, {
			lockExpiration: currentTime
		});
	}

	async deleteQueueItem(queueItemId, holderId) {
		if (typeof(queueItemId) !== "number") {
			return Promise.reject("InvalidLeaseHolder");
		}

		if (typeof(holderId) !== "string") {
			return Promise.reject("InvalidLeaseHolder");
		}

		const currentTime = new Date();
		return this.queueItemsCollection.deleteOne({
			id: queueItemId,
			holderId: holderId,
			lockExpiration: { "$gt": currentTime }
		});
	}

	async clearQueue(queueName) {
		const queueId = await this.queueEntityFactory.getQueueIdByName(queueName);
		if (!queueId) {
			return Promise.resolve(0);
		}

		return this.queueItemsCollection.deleteMany({
			queueId: queueId
		});
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

		const count = await this.queueItemsCollection.count({
			queueId: queueId
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

		const currentTime = new Date();
		const count = await this.queueItemsCollection.count({
			queueId: queueId,
			lockExpiration: { "$gt": currentTime }
		});

		this.heldCountCache[queueId] = count;
		setTimeout(() => {
			delete this.heldCountCache[queueId];
		}, countCacheExpiry);

		return Promise.resolve(count);
	}
};
