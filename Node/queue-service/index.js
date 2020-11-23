import { dirname } from "path";
import { fileURLToPath } from 'url';
import http from "@tix-factory/http";
import httpService from "@tix-factory/http-service";
import mysql from "@tix-factory/mysql-data";
import configurationClientModule from "@tix-factory/configuration-client";
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
const serviceOptions = {
    name: "TixFactory.Queue.Service",
    logName: "TFQS2.TixFactory.Queue.Service"
};

const httpClient = new http.client();
const operationRegistry = new httpService.operationRegistry([]);
const service = new httpService.server(httpClient, operationRegistry, serviceOptions);

console.log(`Starting ${serviceOptions.name}...\n\tWorking directory: ${workingDirectory}`);

const configurationClient = new configurationClientModule.configurationClient(httpClient, service.logger, {});
const databaseConnection = new mysql.ConfiguredConnection(configurationClient, service.logger, "QueueConnectionString", `${workingDirectory}/db-certificate.crt`);
const queueEntityFactory = new QueueEntityFactory(databaseConnection);
const queueItemEntityFactory = new QueueItemEntityFactory(databaseConnection, queueEntityFactory);

operationRegistry.registerOperation(new AddQueueItemOperation(queueItemEntityFactory));
operationRegistry.registerOperation(new ClearQueueOperation(queueItemEntityFactory));
operationRegistry.registerOperation(new GetQueueSizeOperation(queueItemEntityFactory));
operationRegistry.registerOperation(new GetHeldQueueSizeOperation(queueItemEntityFactory));
operationRegistry.registerOperation(new LeaseQueueItemOperation(queueItemEntityFactory));
operationRegistry.registerOperation(new ReleaseQueueItemOperation(queueItemEntityFactory));
operationRegistry.registerOperation(new RemoveQueueItemOperation(queueItemEntityFactory));
