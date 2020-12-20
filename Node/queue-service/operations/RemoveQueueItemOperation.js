import { httpMethods } from "@tix-factory/http";

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
        return httpMethods.post;
    }
 
    async execute(requestBody) {
		for (let key in requestBody) {
			if (requestBody.hasOwnProperty(key)) {
				requestBody[key.toLowerCase()] = requestBody[key];
			}
		}

		const removed = await this.queueItemEntityFactory.deleteQueueItem(requestBody.id, requestBody.leaseid);
		return Promise.resolve(removed ? "Removed" : "InvalidLeaseHolder");
    }
};