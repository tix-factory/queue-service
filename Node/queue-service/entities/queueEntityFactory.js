const CacheExpiry = 60 * 1000;
const MaxQueueNameLength = 128;
const ValidationRegex = /^\s+$/;

const isQueueNameValid = (queueName) => {
	if (typeof(queueName) !== "string") {
		return false;
	}

	return queueName && !ValidationRegex.test(queueName) && queueName.length < MaxQueueNameLength;
};

export default class {
	constructor(queuesCollection) {
		this.queuesCollection = queuesCollection;
		this.idByNameCache = {};
		this.deduplicationEnabled = {};
	}

	async setup() {
		await this.queuesCollection.createIndex({
			"name": 1
		}, {
			unique: true
		});
	}

	async getOrCreateQueueIdByName(queueName) {
		if (!isQueueNameValid(queueName)) {
			return Promise.reject("InvalidQueueName");
		}

		let queueId = await this.getQueueIdByName(queueName);
		if (queueId) {
			return Promise.resolve(queueId);
		}

		const cacheKey = queueName.toLowerCase();
		this.idByNameCache[cacheKey] = queueId = await this.queuesCollection.insert({
			name: queueName
		});

		return Promise.resolve(queueId);
	}

	async getQueueIdByName(queueName) {
		if (!isQueueNameValid(queueName)) {
			return Promise.resolve(null);
		}

		const cacheKey = queueName.toLowerCase();
		if (this.idByNameCache.hasOwnProperty(cacheKey)) {
			return Promise.resolve(this.idByNameCache[cacheKey]);
		}

		const queue = await this.queuesCollection.findOne({
			"name": queueName
		});
		
		const id = queue ? queue.id : null;
		if (id && queue.name.toLowerCase() !== cacheKey) {
			return Promise.reject(new Error(`collection returned wrong queue (expected ${queueName}, got ${queue.name})`));
		}

		this.idByNameCache[cacheKey] = id;
		if (!id) {
			setTimeout(() => {
				if (!this.idByNameCache[cacheKey]) {
					delete this.idByNameCache[cacheKey];
				}
			}, CacheExpiry);
		}

		return Promise.resolve(id);
	}

	async isQueueDeduplicationEnabled(queueId) {
		let deduplicationEnabled = this.deduplicationEnabled[queueId];
		if (typeof(deduplicationEnabled) === "boolean") {
			return Promise.resolve(deduplicationEnabled);
		}
		
		const queue = await this.queuesCollection.findOne({
			"id": queueId
		});

		let deduplicationEnabled = this.deduplicationEnabled[queueId] = queue.isQueueDeduplicationEnabled;
		setTimeout(() => {
			delete this.deduplicationEnabled[queueId];
		}, CacheExpiry);

		return Promise.resolve(deduplicationEnabled);
	}
};
