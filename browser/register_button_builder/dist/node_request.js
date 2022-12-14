"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.processPaperElementInfoCollection = exports.processPaperElementInfo = exports.getURLForNodeRequest = exports.processPaperElementInfoWithATag = exports.processPaperElementInfoWithDivTag = exports.processPaperElementInfoCollectionSub = exports.createCollections = exports.PaperElementInfoCollection = void 0;
class PaperElementInfoCollection {
    constructor() {
        this.collection = [];
    }
}
exports.PaperElementInfoCollection = PaperElementInfoCollection;
function createCollections(info_list) {
    const r = [];
    let x = new PaperElementInfoCollection();
    info_list.forEach((v) => {
        if (x.collection.length > 50) {
            r.push(x);
            x = new PaperElementInfoCollection();
        }
        x.collection.push(v);
    });
    r.push(x);
    return r;
}
exports.createCollections = createCollections;
function processPaperElementInfoCollectionSub(parent, button, url) {
    let button_request = new XMLHttpRequest();
    button_request.open('GET', `http://lampin.sakura.ne.jp/cgi-bin/paper_check.cgi?mode=register&url=${encodeURI(url)}`, true);
    button_request.onload = function () {
        const json = JSON.parse(this.responseText);
        const json_line = json["result"][0];
        const result = json_line[1];
        if (result == "SUCCESS") {
            button.textContent = "OK";
            button.onclick = null;
            parent.style.backgroundColor = "springgreen";
        }
        else {
            button.textContent = "ERROR";
            button.onclick = null;
            parent.style.backgroundColor = "red";
        }
    };
    button_request.send();
}
exports.processPaperElementInfoCollectionSub = processPaperElementInfoCollectionSub;
function processPaperElementInfoWithDivTag(info, check_result) {
    const node = info.node;
    const url = info.url;
    if (check_result == "NOT_REGISTRED") {
        const button = document.createElement("button");
        button.setAttribute("custom-url", url);
        button.textContent = "Regist";
        button.onclick = (e) => {
            processPaperElementInfoCollectionSub(node, button, url);
        };
        node.appendChild(button);
    }
    else if (check_result == "DUPLICATION") {
        node.style.backgroundColor = "springgreen";
    }
}
exports.processPaperElementInfoWithDivTag = processPaperElementInfoWithDivTag;
function processPaperElementInfoWithATag(info, check_result) {
    const node = info.node;
    const url = info.url;
    const span = document.createElement("span");
    span.setAttribute("data-result", check_result);
    span.setAttribute("data-url", info.url);
    const parent = node.parentElement;
    if (parent != null) {
        parent.insertBefore(span, node);
        node.remove();
        span.appendChild(node);
        if (check_result == "NOT_REGISTRED") {
            const button = document.createElement("button");
            button.style.marginRight = "50px";
            button.setAttribute("custom-url", url);
            button.textContent = "Regist";
            button.onclick = (e) => {
                processPaperElementInfoCollectionSub(span, button, url);
            };
            span.insertBefore(button, node);
        }
        else if (check_result == "DUPLICATION") {
            span.style.backgroundColor = "springgreen";
        }
    }
}
exports.processPaperElementInfoWithATag = processPaperElementInfoWithATag;
function getURLForNodeRequest(url) {
    let doi = null;
    let arXivURL = null;
    if (url.indexOf("https://doi.org/10.48550/arXiv.") == 0) {
        const psuf = url.substring("https://doi.org/10.48550/arXiv.".length);
        arXivURL = "https://arxiv.org/abs/" + psuf;
    }
    else if (url.indexOf("https://doi.org/") == 0) {
        doi = url.substring("https://doi.org/".length);
    }
    else if (url.indexOf("https://arxiv.org/") == 0) {
        arXivURL = url;
    }
    else if (url.indexOf("http://arxiv.org/") == 0) {
        arXivURL = url.replace("http", "https");
    }
    let registURL = null;
    if (doi != null) {
        registURL = doi;
    }
    if (arXivURL != null) {
        registURL = arXivURL;
    }
    if (registURL != null) {
        return registURL;
    }
    else {
        return null;
    }
}
exports.getURLForNodeRequest = getURLForNodeRequest;
function processPaperElementInfo(info, check_result) {
    if (info.node.tagName == "A") {
        processPaperElementInfoWithATag(info, check_result);
    }
    else {
        processPaperElementInfoWithDivTag(info, check_result);
    }
}
exports.processPaperElementInfo = processPaperElementInfo;
function processPaperElementInfoCollection(info_collection) {
    const url_parameter = info_collection.collection.map((v) => v.url).join(",");
    let request = new XMLHttpRequest();
    const get_url = `http://lampin.sakura.ne.jp/cgi-bin/paper_check.cgi?mode=check&url=${encodeURI(url_parameter)}`;
    request.open('GET', get_url, true);
    request.onload = function () {
        const json = JSON.parse(this.responseText);
        const result_list = json["result"];
        result_list.forEach((v, i) => {
            const check_result = v[1];
            processPaperElementInfo(info_collection.collection[i], check_result);
        });
    };
    request.onerror = function () {
        console.log("error");
    };
    request.onabort = function () {
        console.log("abort");
    };
    request.ontimeout = function () {
        console.log("timeout");
    };
    request.send();
}
exports.processPaperElementInfoCollection = processPaperElementInfoCollection;
