var custom;custom =
(self["webpackChunk_name_"] = self["webpackChunk_name_"] || []).push([["custom"],{

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
0,[["./module1.js","runtime"]]]);
//# sourceMappingURL=custom.js.map