class myClass {
    constructor(async_param) {
        if (typeof async_param === 'undefined') {
            throw new Error('Cannot be called directly');
        }
    }

    static async build() {
        var async_result = await doSomeAsyncStuff();
        return new myClass(async_result);
    }
}