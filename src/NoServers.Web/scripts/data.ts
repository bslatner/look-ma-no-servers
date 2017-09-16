import * as api from "./api";
import * as util from "./util";

class BearerClient extends api.DefaultApi {
    constructor(token: string) {
        super();
        const oauth = new api.OAuth();
        oauth.accessToken = token;
        this.authentications = {
            "default": oauth
        };
    }
}

function getClient() {
    const token = localStorage.getItem("id_token");
    if (token) {
        return new BearerClient(token);
    }
    return new api.DefaultApi();
}

export function loadTitles() {
    return getClient().getTitles()
        .pipe((rsp) => {
            return rsp.body
                .sort((v1, v2) => { return util.compareStrings(v1.text, v2.text); });
        });
}

export function getActiveEvent() {
    return getClient().getActiveEvent()
        .pipe(evt => { return evt.body });
}

export function submitRegistration(registration: api.Registration) {
    return getClient().createRegistration(registration)
        .pipe(reg => { return reg.body });
}