/*
 * ATTENTION: The "eval" devtool has been used (maybe by default in mode: "development").
 * This devtool is neither made for production nor for readable output files.
 * It uses "eval()" calls to create a separate source file in the browser devtools.
 * If you are trying to read the output file, select a different devtool (https://webpack.js.org/configuration/devtool/)
 * or disable the default devtool with "devtool: false".
 * If you are looking for production-ready output files, see mode: "production" (https://webpack.js.org/configuration/mode/).
 */
var DBLPExtension;
/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	var __webpack_modules__ = ({

/***/ "./src/dblp_preprocessor.ts":
/*!**********************************!*\
  !*** ./src/dblp_preprocessor.ts ***!
  \**********************************/
/***/ ((__unused_webpack_module, exports, __webpack_require__) => {

eval("\r\nObject.defineProperty(exports, \"__esModule\", ({ value: true }));\r\nexports.preprocessDBLP = void 0;\r\nconst node_request_1 = __webpack_require__(/*! ./node_request */ \"./src/node_request.ts\");\r\nfunction createPaperCreateInfoArraySub(nodes) {\r\n    const r = new Array();\r\n    for (let i = 0; i < nodes.length; i++) {\r\n        const node = nodes.item(i);\r\n        if (node instanceof HTMLElement) {\r\n            const info = createPaperElementInfo(node);\r\n            if (info != null) {\r\n                r.push(info);\r\n            }\r\n        }\r\n    }\r\n    return r;\r\n}\r\nfunction createPaperCreateInfoArray() {\r\n    const r = new Array();\r\n    createPaperCreateInfoArraySub(document.body.getElementsByClassName(\"entry inproceedings\")).forEach((v) => {\r\n        r.push(v);\r\n    });\r\n    createPaperCreateInfoArraySub(document.body.getElementsByClassName(\"entry informal\")).forEach((v) => {\r\n        r.push(v);\r\n    });\r\n    createPaperCreateInfoArraySub(document.body.getElementsByClassName(\"entry article\")).forEach((v) => {\r\n        r.push(v);\r\n    });\r\n    return r;\r\n}\r\nfunction createPaperElementInfo(node) {\r\n    const aNodes = node.getElementsByTagName(\"a\");\r\n    let url = null;\r\n    for (let i = 0; i < aNodes.length; i++) {\r\n        const aNode = aNodes.item(i);\r\n        if (aNode != null) {\r\n            const hrefStr = aNode.getAttribute(\"href\");\r\n            if (hrefStr != null) {\r\n                url = (0, node_request_1.getURLForNodeRequest)(hrefStr);\r\n                if (url != null) {\r\n                    break;\r\n                }\r\n            }\r\n        }\r\n    }\r\n    if (url != null) {\r\n        const xnode = { node: node, url: url };\r\n        return xnode;\r\n    }\r\n    else {\r\n        return null;\r\n    }\r\n}\r\nfunction preprocessDBLP() {\r\n    const paperCreateInfoArray = createPaperCreateInfoArray();\r\n    const collections = (0, node_request_1.createCollections)(paperCreateInfoArray);\r\n    collections.forEach((v) => {\r\n        (0, node_request_1.processPaperElementInfoCollection)(v);\r\n    });\r\n}\r\nexports.preprocessDBLP = preprocessDBLP;\r\n\n\n//# sourceURL=webpack://DBLPExtension/./src/dblp_preprocessor.ts?");

/***/ }),

/***/ "./src/index.ts":
/*!**********************!*\
  !*** ./src/index.ts ***!
  \**********************/
/***/ ((__unused_webpack_module, exports, __webpack_require__) => {

eval("\r\nObject.defineProperty(exports, \"__esModule\", ({ value: true }));\r\nconst dblp_preprocessor_1 = __webpack_require__(/*! ./dblp_preprocessor */ \"./src/dblp_preprocessor.ts\");\r\nconst stringology_times_processor_1 = __webpack_require__(/*! ./stringology_times_processor */ \"./src/stringology_times_processor.ts\");\r\nif (window.location.host == \"dblp.org\") {\r\n    (0, dblp_preprocessor_1.preprocessDBLP)();\r\n}\r\nelse if (window.location.pathname.indexOf(\"weekly_arxiv\") != -1) {\r\n    (0, stringology_times_processor_1.preprocessStringologyTimes)();\r\n}\r\n\n\n//# sourceURL=webpack://DBLPExtension/./src/index.ts?");

/***/ }),

/***/ "./src/node_request.ts":
/*!*****************************!*\
  !*** ./src/node_request.ts ***!
  \*****************************/
/***/ ((__unused_webpack_module, exports) => {

eval("\r\nObject.defineProperty(exports, \"__esModule\", ({ value: true }));\r\nexports.processPaperElementInfoCollection = exports.processPaperElementInfo = exports.getURLForNodeRequest = exports.processPaperElementInfoWithATag = exports.processPaperElementInfoWithDivTag = exports.processPaperElementInfoCollectionSub = exports.createCollections = exports.PaperElementInfoCollection = void 0;\r\nclass PaperElementInfoCollection {\r\n    constructor() {\r\n        this.collection = [];\r\n    }\r\n}\r\nexports.PaperElementInfoCollection = PaperElementInfoCollection;\r\nfunction createCollections(info_list) {\r\n    const r = [];\r\n    let x = new PaperElementInfoCollection();\r\n    info_list.forEach((v) => {\r\n        if (x.collection.length > 50) {\r\n            r.push(x);\r\n            x = new PaperElementInfoCollection();\r\n        }\r\n        x.collection.push(v);\r\n    });\r\n    r.push(x);\r\n    return r;\r\n}\r\nexports.createCollections = createCollections;\r\nfunction processPaperElementInfoCollectionSub(parent, button, url) {\r\n    let button_request = new XMLHttpRequest();\r\n    button_request.open('GET', `http://lampin.sakura.ne.jp/cgi-bin/paper_check.cgi?mode=register&url=${encodeURI(url)}`, true);\r\n    button_request.onload = function () {\r\n        const json = JSON.parse(this.responseText);\r\n        const json_line = json[\"result\"][0];\r\n        const result = json_line[1];\r\n        if (result == \"SUCCESS\") {\r\n            button.textContent = \"OK\";\r\n            button.onclick = null;\r\n            parent.style.backgroundColor = \"springgreen\";\r\n        }\r\n        else {\r\n            button.textContent = \"ERROR\";\r\n            button.onclick = null;\r\n            parent.style.backgroundColor = \"red\";\r\n        }\r\n    };\r\n    button_request.send();\r\n}\r\nexports.processPaperElementInfoCollectionSub = processPaperElementInfoCollectionSub;\r\nfunction processPaperElementInfoWithDivTag(info, check_result) {\r\n    const node = info.node;\r\n    const url = info.url;\r\n    if (check_result == \"NOT_REGISTRED\") {\r\n        const button = document.createElement(\"button\");\r\n        button.setAttribute(\"custom-url\", url);\r\n        button.textContent = \"Regist\";\r\n        button.onclick = (e) => {\r\n            processPaperElementInfoCollectionSub(node, button, url);\r\n        };\r\n        node.appendChild(button);\r\n    }\r\n    else if (check_result == \"DUPLICATION\") {\r\n        node.style.backgroundColor = \"springgreen\";\r\n    }\r\n}\r\nexports.processPaperElementInfoWithDivTag = processPaperElementInfoWithDivTag;\r\nfunction processPaperElementInfoWithATag(info, check_result) {\r\n    const node = info.node;\r\n    const url = info.url;\r\n    const span = document.createElement(\"span\");\r\n    span.setAttribute(\"data-result\", check_result);\r\n    span.setAttribute(\"data-url\", info.url);\r\n    const parent = node.parentElement;\r\n    if (parent != null) {\r\n        parent.insertBefore(span, node);\r\n        node.remove();\r\n        span.appendChild(node);\r\n        if (check_result == \"NOT_REGISTRED\") {\r\n            const button = document.createElement(\"button\");\r\n            button.style.marginRight = \"50px\";\r\n            button.setAttribute(\"custom-url\", url);\r\n            button.textContent = \"Regist\";\r\n            button.onclick = (e) => {\r\n                processPaperElementInfoCollectionSub(span, button, url);\r\n            };\r\n            span.insertBefore(button, node);\r\n        }\r\n        else if (check_result == \"DUPLICATION\") {\r\n            span.style.backgroundColor = \"springgreen\";\r\n        }\r\n    }\r\n}\r\nexports.processPaperElementInfoWithATag = processPaperElementInfoWithATag;\r\nfunction getURLForNodeRequest(url) {\r\n    let doi = null;\r\n    let arXivURL = null;\r\n    if (url.indexOf(\"https://doi.org/10.48550/arXiv.\") == 0) {\r\n        const psuf = url.substring(\"https://doi.org/10.48550/arXiv.\".length);\r\n        arXivURL = \"https://arxiv.org/abs/\" + psuf;\r\n    }\r\n    else if (url.indexOf(\"https://doi.org/\") == 0) {\r\n        doi = url.substring(\"https://doi.org/\".length);\r\n    }\r\n    else if (url.indexOf(\"https://arxiv.org/\") == 0) {\r\n        arXivURL = url;\r\n    }\r\n    else if (url.indexOf(\"http://arxiv.org/\") == 0) {\r\n        arXivURL = url.replace(\"http\", \"https\");\r\n    }\r\n    let registURL = null;\r\n    if (doi != null) {\r\n        registURL = doi;\r\n    }\r\n    if (arXivURL != null) {\r\n        registURL = arXivURL;\r\n    }\r\n    if (registURL != null) {\r\n        return registURL;\r\n    }\r\n    else {\r\n        return null;\r\n    }\r\n}\r\nexports.getURLForNodeRequest = getURLForNodeRequest;\r\nfunction processPaperElementInfo(info, check_result) {\r\n    if (info.node.tagName == \"A\") {\r\n        processPaperElementInfoWithATag(info, check_result);\r\n    }\r\n    else {\r\n        processPaperElementInfoWithDivTag(info, check_result);\r\n    }\r\n}\r\nexports.processPaperElementInfo = processPaperElementInfo;\r\nfunction processPaperElementInfoCollection(info_collection) {\r\n    const url_parameter = info_collection.collection.map((v) => v.url).join(\",\");\r\n    let request = new XMLHttpRequest();\r\n    const get_url = `http://lampin.sakura.ne.jp/cgi-bin/paper_check.cgi?mode=check&url=${encodeURI(url_parameter)}`;\r\n    request.open('GET', get_url, true);\r\n    request.onload = function () {\r\n        const json = JSON.parse(this.responseText);\r\n        const result_list = json[\"result\"];\r\n        result_list.forEach((v, i) => {\r\n            const check_result = v[1];\r\n            processPaperElementInfo(info_collection.collection[i], check_result);\r\n        });\r\n    };\r\n    request.onerror = function () {\r\n        console.log(\"error\");\r\n    };\r\n    request.onabort = function () {\r\n        console.log(\"abort\");\r\n    };\r\n    request.ontimeout = function () {\r\n        console.log(\"timeout\");\r\n    };\r\n    request.send();\r\n}\r\nexports.processPaperElementInfoCollection = processPaperElementInfoCollection;\r\n\n\n//# sourceURL=webpack://DBLPExtension/./src/node_request.ts?");

/***/ }),

/***/ "./src/stringology_times_processor.ts":
/*!********************************************!*\
  !*** ./src/stringology_times_processor.ts ***!
  \********************************************/
/***/ ((__unused_webpack_module, exports, __webpack_require__) => {

eval("\r\nObject.defineProperty(exports, \"__esModule\", ({ value: true }));\r\nexports.preprocessStringologyTimes = void 0;\r\nconst node_request_1 = __webpack_require__(/*! ./node_request */ \"./src/node_request.ts\");\r\nfunction createPaperCreateInfoArray() {\r\n    const r = new Array();\r\n    const nodes = document.body.getElementsByTagName(\"a\");\r\n    for (let i = 0; i < nodes.length; i++) {\r\n        const node = nodes.item(i);\r\n        if (node != null) {\r\n            const url = node.getAttribute(\"href\");\r\n            if (url != null) {\r\n                const regularURL = (0, node_request_1.getURLForNodeRequest)(url);\r\n                if (regularURL != null) {\r\n                    const info = { url: regularURL, node: node };\r\n                    r.push(info);\r\n                }\r\n            }\r\n        }\r\n    }\r\n    return r;\r\n}\r\nfunction preprocessStringologyTimes() {\r\n    const paperCreateInfoArray = createPaperCreateInfoArray();\r\n    const collections = (0, node_request_1.createCollections)(paperCreateInfoArray);\r\n    collections.forEach((v) => {\r\n        (0, node_request_1.processPaperElementInfoCollection)(v);\r\n    });\r\n}\r\nexports.preprocessStringologyTimes = preprocessStringologyTimes;\r\n\n\n//# sourceURL=webpack://DBLPExtension/./src/stringology_times_processor.ts?");

/***/ })

/******/ 	});
/************************************************************************/
/******/ 	// The module cache
/******/ 	var __webpack_module_cache__ = {};
/******/ 	
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/ 		// Check if module is in cache
/******/ 		var cachedModule = __webpack_module_cache__[moduleId];
/******/ 		if (cachedModule !== undefined) {
/******/ 			return cachedModule.exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = __webpack_module_cache__[moduleId] = {
/******/ 			// no module.id needed
/******/ 			// no module.loaded needed
/******/ 			exports: {}
/******/ 		};
/******/ 	
/******/ 		// Execute the module function
/******/ 		__webpack_modules__[moduleId](module, module.exports, __webpack_require__);
/******/ 	
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/ 	
/************************************************************************/
/******/ 	
/******/ 	// startup
/******/ 	// Load entry module and return exports
/******/ 	// This entry module can't be inlined because the eval devtool is used.
/******/ 	var __webpack_exports__ = __webpack_require__("./src/index.ts");
/******/ 	DBLPExtension = __webpack_exports__;
/******/ 	
/******/ })()
;