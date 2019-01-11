'use strict';

const webpack = require('webpack');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const StyleLintPlugin = require('stylelint-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');

const postcss = require('./postcss');

const path = require('path');
const FilterWarningsPlugin = require('webpack-filter-warnings-plugin');

const sourceMap = process.env.TEST
  ? [new webpack.SourceMapDevToolPlugin({ filename: null, test: /\.ts$/ })]
  : [ ];

const basePlugins = [
  new webpack.ProvidePlugin({
    $: 'jquery',
    jQuery: 'jquery',
  }),
  new webpack.DefinePlugin({
    __DEV__: process.env.NODE_ENV !== 'production',
    __PRODUCTION__: process.env.NODE_ENV === 'production',
    __TEST__: JSON.stringify(process.env.TEST || false),
    'process.env.NODE_ENV': JSON.stringify(process.env.NODE_ENV),
  }),
  new HtmlWebpackPlugin({
    template: './src/index.html',
    inject: 'body',
    minify: false,
    chunksSortMode: 'dependency',
  }),
  // new webpack.NoEmitOnErrorsPlugin(),
  new CopyWebpackPlugin([
    { from: 'src/assets', to: 'assets' },
  ]),
  new webpack.LoaderOptionsPlugin({
    test: /\.css$/,
    options: {
      postcss,
    },
  }),
  new webpack.ContextReplacementPlugin(
    /angular[\/\\]core[\/\\](esm\/src|src)[\/\\]linker/, __dirname),
  new webpack.ContextReplacementPlugin(
    /\@angular(\\|\/)core(\\|\/)fesm5/, path.join(__dirname, './src')),
  new FilterWarningsPlugin({
    exclude: /System.import/,
  }),
].concat(sourceMap);

const devPlugins = [
  new StyleLintPlugin({
    configFile: './.stylelintrc',
    files: 'src/**/*.css',
    failOnError: false,
  }),
  /*
  // since polyfills are in a non-imported entry file
  new webpack.optimize.CommonsChunkPlugin({
    name: ['polyfills'],
  }),
  // extract webpack bootstrap
  new webpack.optimize.CommonsChunkPlugin({
    minChunks: Infinity,
    name: 'inline',
    filename: 'inline.js',
    sourceMapFilename: 'inline.map',
  }),
  */
];

const prodPlugins = [
  /* new webpack.optimize.CommonsChunkPlugin({
    name: [
      'vendor',
      'polyfills',
    ],
  }),
  // extract webpack bootstrap
  new webpack.optimize.CommonsChunkPlugin({
    minChunks: Infinity,
    name: 'inline',
    filename: 'inline.js',
    sourceMapFilename: 'inline.map',
  }),
  
  new webpack.optimize.UglifyJsPlugin({
    mangle: { keep_fnames: true },
    compress: {
      warnings: false,
    },
  }),
  */
];

module.exports = basePlugins
  .concat(process.env.NODE_ENV === 'production' ? prodPlugins : [])
  .concat(process.env.NODE_ENV === 'development' ? devPlugins : []);
