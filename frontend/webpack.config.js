const path = require("path")

const HtmlWebpackPlugin = require("html-webpack-plugin")

module.exports = {
  //mode: "development",
  mode: "production",
  entry: {
    bundle: path.resolve(`${__dirname}/src/index.js`)
  },
  output: {
    path: path.resolve(`${__dirname}/dist`),
    filename: '[name].[contenthash].js',
    clean: true
  },
  devtool: 'source-map',
  module: {
    rules: [
      {
        test: /\.scss$/,
        use: [
          'style-loader',
          'css-loader',
          'sass-loader'
        ]
      },
      {
        test: /\.(png|svg|ico)$/i,
        type: 'asset/resource'
      }
    ]
  },
  plugins: [
    new HtmlWebpackPlugin({
      filename: 'index.html',
      template: './src/index.html',
      favicon: './src/favicon.ico'
    })
  ]
}