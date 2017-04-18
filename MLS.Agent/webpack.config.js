var webpack = require('webpack');
var path = require('path');
const CopyWebpackPlugin = require('copy-webpack-plugin');

var APP_DIR = path.resolve(__dirname, '.');

module.exports = {
  entry: APP_DIR + '/index.jsx',
  output: {
    filename: 'bundle.js',
    path: path.resolve(__dirname, 'wwwroot')
  },
  module : {
    loaders : [
      {
        test : /\.jsx?/,
        include : APP_DIR,
        loader : 'babel-loader'
      },
      {
        test: /\.css$/,
        loader: 'style-loader!css-loader?modules&importLoaders=1&localIdentName=[name]__[local]___[hash:base64:5]' 
      }
    ]
  },
  plugins: [
    new CopyWebpackPlugin([
      {
        from: 'node_modules/monaco-editor/min/vs',
        to: 'vs',
      }
    ])
  ]
};