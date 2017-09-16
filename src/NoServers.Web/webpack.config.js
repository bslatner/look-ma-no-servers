var path = require("path");
var webpack = require("webpack");
var CleanWebpackPlugin = require("clean-webpack-plugin");

const bundleFolder = "wwwroot/bundle/";

module.exports = {
    devtool: "source-map",
    entry: {
        "index": "./scripts/index.ts"
    },
    output: {
        publicPath: "/dist/",
        filename: "[name].js",
        path: path.resolve(__dirname, bundleFolder)
    },
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                loader: "ts-loader",
                exclude: /node_modules/
            }
        ]
    },
    plugins: [
        new CleanWebpackPlugin([bundleFolder])
    ],
    externals: {
        "jquery": "jQuery"  
    },
    resolve: {
        extensions: [".tsx", ".ts", ".js"]
    }
};