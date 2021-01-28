class Foo {
    constructor (name) {
        this.name = name
    }

    write () {
        console.log(`%c%s`, `color: green;`, this.name)
    }
}

export { Foo }
