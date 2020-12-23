import { dirname } from "path";
import { fileURLToPath } from 'url';
import { HttpServer } from "@tix-factory/http-service";
import { ConfiguredConnection } from "@tix-factory/mysql-data";
import { ConfigurationClient } from "@tix-factory/configuration-client";
import MongoDB from "mongodb";
import QueueEntityFactory from "./entities/queueEntityFactory.js";
import QueueItemEntityFactory from "./entities/queueItemEntityEntityFactory.js";

import AddQueueItemOperation from "./operations/AddQueueItemOperation.js";
import ClearQueueOperation from "./operations/ClearQueueOperation.js";
import GetQueueSizeOperation from "./operations/GetQueueSizeOperation.js";
import GetHeldQueueSizeOperation from "./operations/GetHeldQueueSizeOperation.js";
import LeaseQueueItemOperation from "./operations/LeaseQueueItemOperation.js";
import ReleaseQueueItemOperation from "./operations/ReleaseQueueItemOperation.js";
import RemoveQueueItemOperation from "./operations/RemoveQueueItemOperation.js";

const workingDirectory = dirname(fileURLToPath(import.meta.url));

const service = new HttpServer({
    name: "TixFactory.Queue.Service",
    logName: "TFQS2.TixFactory.Queue.Service"
});

const createMongoConnection = (configurationClient, projectName) => {
	return new Promise(async (resolve, reject) => {
		try {
			const mongoConnectionString = await configurationClient.getSettingValue("MongoDBConnectionString");
			MongoDB.MongoClient.connect(mongoConnectionString, (err, connection) => {
				if (err) {
					reject(err);
				} else {
					const database = connection.db(projectName);
					resolve(database);
				}
			});
		} catch (e) {
			reject(e);
		}
	});
};

const init = () => {
	console.log(`Starting ${service.options.name}...\n\tWorking directory: ${workingDirectory}\n\tNODE_ENV: ${process.env.NODE_ENV}\n\tPort: ${service.options.port}`);

	return new Promise(async (resolve, reject) => {
		try {
			const configurationClient = new ConfigurationClient(service.httpClient, service.logger, {
				maxDatabaseConnections: 25
			});

			const maxConnections = await configurationClient.getSettingValue("maxDatabaseConnections");

			const configuredConnection = new ConfiguredConnection(configurationClient, service.logger, "QueueConnectionString", {
				maxConnections: maxConnections,
				sslCertificateFileName: `${workingDirectory}/db-certificate.crt`
			});

			const mongoConnection = await createMongoConnection(configurationClient, "queue-service");

			const databaseConnection = await configuredConnection.getConnection();
			const queueEntityFactory = new QueueEntityFactory(databaseConnection);
			const queueItemEntityFactory = new QueueItemEntityFactory(databaseConnection, queueEntityFactory, mongoConnection, configurationClient);

			await queueItemEntityFactory.setup();
			
			service.operationRegistry.registerOperation(new AddQueueItemOperation(queueItemEntityFactory));
			service.operationRegistry.registerOperation(new ClearQueueOperation(queueItemEntityFactory));
			service.operationRegistry.registerOperation(new GetQueueSizeOperation(queueItemEntityFactory, service.promClient, service.logger));
			service.operationRegistry.registerOperation(new GetHeldQueueSizeOperation(queueItemEntityFactory));
			service.operationRegistry.registerOperation(new LeaseQueueItemOperation(queueItemEntityFactory));
			service.operationRegistry.registerOperation(new ReleaseQueueItemOperation(queueItemEntityFactory));
			service.operationRegistry.registerOperation(new RemoveQueueItemOperation(queueItemEntityFactory));

			resolve();
		} catch (e) {
			reject(e);
		}
	});
};

init().then(() => {
	service.listen();
}).catch(err => {
	service.logger.error(err);
	console.error(err);
	process.exit(1);
});
