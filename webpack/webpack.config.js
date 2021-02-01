
const nodeEnvironment = process.env.NODE_ENV || 'development';
const webpack = require('webpack');

console.log(`Webpack config environment: ${nodeEnvironment}`);

const isDev = nodeEnvironment === 'development';
//const isProd = nodeEnvironment === 'production';

module.exports = {
    entry: './src/index.js',
    output: {
        filename: 'bundle.js',
        library: 'App'
    },

    devtool: 'source-map',

    plugins: [
        new webpack.DefinePlugin({
            ISDEV: JSON.stringify(isDev),
            //ISPROD: JSON.stringify(isProd)
        })
    ],

    mode: isDev ? 'development' : 'production',
    watch: isDev
};
