'use strict';

const gulp = require('gulp'),
    plumber = require('gulp-plumber'),
    sass = require('gulp-sass'),
    cleanCss = require('gulp-clean-css'),
    sourcemaps = require('gulp-sourcemaps'),
    shorthand = require('gulp-shorthand'),
    autoprefixer = require('gulp-autoprefixer'),
    stylelint = require('gulp-stylelint'),
    concat = require('gulp-concat'),
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
        reporters: [
            {
                formatter: 'string',
                console: true
            }
        ]
    }))
    .pipe(sourcemaps.init())
    .pipe(sass({ outputStyle: 'expanded' }))
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