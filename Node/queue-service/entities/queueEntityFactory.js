export default class {
	constructor(databaseConnection) {
		this.databaseConnection = databaseConnection;
	}

	getQueueIdByName(queueName) {
		return new Promise(async (resolve, reject) => {
			try {
				const connection = await this.databaseConnection.getConnection();
				const entities = await connection.executeReadStoredProcedure("GetQueueByName", {
					_Name: queueName
				});

				resolve(entities[0] ? entities[0].ID : null);
			} catch (e) {
				reject(e);
			}
		});
	}
};
