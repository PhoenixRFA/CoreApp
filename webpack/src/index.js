//'use strict';
import Module from './module1';

const module = new Module('test module');

module.echo('Hello world!');

if (ISDEV) {
    console.warn('App in dev mode!');
}

export default module;