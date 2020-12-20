import { httpMethods } from "@tix-factory/http";

const parseTimeSpan = (timespan) => {
	const match = timespan.match(/^(\d*)\.?(\d{2}):(\d{2}):(\d{2})\.?(\d*)$/);
	const days = Number(match[1] || 0);
	const hours = Number(match[2]);
	const minutes = Number(match[3]);
	const seconds = Number(match[4]);
	const milliseconds = Number(match[5] || 0);

	return (days * (24 * 60 * 60 * 1000))
		+ (hours * (60 * 60 * 1000))
		+ (minutes * (60 * 1000))
		+ (seconds * 1000)
		+ milliseconds;
};

export default class {
	constructor(queueItemEntityFactory) {
		this.queueItemEntityFactory = queueItemEntityFactory;
	}

    get allowAnonymous() {
        return false;
    }
 
    get name() {
        return "LeaseQueueItem";
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

		const expirationImMilliseconds = parseTimeSpan(requestBody.leaseexpiry);
		if (isNaN(expirationImMilliseconds)) {
			return Promise.reject("InvalidLeaseExpiry");
		}

		const queueItem = await this.queueItemEntityFactory.leaseQueueItem(requestBody.queuename, expirationImMilliseconds);
		if (!queueItem) {
			return Promise.resolve(null);
		}

		return Promise.resolve({
			id: queueItem.ID,
			data: queueItem.Data,
			leaseId: queueItem.HolderID
		});
    }
};