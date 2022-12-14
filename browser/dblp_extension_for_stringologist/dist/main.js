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

eval("\r\nObject.defineProperty(exports, \"__esModule\", ({ value: true }));\r\nexports.preprocessDBLP = void 0;\r\nconst node_request_1 = __webpack_require__(/*! ./node_request */ \"./src/node_request.ts\");\r\nfunction addToDBLPElements(nodes, output) {\r\n    for (let i = 0; i < nodes.length; i++) {\r\n        const node = nodes.item(i);\r\n        if (node instanceof HTMLElement) {\r\n            output.push(node);\r\n        }\r\n    }\r\n}\r\nfunction createCollections(info_list) {\r\n    const r = [];\r\n    let x = new node_request_1.RequestCollection();\r\n    info_list.forEach((v) => {\r\n        if (x.collection.length > 50) {\r\n            r.push(x);\r\n            x = new node_request_1.RequestCollection();\r\n        }\r\n        x.collection.push(v);\r\n    });\r\n    r.push(x);\r\n    return r;\r\n}\r\nfunction preprocessDBLP(dblpElements) {\r\n    addToDBLPElements(document.body.getElementsByClassName(\"entry inproceedings\"), dblpElements);\r\n    addToDBLPElements(document.body.getElementsByClassName(\"entry informal\"), dblpElements);\r\n    addToDBLPElements(document.body.getElementsByClassName(\"entry article\"), dblpElements);\r\n    const dblpElementsWithURL = [];\r\n    dblpElements.forEach((node) => {\r\n        const aNodes = node.getElementsByTagName(\"a\");\r\n        let doi = null;\r\n        let arXivURL = null;\r\n        for (let i = 0; i < aNodes.length; i++) {\r\n            const aNode = aNodes.item(i);\r\n            if (aNode != null) {\r\n                const hrefStr = aNode.getAttribute(\"href\");\r\n                if (hrefStr != null) {\r\n                    if (hrefStr.indexOf(\"https://doi.org/\") == 0) {\r\n                        doi = hrefStr.substring(\"https://doi.org/\".length);\r\n                        break;\r\n                    }\r\n                    else if (hrefStr.indexOf(\"https://arxiv.org/\") == 0) {\r\n                        arXivURL = hrefStr;\r\n                        break;\r\n                    }\r\n                    else if (hrefStr.indexOf(\"http://arxiv.org/\") == 0) {\r\n                        arXivURL = hrefStr.replace(\"http\", \"https\");\r\n                        break;\r\n                    }\r\n                }\r\n            }\r\n        }\r\n        let registURL = null;\r\n        if (doi != null) {\r\n            registURL = doi;\r\n        }\r\n        if (arXivURL != null) {\r\n            registURL = arXivURL;\r\n        }\r\n        if (registURL != null) {\r\n            const xnode = { node: node, url: registURL };\r\n            dblpElementsWithURL.push(xnode);\r\n        }\r\n    });\r\n    const collections = createCollections(dblpElementsWithURL);\r\n    collections.forEach((v) => {\r\n        (0, node_request_1.node_request_X)(v);\r\n    });\r\n}\r\nexports.preprocessDBLP = preprocessDBLP;\r\n\n\n//# sourceURL=webpack://DBLPExtension/./src/dblp_preprocessor.ts?");

/***/ }),

/***/ "./src/index.ts":
/*!**********************!*\
  !*** ./src/index.ts ***!
  \**********************/
/***/ ((__unused_webpack_module, exports, __webpack_require__) => {

eval("\r\nObject.defineProperty(exports, \"__esModule\", ({ value: true }));\r\nconsole.log(\"Hello\");\r\nconst dblp_preprocessor_1 = __webpack_require__(/*! ./dblp_preprocessor */ \"./src/dblp_preprocessor.ts\");\r\nconst dblpElements = [];\r\n(0, dblp_preprocessor_1.preprocessDBLP)(dblpElements);\r\n\n\n//# sourceURL=webpack://DBLPExtension/./src/index.ts?");

/***/ }),

/***/ "./src/node_request.ts":
/*!*****************************!*\
  !*** ./src/node_request.ts ***!
  \*****************************/
/***/ ((__unused_webpack_module, exports) => {

eval("\r\nObject.defineProperty(exports, \"__esModule\", ({ value: true }));\r\nexports.node_request_X = exports.node_request2 = exports.RequestCollection = void 0;\r\nclass RequestCollection {\r\n    constructor() {\r\n        this.collection = [];\r\n    }\r\n}\r\nexports.RequestCollection = RequestCollection;\r\nfunction node_request2(parent, button, url) {\r\n    let button_request = new XMLHttpRequest();\r\n    button_request.open('GET', `http://lampin.sakura.ne.jp/cgi-bin/paper_check.cgi?mode=register&url=${encodeURI(url)}`, true);\r\n    button_request.onload = function () {\r\n        const json = JSON.parse(this.responseText);\r\n        const json_line = json[\"result\"][0];\r\n        const result = json_line[1];\r\n        if (result == \"SUCCESS\") {\r\n            button.textContent = \"OK\";\r\n            button.onclick = null;\r\n            parent.style.backgroundColor = \"springgreen\";\r\n        }\r\n        else {\r\n            button.textContent = \"ERROR\";\r\n            button.onclick = null;\r\n            parent.style.backgroundColor = \"red\";\r\n        }\r\n    };\r\n    button_request.send();\r\n}\r\nexports.node_request2 = node_request2;\r\nfunction node_request_X(info_collection) {\r\n    const url_parameter = info_collection.collection.map((v) => v.url).join(\",\");\r\n    let request = new XMLHttpRequest();\r\n    const get_url = `http://lampin.sakura.ne.jp/cgi-bin/paper_check.cgi?mode=check&url=${encodeURI(url_parameter)}`;\r\n    request.open('GET', get_url, true);\r\n    request.onload = function () {\r\n        const json = JSON.parse(this.responseText);\r\n        const result_list = json[\"result\"];\r\n        result_list.forEach((v, i) => {\r\n            const check_result = v[1];\r\n            const node = info_collection.collection[i].node;\r\n            const url = info_collection.collection[i].url;\r\n            if (check_result == \"NOT_REGISTRED\") {\r\n                const button = document.createElement(\"button\");\r\n                button.setAttribute(\"custom-url\", url);\r\n                button.textContent = \"Regist\";\r\n                button.onclick = (e) => {\r\n                    node_request2(node, button, url);\r\n                };\r\n                node.appendChild(button);\r\n            }\r\n            else if (check_result == \"DUPLICATION\") {\r\n                node.style.backgroundColor = \"springgreen\";\r\n            }\r\n        });\r\n    };\r\n    request.onerror = function () {\r\n        console.log(\"error\");\r\n    };\r\n    request.onabort = function () {\r\n        console.log(\"abort\");\r\n    };\r\n    request.ontimeout = function () {\r\n        console.log(\"timeout\");\r\n    };\r\n    request.send();\r\n}\r\nexports.node_request_X = node_request_X;\r\n\n\n//# sourceURL=webpack://DBLPExtension/./src/node_request.ts?");

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