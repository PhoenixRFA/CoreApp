var db = require('db');

function User(name){
    this.name = name;
}

User.prototype.say = function(text){
    console.log(this.name + ' ' + db.getPhrase('said') + ': ' + text);
}

//exports.User = User;
module.exports = User