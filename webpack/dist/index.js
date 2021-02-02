var index;index =
(self["webpackChunk_name_"] = self["webpackChunk_name_"] || []).push([["index"],{

/***/ "./index.js":
/*!******************!*\
  !*** ./index.js ***!
  \******************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _module1__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./module1 */ "./module1.js");
﻿//'use strict';


const module = new _module1__WEBPACK_IMPORTED_MODULE_0__.default('test module');

module.echo('Hello world!');

if (true) {
    console.warn(`${new Date()} App in dev mode!`);
}

/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (module);

/***/ }),

/***/ "./module1.js":
/*!********************!*\
  !*** ./module1.js ***!
  \********************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
﻿
function module(name) {
    this.name = name;
}

module.prototype.echo = function(msg) {
    console.log('%c%s %csaid: %c%s', 'font-weight: bold;', this.name, null, 'text-decoration: underline; font-style: oblique;', msg);
}

/* harmony default export */ const __WEBPACK_DEFAULT_EXPORT__ = (module);


/***/ })

},
0,[["./index.js","runtime"]]]);
//# sourceMappingURL=index.js.map