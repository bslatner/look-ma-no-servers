import * as $ from "jquery";
import * as auth0 from "auth0-js";
import * as dom from "./dom";

const auth0Config = {
    clientId: "zeYGV86kHrWAetV2SUVQxgtA7JFjdBAs",
    domain: "sct.auth0.com",
    callbackUrl: window.location.href
};

const webAuth = new auth0.WebAuth({
    domain: auth0Config.domain,
    clientID: auth0Config.clientId,
    redirectUri: auth0Config.callbackUrl,
    audience: `https://${auth0Config.domain}/userinfo`,
    responseType: "token id_token",
    scope: "openid",
    leeway: 60
});

const loginLink = $("#login-link");
const logoutLink = $("#logout-link");
const adminLink = $("#admin-link");
const registrationForm = $("#registration-form");
const loginMessage = $("#login-message");

function isAuthenticated() {
    // Check whether the current time is past the
    // access token's expiry time
    const expiresAt = JSON.parse(localStorage.getItem("expires_at"));
    return new Date().getTime() < expiresAt;
}

function setSession(authResult: any) {
    // Set the time that the access token will expire at
    const expiresAt = JSON.stringify(authResult.expiresIn * 1000 + new Date().getTime());
    localStorage.setItem("access_token", authResult.accessToken);
    localStorage.setItem("id_token", authResult.idToken);
    localStorage.setItem("expires_at", expiresAt);
}

function hide(items: JQuery[]) {
    items.forEach(i => {
        i.addClass(dom.classNames.hiddenItem);
    });
}

function show(items: JQuery[]) {
    items.forEach(i => {
        i.removeClass(dom.classNames.hiddenItem);
    });
}

function updateDisplay() {
    if (isAuthenticated()) {
        hide([loginLink, loginMessage]);
        show([logoutLink, adminLink, registrationForm]);
    } else {
        hide([logoutLink, adminLink, registrationForm]);
        show([loginLink, loginMessage]);
    }
}

function logout() {
    // Remove tokens and expiry time from localStorage
    localStorage.removeItem("access_token");
    localStorage.removeItem("id_token");
    localStorage.removeItem("expires_at");
    updateDisplay();
}

loginLink.find("a").click((e) => {
    e.preventDefault();
    webAuth.authorize({});
});

logoutLink.find("a").click((e) => {
    e.preventDefault();
    logout();
});

function parseQuery(qstr: string) {
    const query = {};
    const a = (qstr[0] === "?" ? qstr.substr(1) : qstr).split("&");
    for (var i = 0; i < a.length; i++) {
        const b = a[i].split("=");
        query[decodeURIComponent(b[0])] = decodeURIComponent(b[1] || "");
    }
    return query;
}

function parseToken(token: string) {
    if (token) {
        const parts = token.split(".");
        if (parts.length !== 3) {
            throw new Error("Cannot decode a malformed JWT");
        }
        return JSON.parse(atob(parts[1]));
    }
    return {};
}

export function handleAuthentication() {
    // hack?
    //
    // The auth0 example calls parseHash with just the handler, not with any options.
    // For some reason, if you don't do it this way, you sometimes get errors validating
    // the "nonce" field.
    const hash = window.location.hash;
    const token = parseToken(parseQuery(hash)["id_token"]);
    const nonce = token.nonce || "";
    webAuth.parseHash({
        hash: window.location.hash,
        nonce: nonce
    }, (err, authResult) => {
        if (authResult && authResult.accessToken && authResult.idToken) {
            window.location.hash = "";
            setSession(authResult);
        } else if (err) {
            alert(`Error: ${err.error}`);
        }
        updateDisplay();
    });
}