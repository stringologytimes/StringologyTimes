module.exports = {
    mode: 'development',
    entry: './src/index.ts',
    output: {
        library: "DBLPExtension",
        path: __dirname,
        filename: 'dist/main.js'
    },
    resolve: {
        extensions: ['.ts', '.js']
    },
    module: {
        rules: [{
            test: /\.ts$/,
            loader: 'ts-loader'
        }]
    }
}