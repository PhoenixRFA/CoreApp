const nodeEnvironment = process.env.NODE_ENV || 'development';
const webpack = require('webpack');
const path = require('path');

console.log(`Webpack config environment: ${nodeEnvironment}`);

const isDev = nodeEnvironment === 'development';
//const isProd = nodeEnvironment === 'production';

module.exports = {
    context: path.resolve(__dirname, 'src'),

    entry: {
        index: './index',
        theaccount: './account'
    },
    output: {
        path: path.resolve(__dirname, 'dist'),
        filename: '[name].js',
        library: '[name]'
    },

    devtool: 'source-map',

    plugins: [
        new webpack.DefinePlugin({
            ISDEV: JSON.stringify(isDev),
            //ISPROD: JSON.stringify(isProd)
        }),
        new webpack.NoEmitOnErrorsPlugin()
    ],

    resolve: {
        modules: ['node_modules'],
        extensions: ['.js', '.json']
    },
    resolveLoader: {
        modules: ['node_modules'],
        extensions: ['.js', '.json']
    },

    optimization: {
        runtimeChunk: {name: 'runtime'}
    },

    module: {
        rules: [
            //{
            //    test: /\.js$/,
            //    exclude: /node_modules/,
            //    use: {
            //        loader: 'babel-loader',
            //        options: {
            //            presets: [
            //                ['@babel/preset-env', { targets: 'defaults' }]
            //            ],
            //            plugins: ['@babel/plugin-transform-runtime']
            //        }
            //    }
            //}
        ]
    },

    mode: isDev ? 'development' : 'production',
    //watch: isDev
};

if (!isDev) {
    module.exports.module.rules.push(
        {
            test: /\.js$/,
            exclude: /node_modules/,
            use: {
                loader: 'babel-loader',
                options: {
                    presets: [
                        ['@babel/preset-env', { targets: 'defaults' }]
                    ],
                    plugins: ['@babel/plugin-transform-runtime']
                }
            }
        });
}