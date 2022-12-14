"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.preprocessDBLP = void 0;
const node_request_1 = require("./node_request");
function addToDBLPElements(nodes, output) {
    for (let i = 0; i < nodes.length; i++) {
        const node = nodes.item(i);
        if (node instanceof HTMLElement) {
            output.push(node);
        }
    }
}
function createCollections(info_list) {
    const r = [];
    let x = new node_request_1.RequestCollection();
    info_list.forEach((v) => {
        if (x.collection.length > 50) {
            r.push(x);
            x = new node_request_1.RequestCollection();
        }
        x.collection.push(v);
    });
    r.push(x);
    return r;
}
function preprocessDBLP(dblpElements) {
    addToDBLPElements(document.body.getElementsByClassName("entry inproceedings"), dblpElements);
    addToDBLPElements(document.body.getElementsByClassName("entry informal"), dblpElements);
    addToDBLPElements(document.body.getElementsByClassName("entry article"), dblpElements);
    const dblpElementsWithURL = [];
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
            const xnode = { node: node, url: registURL };
            dblpElementsWithURL.push(xnode);
        }
    });
    const collections = createCollections(dblpElementsWithURL);
    collections.forEach((v) => {
        (0, node_request_1.node_request_X)(v);
    });
}
exports.preprocessDBLP = preprocessDBLP;
