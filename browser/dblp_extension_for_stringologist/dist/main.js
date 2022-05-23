"use strict";
const dblpElements = [];
function addToDBLPElements(nodes, output) {
    for (let i = 0; i < nodes.length; i++) {
        const node = nodes.item(i);
        if (node instanceof HTMLElement) {
            dblpElements.push(node);
        }
    }
}
function node_request2(parent, button, url) {
    let button_request = new XMLHttpRequest();
    button_request.open('GET', `http://lampin.sakura.ne.jp/cgi-bin/paper_check.cgi?mode=register&url=${encodeURI(url)}`, true);
    button_request.onload = function () {
        const json = JSON.parse(this.responseText);
        if (json["result"] == "SUCCESS") {
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
function node_request(node, url) {
    let request = new XMLHttpRequest();
    request.open('GET', `http://lampin.sakura.ne.jp/cgi-bin/paper_check.cgi?mode=check&url=${encodeURI(url)}`, true);
    request.onload = function () {
        const json = JSON.parse(this.responseText);
        if (json["result"] == "NOT_REGISTRED") {
            const button = document.createElement("button");
            button.setAttribute("custom-url", url);
            button.textContent = "Regist";
            button.onclick = (e) => {
                node_request2(node, button, url);
            };
            node.appendChild(button);
        }
        else if (json["result"] == "DUPLICATION") {
            node.style.backgroundColor = "springgreen";
        }
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
addToDBLPElements(document.body.getElementsByClassName("entry inproceedings toc"), dblpElements);
addToDBLPElements(document.body.getElementsByClassName("entry informal toc"), dblpElements);
addToDBLPElements(document.body.getElementsByClassName("entry article toc"), dblpElements);
dblpElements.forEach((node) => {
    const aNodes = node.getElementsByTagName("a");
    let doi = null;
    let arXivURL = null;
    for (let i = 0; i < aNodes.length; i++) {
        const aNode = aNodes.item(i);
        if (aNode != null) {
            const hrefStr = aNode.getAttribute("href");
            if (hrefStr != null) {
                if (hrefStr.indexOf("https://doi.org/") == 0) {
                    doi = hrefStr.substring("https://doi.org/".length);
                    break;
                }
                else if (hrefStr.indexOf("https://arxiv.org/") == 0) {
                    arXivURL = hrefStr;
                    break;
                }
                else if (hrefStr.indexOf("http://arxiv.org/") == 0) {
                    arXivURL = hrefStr.replace("http", "https");
                    break;
                }
            }
        }
    }
    let registURL = null;
    if (doi != null) {
        registURL = doi;
    }
    if (arXivURL != null) {
        registURL = arXivURL;
    }
    if (registURL != null) {
        node_request(node, registURL);
    }
});
//# sourceMappingURL=main.js.map