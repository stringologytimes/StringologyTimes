"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.node_request_X = exports.node_request2 = exports.RequestCollection = void 0;
class RequestCollection {
    constructor() {
        this.collection = [];
    }
}
exports.RequestCollection = RequestCollection;
function node_request2(parent, button, url) {
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
exports.node_request2 = node_request2;
function node_request_X(info_collection) {
    const url_parameter = info_collection.collection.map((v) => v.url).join(",");
    let request = new XMLHttpRequest();
    const get_url = `http://lampin.sakura.ne.jp/cgi-bin/paper_check.cgi?mode=check&url=${encodeURI(url_parameter)}`;
    request.open('GET', get_url, true);
    request.onload = function () {
        const json = JSON.parse(this.responseText);
        const result_list = json["result"];
        result_list.forEach((v, i) => {
            const check_result = v[1];
            const node = info_collection.collection[i].node;
            const url = info_collection.collection[i].url;
            if (check_result == "NOT_REGISTRED") {
                const button = document.createElement("button");
                button.setAttribute("custom-url", url);
                button.textContent = "Regist";
                button.onclick = (e) => {
                    node_request2(node, button, url);
                };
                node.appendChild(button);
            }
            else if (check_result == "DUPLICATION") {
                node.style.backgroundColor = "springgreen";
            }
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
exports.node_request_X = node_request_X;
