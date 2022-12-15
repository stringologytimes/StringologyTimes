"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.preprocessWeeklyArxivTop = void 0;
function processButtonRequest(parent, button, wid) {
    let button_request = new XMLHttpRequest();
    button_request.open('GET', `http://lampin.sakura.ne.jp/cgi-bin/weekly_arxiv_check.cgi?mode=register&wid=${wid}`, true);
    button_request.onload = function () {
        const json = JSON.parse(this.responseText);
        const result = json["result"];
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
function createSpanTagForWID(info, checked) {
    const node = info.node;
    const span = document.createElement("span");
    const parent = node.parentElement;
    if (parent != null) {
        parent.insertBefore(span, node);
        node.remove();
        span.appendChild(node);
        if (!checked) {
            const button = document.createElement("button");
            button.style.marginRight = "50px";
            button.setAttribute("custom-wid", info.wid);
            button.textContent = "Regist";
            button.onclick = (e) => {
                processButtonRequest(span, button, info.wid);
            };
            span.insertBefore(button, node);
        }
        else {
            span.style.backgroundColor = "springgreen";
        }
    }
}
function getWIDInfoList() {
    const r = new Array();
    const nodes = document.body.getElementsByTagName("a");
    for (let i = 0; i < nodes.length; i++) {
        const node = nodes.item(i);
        if (node != null) {
            const url = node.getAttribute("href");
            if (url != null) {
                const factors = url.split("/");
                if (factors.length >= 2 && factors[factors.length - 2] == "weekly_arxiv") {
                    const lastFactor = factors[factors.length - 1];
                    const wid = lastFactor.split(".")[0];
                    const info = { wid: wid, node: node };
                    r.push(info);
                }
            }
        }
    }
    return r;
}
function initialize() {
    let request = new XMLHttpRequest();
    const get_url = `http://lampin.sakura.ne.jp/cgi-bin/weekly_arxiv_check.cgi?mode=check&wid=0`;
    request.open('GET', get_url, true);
    request.onload = function () {
        const json = JSON.parse(this.responseText);
        const result_list = json["result"];
        const wid_set = new Set();
        result_list.forEach((v) => {
            wid_set.add(`${v[0]}`);
        });
        const infList = getWIDInfoList();
        infList.forEach((v) => {
            createSpanTagForWID(v, wid_set.has(v.wid));
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
function preprocessWeeklyArxivTop() {
    initialize();
}
exports.preprocessWeeklyArxivTop = preprocessWeeklyArxivTop;
