"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.preprocessWeeklyArxiv = void 0;
const node_request_1 = require("./node_request");
function createPaperCreateInfoArray() {
    const r = new Array();
    const nodes = document.body.getElementsByTagName("a");
    for (let i = 0; i < nodes.length; i++) {
        const node = nodes.item(i);
        if (node != null) {
            const url = node.getAttribute("href");
            if (url != null) {
                const regularURL = (0, node_request_1.getURLForNodeRequest)(url);
                if (regularURL != null) {
                    const info = { url: regularURL, node: node };
                    r.push(info);
                }
            }
        }
    }
    return r;
}
function preprocessWeeklyArxiv() {
    const paperCreateInfoArray = createPaperCreateInfoArray();
    const collections = (0, node_request_1.createCollections)(paperCreateInfoArray);
    collections.forEach((v) => {
        (0, node_request_1.processPaperElementInfoCollection)(v);
    });
}
exports.preprocessWeeklyArxiv = preprocessWeeklyArxiv;
