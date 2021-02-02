var dynamic;dynamic =
(self["webpackChunk_name_"] = self["webpackChunk_name_"] || []).push([["dynamic"],{

/***/ "./dynamicRequire.js":
/*!***************************!*\
  !*** ./dynamicRequire.js ***!
  \***************************/
/***/ ((__unused_webpack_module, __unused_webpack_exports, __webpack_require__) => {

﻿document.getElementById('get-require').onclick = () => {
    __webpack_require__.e(/*! import() */ "module1_js").then(__webpack_require__.bind(__webpack_require__, /*! ./module1 */ "./module1.js")).then(module => {
        const module1 = new module.default('dynamic');
        module1.echo('hello from dynamic require');
    });
};

/***/ })

},
0,[["./dynamicRequire.js","runtime"]]]);
//# sourceMappingURL=dynamic.js.map