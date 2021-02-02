var theaccount;theaccount =
(self["webpackChunk_name_"] = self["webpackChunk_name_"] || []).push([["theaccount"],{

/***/ "./account.js":
/*!********************!*\
  !*** ./account.js ***!
  \********************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (__WEBPACK_DEFAULT_EXPORT__)
/* harmony export */ });
/* harmony import */ var _module1__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./module1 */ "./module1.js");
﻿

const module = new _module1__WEBPACK_IMPORTED_MODULE_0__.default('account');

module.echo('Echo from account !');

if (true) {
    console.warn(`${new Date()} App in dev mode! (sended from account)`);
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
0,[["./account.js","runtime"]]]);
//# sourceMappingURL=theaccount.js.map