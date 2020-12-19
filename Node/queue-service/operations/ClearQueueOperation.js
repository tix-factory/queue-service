import { httpMethods } from "@tix-factory/http";

export default class {
	constructor(queueItemEntityFactory) {
		this.queueItemEntityFactory = queueItemEntityFactory;
	}

    get allowAnonymous() {
        return false;
    }
 
    get name() {
        return "ClearQueue";
    }
 
    get route() {
        return `/v1/${this.name}`;
    }
 
    get method() {
        return httpMethods.post;
    }
 
    execute(requestBody) {
		for (let key in requestBody) {
			if (requestBody.hasOwnProperty(key)) {
				requestBody[key.toLowerCase()] = requestBody[key];
			}
		}

        return new Promise(async (resolve, reject) => {
			try {
				const count = await this.queueItemEntityFactory.clearQueue(requestBody.data);
				resolve(count);
			} catch(e) {
				reject(e);
			}
        });
    }
};