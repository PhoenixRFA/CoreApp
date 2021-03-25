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
    //https://github.com/sindresorhus/del
    del = require('del');
    
    //https://browsersync.io/
    const server = require('browser-sync').create()

const webroot = './';
const paths = {
    sass: `${webroot}sass/**/*.scss`,
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
    //.pipe(concat('styles.css'))
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

function readyReload(cb) {
    server.reload()
    cb()
}

module.exports.css = css;
module.exports.clean = clean;
module.exports.serve = function(cb){
    server.init({
        server: {
            baseDir: "./"
        },
        notify: false,
        open: true,
        cors: true
    })

    gulp.watch(paths.sass, gulp.series(css, cb => gulp.src('css').pipe(server.stream()).on('end', cb)));
    gulp.watch('./**/*.html', readyReload);

    return cb();
}