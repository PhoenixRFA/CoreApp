"use strict"

//const autoprefixer = require("autoprefixer");
//const eslint = require("gulp-eslint");
//const imagemin = require("gulp-imagemin");
const cssnano = require("cssnano");
const del = require("del");
const gulp = require("gulp");
const newer = require("gulp-newer");
const plumber = require("gulp-plumber");
const postcss = require("gulp-postcss");
const rename = require("gulp-rename");
const sass = require("gulp-sass");

//const
//    gulp = require("gulp"),
//    concat = require("gulp-concat"),
//    cssmin = require("gulp-cssmin"),
//    uglify = require("gulp-uglify"),
//    sass = require("gulp-sass"),
//    rimraf = require("rimraf");

const paths = {
    webroot: './webroot/',
    js: `${paths.webroot}js/**/*.js`,
    minJs: `${paths.webroot}js/**/*.min.js`,
    css: `${paths.webroot}css/**/*.css`,
    minCss: `${paths.webroot}css/**/*.min.css`,
    scss: `${paths.webroot}sass/**/*.scss`,
    concatJsDest: `${paths.webroot}js/bundle.min.js`,
    concatCssDest: `${paths.webroot}css/`,//style.min.css
};

// Clean assets
function clean() {
    return del([paths.concatCssDest, paths.concatJsDest]);
}

// CSS task
function css() {
    return gulp
        .src(paths.sass)
        .pipe(plumber())
        .pipe(sass({ outputStyle: 'expanded' }))
        .pipe(gulp.dest(paths.concatCssDest))
        .pipe(rename({ suffix: '.min' }))
        .pipe(postcss([cssnano()]))
        .pipe(gulp.dest(paths.concatCssDest));
}

// Lint scripts
function scriptsLint() {
    return gulp
        .src([paths.js])
        .pipe(plumber())
        .pipe(eslint())
        .pipe(eslint.format())
        .pipe(eslint.failAfterError());
}