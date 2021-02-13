const add = (x, y) => (+x) + (+y);
const subtract = (x, y) => (+x) - (+y);
const multiply = (x, y) => (+x) * (+y);
const divide = (x, y) => (+x) / (+y);
const validateNumbers = (x, y) => !isNaN(x) && !isNaN(y);

module.exports = {
    add, subtract, multiply, divide,
    validateNumbers
}