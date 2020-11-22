import { dirname } from "path";
import { fileURLToPath } from 'url';
import http from "@tix-factory/http";
import httpService from "@tix-factory/http-service";
import configurationClientModule from "@tix-factory/configuration-client";
import DatabaseConnection from "./entities/databaseConnection.js";
import QueueEntityFactory from "./entities/queueEntityFactory.js";
import QueueItemEntityFactory from "./entities/queueItemEntityEntityFactory.js";
import LeaseQueueItemOperation from "./operations/LeaseQueueItemOperation.js";

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
const databaseConnection = new DatabaseConnection(configurationClient, service.logger, "QueueConnectionString", `${workingDirectory}/db-certificate.crt`);
const queueEntityFactory = new QueueEntityFactory(databaseConnection);
const queueItemEntityFactory = new QueueItemEntityFactory(databaseConnection, queueEntityFactory);

operationRegistry.registerOperation(new LeaseQueueItemOperation(queueItemEntityFactory));
