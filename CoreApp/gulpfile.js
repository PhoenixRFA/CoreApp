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
    rename = require('gulp-rename'),
    //https://github.com/adametry/gulp-eslint
    eslint = require('gulp-eslint'),
    //https://github.com/duan602728596/gulp-terser
    minify = require('gulp-terser'),
    //https://github.com/sindresorhus/del
    del = require('del'),
    //https://github.com/dshemendiuk/gulp-npm-dist
    npmDist = require('gulp-npm-dist');

const webroot = './wwwroot/';
const paths = {
    js: `${webroot}js/**/*.js`,
    //minJs: `${webroot}js/**/*.min.js`,
    //css: `${webroot}css/**/*.css`,
    //minCss: `${webroot}css/**/*.min.css`,
    sass: `${webroot}sass/**/*.scss`,
    concatJsDest: `${webroot}js/`,
    concatCssDest: `${webroot}css/`
};

// Clean assets
function clean(cb) {
    return del([`${webroot}js/appbundle.min.js`, `${webroot}css/styles.min.css`]).then(() => {
        cb();
    });
}

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
function js() {
    return gulp
    .src(paths.js)
    .pipe(plumber())
    .pipe(eslint())
    .pipe(eslint.format())//outputs the lint results to the console
    //.pipe(eslint.failAfterError())
    .pipe(sourcemaps.init())
    .pipe(concat('appbundle.js'))
    .pipe(minify())
    //.pipe(sourcemaps.write('/map'))
    .pipe(sourcemaps.write())
    .pipe(rename({ suffix: '.min' }))
    .pipe(gulp.dest(paths.concatJsDest));
}

function copyModules(cb) {
    del(`${webroot}lib`).then(() => {
        gulp.src(npmDist(), { base: `./node_modules` })
            .pipe(gulp.dest(`${webroot}lib`)).on('end', cb)
    }).catch(cb)
}

module.exports.css = css;
module.exports.clean = clean;
module.exports.js = js;
module.exports.copyModules = copyModules;