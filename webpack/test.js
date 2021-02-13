const mocha = require('mocha');
const chai = require('chai');
var sinon = require('sinon');
//const assert = require('assert');
//const test1 = require('/tests/test1');
const operations = require('./src/toTest/operations');

const assert = chai.assert;
const expect = chai.expect;

describe('calc operations', function() {
    it('correctly sum 1 & 6', function() {
        assert.equal(operations.add(1, 6), 7);
    });

    it('correctly sum -1 & -1', () => {
        assert.equal(operations.add(-1, -1), -2);
    });

    it('correctly subtract 33 & 3', () => {
        assert.equal(operations.subtract(33, 3), 30);
    });

    it('correctly multiply 12 & 12', () => {
        assert.equal(operations.multiply(12, 12), 144);
    });

    it('correctly divide 10 & 2', () => { 
        assert.equal(operations.divide(10, 2), 5);
    });

    it('false on string instead of number', () => {
        assert.equal(operations.validateNumbers('foo', 5), false);
        assert.equal(operations.validateNumbers(5, 'foo'), false);
        assert.equal(operations.validateNumbers('foo', 'bar'), false);
    });

    it('true on both numbers', () => {
        assert.equal(operations.validateNumbers(5, 5), true);
    });
});

//chai asserts https://www.chaijs.com/api/assert/
describe('Other tests', () => {
    it('equal test', () => {
        assert.equal('1', 1);
    });

    it('strict equal test', () => {
        assert.strictEqual(1, 1);
    });

    it('not equal', () => {
        assert.notEqual(1, 0);
    });

    it('not strict equal', () => {
        assert.notStrictEqual(1, '1');
    });
    
    it('is true', () => {
        assert.isTrue(1 > 0);
    });

    it('expect to equal', () => {
        expect('123').to.equal('123');
    });

    it('type of', () => {
        assert.typeOf('foo', 'string', 'foo must be a string');
    });
    
    it('length of', () => {
        assert.lengthOf([1,2,3], 3, 'array length must be 3');
    });
});

describe('Try sinon', () => {
    function once(fn) {
        let returnValue, called = false;
        return function () {
            if (!called) {
                called = true;
                returnValue = fn.apply(this, arguments);
            }
            
            return returnValue;
        };
    }

    it('calls the original function', () => {
        const cb = sinon.fake();
        var proxy = once(cb);

        proxy();
        
        assert.isTrue(cb.called);
    });

    it('calls the original function only once', () => {
        const cb = sinon.fake();
        var proxy = once(cb);

        proxy();
        proxy();

        assert.isTrue(cb.calledOnce);
    });

    it('calls original function with right this and args', function () {
        const callback = sinon.fake();
        const proxy = once(callback);
        const obj = {};

        proxy.call(obj, 1, 2, 3);

        assert(callback.calledOn(obj));//check this
        assert(callback.calledWith(1, 2, 3));//check params
    });

    it('returns the return value from the original function', function () {
        const callback = sinon.fake.returns(42);
        const proxy = once(callback);

        assert.equal(proxy(), 42);
    });

    //emulate jquery
    const jQuery = {
        ajax(params) { }
    };

    //function to mock
    function getTodos(listId, callback) {
        jQuery.ajax({
            url: `/todo/${listId}/items`,
            success: function (data) {
                callback(null, data);
            }
        });
    }

    after(() => sinon.restore());

    it('makes a GET request for todo items', () => {
        sinon.replace(jQuery, 'ajax', sinon.fake());

        getTodos(42, sinon.fake());

        //check if ajax called with correct url
        assert(jQuery.ajax.calledWithMatch({ url: '/todo/42/items' }));
    });

    //TODO Not working
    //var xhr, requests, realXhr;

    //before(function () {
    //    realXhr = xhr;
    //    xhr = sinon.useFakeXMLHttpRequest();
    //    requests = [];
    //    xhr.onCreate = function (req) {
    //        requests.push(req);
    //    };
    //});

    //after(function () {
    //    sinon.restore();
    //    xhr = realXhr;
    //});

    //it('makes a GET request for todo items', function () {
    //    var x = new xhr();
    //    x.open('GET', '/todo/42/items', false);
    //    x.send();
        
    //    assert.equal(requests.length, 1);
    //    assert.match(requests[0].url, '/todo/42/items');
    //});

    //TODO Not working
    //var server;

    //before(function () {
    //    server = sinon.fakeServer.create();
    //});
    //after(function () {
    //    server.restore();
    //});

    //it('calls callback with deserialized data', function () {
    //    const callback = sinon.fake();
    //    getTodos(42, callback);

    //    // This is part of the FakeXMLHttpRequest API
    //    server.requests[0].respond(
    //        200,
    //        { 'Content-Type': 'application/json' },
    //        JSON.stringify([{ id: 1, text: 'Provide examples', done: true }])
    //    );

    //    assert(callback.calledOnce);
    //});

    function debounce(callback) {
        var timer;
        return function () {
            clearTimeout(timer);
            var args = [].slice.call(arguments);
            timer = setTimeout(function () {
                callback.apply(this, args);
            }, 100);
        };
    }

    var clock;

    before(function () {
        clock = sinon.useFakeTimers();
    });
    after(function () {
        clock.restore();
    });

    it('calls callback after 100ms', function () {
        const callback = sinon.fake();
        const throttled = debounce(callback);

        throttled();

        clock.tick(99);
        assert(callback.notCalled);

        clock.tick(1);
        assert(callback.calledOnce);

        // Also:
        assert.equal(new Date().getTime(), 100);
    });
});