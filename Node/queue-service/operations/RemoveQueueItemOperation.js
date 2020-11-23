import http from "@tix-factory/http";

export default class {
	constructor(queueItemEntityFactory) {
		this.queueItemEntityFactory = queueItemEntityFactory;
	}

    get allowAnonymous() {
        return false;
    }
 
    get name() {
        return "RemoveQueueItem";
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
				const removed = await this.queueItemEntityFactory.deleteQueueItem(requestBody.id, requestBody.leaseid);
				resolve(removed ? "Removed" : "InvalidLeaseHolder");
			} catch(e) {
				reject(e);
			}
        });
    }
};