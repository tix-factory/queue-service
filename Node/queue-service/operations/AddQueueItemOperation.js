import http from "@tix-factory/http";
const MaxQueueNameLength = 128;
const MaxQueueDataLength = 32786;
const ValidationRegex = /^\s+$/;

export default class {
	constructor(queueItemEntityFactory) {
		this.queueItemEntityFactory = queueItemEntityFactory;
	}

    get allowAnonymous() {
        return false;
    }
 
    get name() {
        return "AddQueueItem";
    }
 
    get route() {
        return `/v1/${this.name}`;
    }
 
    get method() {
        return http.methods.post;
    }
 
    execute(requestBody) {
		for (let key in requestBody) {
			if (requestBody.hasOwnProperty(key)) {
				requestBody[key.toLowerCase()] = requestBody[key];
			}
		}

        return new Promise(async (resolve, reject) => {
			try {
				const queueName = requestBody.queuename;
				if (!queueName || ValidationRegex.test(queueName) || queueName.length > MaxQueueNameLength) {
					reject("InvalidQueueName");
					return;
				}

				const queueData = requestBody.data;
				if (!queueData || ValidationRegex.test(queueData) || queueData.length > MaxQueueDataLength) {
					reject("InvalidData");
					return;
				}

				const queueItemId = await this.queueItemEntityFactory.insertQueueItem(queueName, queueData);
				resolve(queueItemId);
			} catch(e) {
				reject(e);
			}
        });
    }
};