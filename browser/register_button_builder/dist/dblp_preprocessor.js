"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.preprocessDBLP = void 0;
const node_request_1 = require("./node_request");
function createPaperCreateInfoArraySub(nodes) {
    const r = new Array();
    for (let i = 0; i < nodes.length; i++) {
        const node = nodes.item(i);
        if (node instanceof HTMLElement) {
            const info = createPaperElementInfo(node);
            if (info != null) {
                r.push(info);
            }
        }
    }
    return r;
}
function createPaperCreateInfoArray() {
    const r = new Array();
    createPaperCreateInfoArraySub(document.body.getElementsByClassName("entry inproceedings")).forEach((v) => {
        r.push(v);
    });
    createPaperCreateInfoArraySub(document.body.getElementsByClassName("entry informal")).forEach((v) => {
        r.push(v);
    });
    createPaperCreateInfoArraySub(document.body.getElementsByClassName("entry article")).forEach((v) => {
        r.push(v);
    });
    return r;
}
function createPaperElementInfo(node) {
    const aNodes = node.getElementsByTagName("a");
    let url = null;
    for (let i = 0; i < aNodes.length; i++) {
        const aNode = aNodes.item(i);
        if (aNode != null) {
            const hrefStr = aNode.getAttribute("href");
            if (hrefStr != null) {
                url = (0, node_request_1.getURLForNodeRequest)(hrefStr);
                if (url != null) {
                    break;
                }
            }
        }
    }
    if (url != null) {
        const xnode = { node: node, url: url };
        return xnode;
    }
    else {
        return null;
    }
}
function preprocessDBLP() {
    const paperCreateInfoArray = createPaperCreateInfoArray();
    const collections = (0, node_request_1.createCollections)(paperCreateInfoArray);
    collections.forEach((v) => {
        (0, node_request_1.processPaperElementInfoCollection)(v);
    });
}
exports.preprocessDBLP = preprocessDBLP;
