import { httpMethods } from "@tix-factory/http";

export default class {
	constructor(queueItemEntityFactory, promClient, logger) {
		this.queueItemEntityFactory = queueItemEntityFactory;
		this.logger = logger;

		this.sizeCounter = new promClient.Gauge({
			name: "queue_size",
			help: "Number of items in a queue.",
			labelNames: ["queueName"]
		});
	}

    get allowAnonymous() {
        return false;
    }
 
    get name() {
        return "GetQueueSize";
    }
 
    get route() {
        return `/v1/${this.name}`;
    }
 
    get method() {
        return httpMethods.post;
    }
 
    async execute(requestBody) {
		for (let key in requestBody) {
			if (requestBody.hasOwnProperty(key)) {
				requestBody[key.toLowerCase()] = requestBody[key];
			}
		}

		const count = await this.queueItemEntityFactory.getQueueSize(requestBody.data);
		this.recordSize(requestBody.data, count);

		return Promise.resolve(count);
	}
	
	recordSize(queueName, count) {
		try {
			this.sizeCounter.set({
				queueName: queueName
			}, count);
		} catch (e) {
			this.logger.warn(`Failed to record queue size.\n\tQueue name: ${queueName}\n\tSize: ${count}\n`, e);
		}
	}
};