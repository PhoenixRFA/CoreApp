var db = require('db');
var log = require('logger')(module);
var User = require('./user');

db.connect();

function run(){
    var foo = new User('John');
    foo.say(db.getPhrase('helloworld') + '!');

    log('server run');
}

if(module.parent){
    exports.run = run;
}else{
    run();
}