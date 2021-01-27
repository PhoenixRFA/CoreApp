'use strict';

const gulp = require('gulp'),
    //https://github.com/floatdrop/gulp-plumber
    plumber = require('gulp-plumber'),
    //https://github.com/dlmanning/gulp-sass#readme
    sass = require('gulp-sass'),
    //https://github.com/scniro/gulp-clean-css#readme
    cleanCss = require('gulp-clean-css'),
    //https://github.com/gulp-sourcemaps/gulp-sourcemaps
    sourcemaps = require('gulp-sourcemaps'),
    //https://github.com/kevva/gulp-shorthand
    shorthand = require('gulp-shorthand'),
    //https://github.com/sindresorhus/gulp-autoprefixer#readme
    autoprefixer = require('gulp-autoprefixer'),
    //https://github.com/olegskl/gulp-stylelint
    stylelint = require('gulp-stylelint'),
    //https://github.com/gulp-community/gulp-concat#readme
    concat = require('gulp-concat'),
    //https://github.com/hparra/gulp-rename
    rename = require('gulp-rename');

const webroot = './wwwroot/';
const paths = {
    //js: `${webroot}js/**/*.js`,
    //minJs: `${webroot}js/**/*.min.js`,
    //css: `${webroot}css/**/*.css`,
    //minCss: `${webroot}css/**/*.min.css`,
    sass: `${webroot}sass/**/*.scss`,
    //concatJsDest: `${webroot}js/bundle.min.js`,
    concatCssDest: `${webroot}css/`
};

// Clean assets
//function clean() {
//    return del([paths.concatCssDest, paths.concatJsDest]);
//}

// CSS task
function css() {
    return gulp
    .src(paths.sass)
    .pipe(plumber())
    .pipe(stylelint({
        failAfterError: false,
        reporters: [ { formatter: 'string', console: true } ]
    }))
    .pipe(sass({ outputStyle: 'expanded' }))
    .pipe(sourcemaps.init())
    .pipe(concat('styles.css'))
    .pipe(autoprefixer({ cascade: false }))
    .pipe(shorthand())
    .pipe(cleanCss({
        debug: true,
        compatibility: '*'
    }, details => {
        console.log(`${details.name}: Original size:${details.stats.originalSize} - Minified size: ${details.stats.minifiedSize}`);
    }))
    .pipe(sourcemaps.write('/map'))
    //.pipe(sourcemaps.write())
    .pipe(rename({ suffix: '.min' }))
    .pipe(gulp.dest(paths.concatCssDest));
}

// Lint scripts
//function scriptsLint() {
//    return gulp
//        .src([paths.js])
//        .pipe(plumber())
//        .pipe(eslint())
//        .pipe(eslint.format())
//        .pipe(eslint.failAfterError());
//}

module.exports.css = css;