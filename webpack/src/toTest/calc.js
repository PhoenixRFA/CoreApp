const operations = require('./operations');
const readline = require('readline');//Node.js CLI module https://nodejs.org/api/readline.html

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

console.log(`
Calc.js

Hello world!
`);

rl.question('First number: ', (x) => {
    rl.question('Second number: ', (y) => {
        rl.question(`
Choose operation:

[1] Sum (+)
[2] Subtract (-)
[3] Multiply (*)
[4] Divide (/)

Operation: `, (choice) => {
            if (!operations.validateNumbers(x, y)) {
                console.log('Please enter only numbers!');
            } else {
                switch (choice) {
                case '1':
                    console.log(`${x} + ${y} = ${operations.add(x, y)}`);
                    break;
                case '2':
                    console.log(`${x} - ${y} равна ${operations.subtract(x, y)}`);
                    break;
                case '3':
                    console.log(`${x} * ${y} равно ${operations.multiply(x, y)}`);
                    break;
                case '4':
                    console.log(`${x} / ${y} равно ${operations.divide(x, y)}`);
                    break;
                default:
                    console.log('Unknown operation');
                    break;
                }
            }

            rl.close();
        });
    });
});

//rl.close();