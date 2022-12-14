"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const dblp_preprocessor_1 = require("./dblp_preprocessor");
const stringology_times_processor_1 = require("./stringology_times_processor");
if (window.location.host == "dblp.org") {
    (0, dblp_preprocessor_1.preprocessDBLP)();
}
else if (window.location.pathname.indexOf("weekly_arxiv") != -1) {
    (0, stringology_times_processor_1.preprocessStringologyTimes)();
}
