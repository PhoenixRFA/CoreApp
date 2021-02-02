document.getElementById('get-require').onclick = () => {
    import(/*webpackChunkName: [id].js */'./module1').then(module => {
        const module1 = new module.default('dynamic');
        module1.echo('hello from dynamic require');
    });
};