"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const dblp_preprocessor_1 = require("./dblp_preprocessor");
const weekly_arxiv_1 = require("./weekly_arxiv");
const weekly_arxiv_top_1 = require("./weekly_arxiv_top");
if (window.location.host == "dblp.org") {
    (0, dblp_preprocessor_1.preprocessDBLP)();
}
else if (window.location.pathname.indexOf("weekly_arxiv/") != -1) {
    (0, weekly_arxiv_1.preprocessWeeklyArxiv)();
}
else if (window.location.pathname.indexOf("weekly_arxiv_top") != -1) {
    console.log(window.location.pathname);
    (0, weekly_arxiv_top_1.preprocessWeeklyArxivTop)();
}
