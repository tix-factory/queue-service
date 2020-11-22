const CacheExpiry = 60 * 1000;

export default class {
	constructor(databaseConnection) {
		this.databaseConnection = databaseConnection;
		this.idByNameCache = {};
	}

	getOrCreateQueueIdByName(queueName) {
		return new Promise(async (resolve, reject) => {
			try {
				let queueId = await this.getQueueIdByName(queueName);
				if (queueId) {
					resolve(queueId);
					return;
				}

				const cacheKey = queueName.toLowerCase();
				this.idByNameCache[cacheKey] = queueId = await this.executeInsertStoredProcedure("InsertQueue", {
					_Name = queueName
				});

				resolve(queueId);
			} catch (e) {
				reject(e);
			}
		});
	}

	getQueueIdByName(queueName) {
		return new Promise(async (resolve, reject) => {
			try {
				const cacheKey = queueName.toLowerCase();
				if (this.idByNameCache.hasOwnProperty(cacheKey)) {
					resolve(this.idByNameCache[cacheKey]);
					return;
				}

				const connection = await this.databaseConnection.getConnection();
				const entities = await connection.executeReadStoredProcedure("GetQueueByName", {
					_Name: queueName
				});

				const id = this.idByNameCache[cacheKey] = entities[0] ? entities[0].ID : null;
				if (!id) {
					setTimeout(() => {
						if (!this.idByNameCache[cacheKey]) {
							delete this.idByNameCache[cacheKey];
						}
					}, CacheExpiry);
				}

				resolve(id);
			} catch (e) {
				reject(e);
			}
		});
	}
};
