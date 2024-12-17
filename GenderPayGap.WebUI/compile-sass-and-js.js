var fs = require('fs');
var path = require('path');
var crypto = require('crypto');

var sass = require('node-sass');
var UglifyJS = require("uglify-js");


var pathToCurrentDirectory = './';
var pathToVisualStudioDebugDirectory = './bin/Debug/net8.0/';

var inputDirectory = './wwwroot';
var inputJsDirectory = './wwwroot/scripts';
var outputDirectory = './wwwroot/compiled';


function makeOutputDirectoryIfItDoesNotExist(options) {
    function action(directory) {
        console.log(`Making output directory (if it does not exist) [${directory + outputDirectory}]`);

        if (!fs.existsSync(directory + outputDirectory)) {
            fs.mkdirSync(directory + outputDirectory);
        }
    }

    action(pathToCurrentDirectory);
    if (options.runningLocally) {
        action(pathToVisualStudioDebugDirectory);
    }
}

function deleteExistingCompiledCssAndJsFiles(options) {
    function action(directory) {
        console.log(`Deleting existing compiled CSS and JS files from output directory [${directory + outputDirectory}]`);
        var files = fs.readdirSync(directory + outputDirectory);

        files.forEach(function (fileName) {
            if (/app-.*.css/.test(fileName) || /app-.*.js/.test(fileName)) {
                var filePath = path.join(directory + outputDirectory, fileName);
                console.log(`Deleting file [${filePath}]`);
                fs.unlinkSync(filePath);
            }
        });
    }

    action(pathToCurrentDirectory);
    if (options.runningLocally) {
        action(pathToVisualStudioDebugDirectory);
    }
}

function compileSass(inputFile, outputFileNamePrefix, options) {
    function saveAction(directory) {
        // Generate the filename, based on the hash
        var outputFilePath = `${directory + outputDirectory}/${outputFileNamePrefix}-${hashResult}.css`;
        console.log(`Saving SASS to file [${outputFilePath}]`);

        // Save the SASS
        fs.writeFileSync(outputFilePath, renderResult.css);
    }

    console.log(`Compiling SASS from file [${inputFile}]`);

    // Render the SASS to string
    var renderResult = sass.renderSync({
        file: inputFile
    });

    if (renderResult) {
        // Compute the hash of the compiled SASS
        var hash = crypto.createHash('sha256');
        hash.update(renderResult.css);
        var hashResult = hash.digest('hex');

        saveAction(pathToCurrentDirectory);
        if (options.runningLocally) {
            saveAction(pathToVisualStudioDebugDirectory);
        }
    }

}

function compileJs(options) {
    function saveAction(directory) {
        // Generate the filename, based on the hash
        var outputFilePath = `${directory + outputDirectory}/app-${hashResult}.js`;
        console.log(`Saving JS to file [${outputFilePath}]`);

        // Save the JS
        fs.writeFileSync(outputFilePath, minifyResult.code);
    }

    console.log(`Compiling JS`);
    var files = fs.readdirSync(inputJsDirectory);

    var code = {};

    files.forEach(function (fileName) {
        if (fileName.endsWith('.js')) {
            var filePath = path.join(inputJsDirectory, fileName);
            var fileContents = fs.readFileSync(filePath, { encoding: 'utf8' });
            code[fileName] = fileContents;
        }
    });

    var minifyOptions = {
        keep_fnames: false,
        mangle: false     
    };
    var minifyResult = UglifyJS.minify(code, minifyOptions);

    if (minifyResult.code) {
        // Compute the hash of the compiled JS
        var hash = crypto.createHash('sha256');
        hash.update(minifyResult.code);
        var hashResult = hash.digest('hex');

        saveAction(pathToCurrentDirectory);
        if (options.runningLocally) {
            saveAction(pathToVisualStudioDebugDirectory);
        }
    } else {
        console.log("MINIFY ERROR: " + minifyResult.error)
    }
}

async function fullRecompile(options) {
    makeOutputDirectoryIfItDoesNotExist(options);
    deleteExistingCompiledCssAndJsFiles(options);

    compileSass('./wwwroot/styles/app.scss', 'app', options);
    compileSass('./wwwroot/styles/app-ie8.scss', 'app-ie8', options);

    compileJs(options);
}

function stopOnCtrlC() {
    if (process.platform === "win32") {
        var rl = require("readline").createInterface({
            input: process.stdin,
            output: process.stdout
        });

        rl.on("SIGINT",
            function () {
                process.emit("SIGINT");
            });
    }

    process.on("SIGINT",
        function () {
            //graceful shutdown
            process.exit();
        });
}

var needsRecompile = false;

async function recompileWhenNeeded() {
    var snooze = ms => new Promise(resolve => setTimeout(resolve, ms));

    while (true) {
        await snooze(200);

        if (needsRecompile) {
            needsRecompile = false;
            fullRecompile({ runningLocally: true });
        }
    }
}

function watchAndAskForRecompile() {
    function fileChanged(eventType, filename) {
        if (filename && filename.indexOf('compiled') === -1) {
            needsRecompile = true;
        }
    }

    fs.watch(
        inputDirectory,
        {
            recursive: true
        },
        fileChanged
    );
}


if (process.argv.includes('--watch')) {
    fullRecompile({ runningLocally: true });

    console.log('');
    console.log('Watching for changes');
    console.log('Press Ctrl+C to exit');
    console.log('');

    stopOnCtrlC();
    recompileWhenNeeded();
    watchAndAskForRecompile();
}
else
{
    fullRecompile({});
}
