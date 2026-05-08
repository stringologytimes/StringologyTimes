const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');



module.exports = {
  mode: "development",
  devtool: "eval-source-map",
  
  entry: './src/index.ts',
  output: {
    path: path.resolve(__dirname, 'dist'),
    filename: 'bundle.js',
    clean: true,
  },
  resolve: {
    extensions: ['.ts', '.tsx', '.js'],
  },
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: 'ts-loader',
        exclude: /node_modules/,
      },
      {
        test: /\.css$/i,
        use: ['style-loader', 'css-loader'],
      },
    ],
  },
  externals: {
    'pako': 'pako',
  },
  plugins: [
    new HtmlWebpackPlugin({
      template: './index.html',
      filename: 'index.html',
    }),
    new CopyWebpackPlugin({
      patterns: [
        {
          from: 'doi_info_parts',
          to: 'doi_info_parts',
        },
      ],
    }),
    new CopyWebpackPlugin({
      patterns: [
        {
          from: 'src/images',
          to: 'images',
        },
      ],
    }),
  ],
  devServer: {
    static: [
      {
        directory: path.join(__dirname, 'dist'),
      },
      {
        directory: path.join(__dirname, 'doi_info_parts'),
        publicPath: '/doi_info_parts',
      },
    ],
    compress: true,
    port: 9000,
    open: true,
  },
};

