// Copyright 2020 Frameplay. All Rights Reserved.
//JavaScript library for decoding images at runtime in a WebWorker in WebGLbuilds.

var LibraryImageDecodeChunks = {
    $iterationsReady: [],
    $iterations: -1,
    $iteration: 0,
    $worker: undefined,
    $imageInstances: [],
    $rectVector4s: [],
    $currentRectV4: {},
    $errorCode: 0,
    // number of image chunks in the image
    NumWebGLIterations: function() {
        return iterations;
    },
    CanGetTexture: function(imageChunkIndex) {
        return iterationsReady.includes(imageChunkIndex) && iterations > 0;
    },
    ShaderRectBLX: function() {
        return currentRectV4.bl_x;
    },
    ShaderRectBLY: function() {
        return currentRectV4.bl_y;
    },
    ShaderRectTRX: function() {
        return currentRectV4.tr_x;
    },
    ShaderRectTRY: function() {
        return currentRectV4.tr_y;
    },
    UpdateTexture: function(imageChunkIndex, tex) {
        //console.time("UpdateTexture");
        var chunkFound = false;
        for(var i = 0; i < iterationsReady.length; ++i)
        {
            if(imageChunkIndex == iterationsReady[i])
            { // found the correct image chunk, put the Image into the texture for Unity to blit from
                // pass the chunk window to C# for the shader
                currentRectV4 = rectVector4s.splice(i, 1)[0];
                iterationsReady.splice(i, 1);
                GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[tex]);
                // normally webgl's y coordinate is flipped - we maintain that, but flip the destination uvs in the shader in unity
                GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, false);
                const img = imageInstances.splice(i, 1)[0];
                document.body.removeChild(img);
                GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGB, GLctx.UNSIGNED_BYTE, img);
                if(imageChunkIndex == iterations-1)
                { // handle final chunk
                    iterations = -1;
                }
                chunkFound = true;
                break;
            }
        }
        if(!chunkFound)
            console.warn("Tried to update texture but chunk " + tex + " was not found");
        //console.timeEnd("UpdateTexture");
    },
    AbortLoad: function() {
        worker.postMessage(["Abort"]);
        iterations = -2; // so we can check if it is aborted
        iteration = 0;
        while(imageInstances.length>0)
        { // remove existing images from document
            document.body.removeChild(imageInstances.pop());
        }
        iterationsReady.length = 0;
        rectVector4s.length = 0;
        currentRectV4 = {};
    },
    // The worker downloads and decodes an image, and splits it into subImageWidth * subImageHeight chunks,
    // which get converted to pngs and stored in blobs for the main thread to turn into Images
    InitializeWebWorker: function(_subImageWidth, _subImageHeight, _timeout) {
        //console.time("InitializeWebWorker");
        var workerDecode = "\n\
        error = 0;\n\
        try{importScripts(\n\
            \"https://static.frameplay.gg/js/sdk_unity_webgl1.js\",\n\
            \"https://static.frameplay.gg/js/sdk_unity_webgl2.js\",\n\
            \"https://static.frameplay.gg/js/sdk_unity_webgl3.js\",\n\
            \"https://static.frameplay.gg/js/sdk_unity_webgl4.js\",\n\
        );} \n\
        catch(e) {\n\
            postMessage(JSON.stringify({\n\
                error: \"Error loading web worker decode js files\"\n\
            }));\n\
            error = 1;\n\
        }\n\
        if(error==0)\n\
        {\n\
            \n\
            var largeImage;\n\
            var largeImageArray;\n\
            var bytesPerPixelSubImage, bytesPerPixelLargeImage, bytesInSubWidth, bytesInSubImage;\n\
            var subPixels;\n\
            var subImageWidth = 512;\n\
            var subImageHeight = 512;\n\
            var timeout = 5;\n\
            var decodingImagesPool = [];\n\
            class DecodingImage {\n\
                constructor(url) {\n\
                    this.url = url;\n\
                    this.iterations = -1;\n\
                    this.iteration = -1;\n\
                    this.aborted = false;\n\
                    this.bottomLeftPixelX = 0;\n\
                    this.bottomLeftPixelY = 0;\n\
                    this.bytesInLargeWidth = 0;\n\
                    this.bytesInLargeImage = 0;\n\
                    this.remainingWidthInLargeImage = 0;\n\
                    this.remainingLinesInLargeImage = 0;\n\
                    this.sourceOffset = 0;\n\
                    this.numChunksX = 0;\n\
                    this.numChunksY = 0;\n\
                    this.largeImageWidth = 0;\n\
                    this.largeImageHeight = 0;\n\
                }\n\
            }\n\
            function decodePNG(arraybuffer) {\n\
                return UPNG.decode(arraybuffer);\n\
            }\n\
            function decodeJPG(arrayBuffer) {\n\
                return decode(new Uint8Array(arrayBuffer), true);\n\
            }\n\
            const decoders = {\n\
                \"image/png\": decodePNG,\n\
                \"image/jpeg\": decodeJPG,\n\
                \"image/jpg\": decodeJPG,\n\
            };\n\
            \n\
            \n\
            function postError(errorMsg)\n\
            {\n\
                postMessage(JSON.stringify({\n\
                    error: errorMsg\n\
                }));\n\
            }\n\
            \n\
            function initChunkingDefaults()\n\
            {\n\
                bytesPerPixelSubImage = 3;\n\
                bytesPerPixelLargeImage = 4;\n\
                bytesInSubWidth = subImageWidth * bytesPerPixelSubImage;\n\
                bytesInSubImage = subImageHeight * bytesInSubWidth;\n\
                subPixels = new Uint8Array(bytesInSubImage);\n\
            }\n\
            initChunkingDefaults();\n\
            \n\
            function setupImageChunking(d, lWidth, lHeight)\n\
            {\n\
                d.largeImageWidth = lWidth;\n\
                d.largeImageHeight = lHeight;\n\
                d.bytesInLargeWidth = lWidth * bytesPerPixelLargeImage;\n\
                d.bytesInLargeImage = d.bytesInLargeWidth * lHeight;\n\
                d.remainingWidthInLargeImage = lWidth;\n\
                d.remainingLinesInLargeImage = lHeight;\n\
                d.sourceOffset = 0;\n\
                d.numChunksX = Math.floor(lWidth / subImageWidth);\n\
                if (lWidth % subImageWidth > 0)\n\
                {\n\
                    d.numChunksX++;\n\
                }\n\
                d.numChunksY = Math.floor(lHeight / subImageHeight);\n\
                if (lHeight % subImageHeight > 0)\n\
                {\n\
                    d.numChunksY++;\n\
                }\n\
                d.iterations = d.numChunksX * d.numChunksY;\n\
                d.bottomLeftPixelX = 0;\n\
                d.bottomLeftPixelY = 0;\n\
            }\n\
            function abortLoadingImages()\n\
            {\n\
                for(var i = 0; i < decodingImagesPool.length; ++i)\n\
                {\n\
                    decodingImagesPool[i].aborted = true;\n\
                }\n\
            }\n\
            function cleanupImagePool()\n\
            {\n\
                decodingImagesPool.length = 0;\n\
            }\n\
            async function loadNextImage(url) {\n\
            \n\
                // download image\n\
                var d = new DecodingImage(url);\n\
                abortLoadingImages();\n\
                decodingImagesPool.push(d);\n\
                try {\n\
                    // add timeout to image fetch\n\
                    const controller = new AbortController();\n\
                    const timeoutId = setTimeout(() => controller.abort(), timeout*1000);\n\
                    const res = await fetch(url, {mode: \"cors\", signal: controller.signal});\n\
                    if(d.aborted) return;\n\
                    const arrayBuffer = await res.arrayBuffer();\n\
                    // use png/ jpg decoder\n\
                    var type = res.headers.get(\"Content-Type\");\n\
                    if(!(type in decoders))\n\
                    { // get last 3 letters before ? in URL \n\
                        const fileExt = url.split(\"?\")[0].slice(-3);\n\
                        type = \"image/\" + fileExt;\n\
                    }\n\
                    const decoder = decoders[type];\n\
                    if (!decoder) {\n\
                        console.error(\"unknown image type:\", type);\n\
                    }\n\
                    if(d.aborted) return;\n\
                    // decode and get data as Uint8Array w*h*4bpp\n\
                    largeImage = decoder(arrayBuffer);\n\
                    if(d.aborted) return;\n\
                    if(decoder == decodePNG)\n\
                    {\n\
                        largeImageArray = new Uint8Array(UPNG.toRGBA8(largeImage)[0]);\n\
                    }\n\
                    else\n\
                    {\n\
                        largeImageArray = largeImage.data;\n\
                    }\n\
                } catch(err)\n\
                {\n\
                    postError(err.toString() + \" \" + url);\n\
                    d.aborted = true;\n\
                    return;\n\
                }\n\
                if(d.aborted) return;\n\
                setupImageChunking(d, largeImage.width, largeImage.height);\n\
                postMessage(\"Image load success\");\n\
            }\n\
            function currentImage() {\n\
                for(var i = decodingImagesPool.length-1; i >= 0 ; --i)\n\
                {\n\
                    var d = decodingImagesPool[i];\n\
                    if(d.aborted)\n\
                        continue;\n\
                    return d;\n\
                }\n\
                return null;\n\
            }\n\
            function copyChunk(d)\n\
            {\n\
                //console.time(\"copyChunk\");\n\
                // sourceOffset is the index into the byte array that we start copying from.\n\
                // Copy horizontal lines into the sub image until it is full, or we hit the top edge of the large image.\n\
                // Chunks fill in from left to right, bottom to top.\n\
                var hitRightEdge = false;\n\
                var hitTopEdge = false;\n\
                var lengthToCopySubImage = bytesInSubWidth;\n\
                if (d.remainingWidthInLargeImage <= subImageWidth)\n\
                {\n\
                    hitRightEdge = true;\n\
                    lengthToCopySubImage = d.remainingWidthInLargeImage * bytesPerPixelSubImage;\n\
                }\n\
                var lengthToCopyLargeImage = parseInt(lengthToCopySubImage * bytesPerPixelLargeImage/bytesPerPixelSubImage);\n\
            \n\
                var pixelsInCopiedWidth = lengthToCopySubImage / bytesPerPixelSubImage;\n\
                var destOffset = 0;\n\
                var linesToCopy = subImageHeight;\n\
                if (d.remainingLinesInLargeImage <= linesToCopy)\n\
                {\n\
                    hitTopEdge = true;\n\
                    linesToCopy = d.remainingLinesInLargeImage;\n\
                }\n\
            \n\
                var prevSourceOffset = d.sourceOffset;\n\
                // all the lines up to now + the length to copy should be <= bytesInLargeImage\n\
                if ((linesToCopy - 1) * d.bytesInLargeWidth + lengthToCopyLargeImage \n\
                    + d.sourceOffset > d.bytesInLargeImage)\n\
                    throw \"trying to read from past end of large image source (iteration \" + d.iteration + \")\";\n\
                if (linesToCopy * lengthToCopySubImage > bytesInSubImage)\n\
                    throw \"trying to write past end of destination (chunk \" + d.iteration + \")\";\n\
                for(var l = 0; l < linesToCopy; ++l)\n\
                {\n\
                    var channel = 0;\n\
                    for(var b = 0; b < lengthToCopySubImage; ++b)\n\
                    {\n\
                        subPixels[destOffset++] = largeImageArray[d.sourceOffset++];\n\
                        channel++;\n\
                        if(channel == 3)\n\
                        { // skip alpha channel for dest\n\
                            channel = 0;\n\
                            d.sourceOffset++;\n\
                        }\n\
                    }\n\
                    // move up a source line\n\
                    d.sourceOffset += d.bytesInLargeWidth - lengthToCopyLargeImage;\n\
                    // snap destOffset to start of the next line\n\
                    destOffset = Math.ceil(destOffset / bytesInSubWidth) * bytesInSubWidth;\n\
                }\n\
                d.bottomLeftPixelX += subImageWidth;\n\
                if (hitRightEdge)\n\
                {\n\
                    // snap sourceOffset to start of the next line\n\
                    d.sourceOffset = Math.floor(d.sourceOffset / d.bytesInLargeWidth) * d.bytesInLargeWidth;\n\
                    \n\
                    d.remainingWidthInLargeImage = d.largeImageWidth;\n\
                    d.bottomLeftPixelX = 0;\n\
                    if (hitTopEdge)\n\
                    {\n\
                        if(d.sourceOffset != d.bytesInLargeImage)\n\
                            throw \"Finished copying chunks but didn't copy all pixels\"\n\
                    }\n\
                    else\n\
                    {\n\
                        d.remainingLinesInLargeImage -= linesToCopy;\n\
                        d.bottomLeftPixelY += subImageHeight;\n\
                    }\n\
                }\n\
                else\n\
                {\n\
                    // go back to chunk origin\n\
                    d.sourceOffset = prevSourceOffset;\n\
                    // go to start of next chunk\n\
                    d.sourceOffset += lengthToCopyLargeImage;\n\
                    d.remainingWidthInLargeImage -= pixelsInCopiedWidth;\n\
                }\n\
                //console.timeEnd(\"copyChunk\");\n\
            }\n\
            \n\
            function loadChunk(i)\n\
            {\n\
                //console.time(\"loadChunk\");\n\
                var d = currentImage();\n\
                if(d===null)\n\
                {\n\
                    console.warn(\"no current image\");\n\
                    return;\n\
                }\n\
                if(i >= d.iterations)\n\
                {\n\
                    console.warn(\"Tried to copy a chunk (index \" + i + \") past the number of iterations (\" + d.iterations + \")\");\n\
                    return;\n\
                }\n\
                if(i <= d.iteration)\n\
                {\n\
                    console.warn(\"Tried to copy a chunk (index \" + i + \") <= the current iteration (\" + d.iteration + \")\");\n\
                    return;\n\
                }\n\
                d.iteration++;\n\
                // shader parameters for window size\n\
                const bl_x =  d.bottomLeftPixelX/d.largeImageWidth;\n\
                const bl_y =  d.bottomLeftPixelY/d.largeImageHeight;\n\
                const tr_x = (d.bottomLeftPixelX + subImageWidth) /d.largeImageWidth;\n\
                const tr_y = (d.bottomLeftPixelY + subImageHeight)/d.largeImageHeight;\n\
                copyChunk(d);\n\
                // convert to PNG encoding so we can put it in an Image on the main thread\n\
                var png = UPNG.encodeLL([subPixels.buffer], subImageWidth, subImageHeight, 3, 0, 8);\n\
                var pngBlobURL = URL.createObjectURL(new Blob([png], {type: \"image/png\"}));\n\
                const iterationsLeft = d.iterations - i - 1;\n\
                if(iterationsLeft == 0)\n\
                {\n\
                    cleanupImagePool();\n\
                }\n\
                postMessage(JSON.stringify({\n\
                    pngBlobURL: pngBlobURL,\n\
                    iterationsLeft: iterationsLeft,\n\
                    bl_x: bl_x,\n\
                    bl_y: bl_y,\n\
                    tr_x: tr_x,\n\
                    tr_y: tr_y,\n\
                    error: \"\"\n\
                }));\n\
                //console.timeEnd(\"loadChunk\");\n\
            }\n\
            self.addEventListener(\"message\",  function(e) {\n\
                const op = e.data.op;\n\
                if(op === \"LoadNextImage\")\n\
                {\n\
                    loadNextImage(e.data.url);\n\
                }\n\
                else if(op === \"LoadChunk\")\n\
                {\n\
                    loadChunk(e.data.iteration);\n\
                }\n\
                else if(op === \"Abort\")\n\
                {\n\
                    abortLoadingImages();\n\
                }\n\
                else if(op ===\"Initialize\")\n\
                {\n\
                    subImageWidth = e.data.subImageWidth;\n\
                    subImageHeight = e.data.subImageHeight;\n\
                    timeout = e.data.timeout;\n\
                }\n\
            });\n\
        }\n\
        \n";
        // create a worker to operate on images (download, decode, split into chunks, and save as png blobs)
        worker = new Worker(URL.createObjectURL(new Blob([workerDecode], {type: "text/javascript"})));
        worker.postMessage({op: "Initialize", subImageWidth: _subImageWidth, 
            subImageHeight: _subImageHeight, timeout: _timeout});
        // this is how we receive and handle messages from the worker.
        worker.addEventListener("message", function(e)
        {
            //console.time("MessageListener");
            if(e.data === "Image load success")
            { // successfully decoded the image, can start loading the next chunk
                worker.postMessage({op: "LoadChunk", iteration: iteration});
                iteration++;
            }
            else
            {
                var parsed = JSON.parse(e.data);
                if(parsed.error != "")
                {
                    if(parsed.error == "Error loading web worker decode js files")
                    {
                        errorCode = 10;
                        return;
                    }
                    console.error(parsed.error);
                    console.error("Setting errorCode to 1!!! JS");
                    errorCode = 1;
                    return;
                }
                if(iterations == -1) // first time, set no. of iterations for C# to access
                {
                    iterations = parsed.iterationsLeft + 1;
                }
                // generate an Image from the png-encoded blob generated by the worker, and add it to a list
                var img = new Image();
                img.crossOrigin = "";
                img.decoding = "async";
                img.style.display = 'none';
                img.src = parsed.pngBlobURL;
                img.decode().then(function() {
                    if(iterations < 0)
                        return;
                    document.body.appendChild(img); // by adding to the document, the browser will decode it in another thread
                    rectVector4s.push({bl_x: parsed.bl_x, bl_y: parsed.bl_y, tr_x: parsed.tr_x, tr_y: parsed.tr_y});
                    var receivedIt = iterations - parsed.iterationsLeft - 1;
                    iterationsReady.push(receivedIt);
                    imageInstances.push(img);
                    // free PNG memory
                    URL.revokeObjectURL(parsed.pngBlobURL);
                })
                if(parsed.iterationsLeft > 0 && iterations > 0)
                {
                    worker.postMessage({op: "LoadChunk", iteration: iteration});
                    iteration++;
                }
            }
            //console.timeEnd("MessageListener");
        });
        //console.timeEnd("InitializeWebWorker");
     },
     DecodeImage: function(url) {
         var str = Pointer_stringify(url);
        iteration = 0;
        iterations = -1;
        iterationsReady = [];
        if(errorCode == 10) // 10 means the web worker decode js files failed to load - big problem!
        {
            worker.terminate();
        }
        else 
        {
            errorCode = 0;
            worker.postMessage({op: "LoadNextImage", url: str, });
        }
    },
    DestroyImageLoader: function() {
        worker.terminate();
    },
    HasErrored: function() {
        return errorCode;
    }
};
autoAddDeps(LibraryImageDecodeChunks, "$iteration");
autoAddDeps(LibraryImageDecodeChunks, "$iterations");
autoAddDeps(LibraryImageDecodeChunks, "$iterationsReady");
autoAddDeps(LibraryImageDecodeChunks, "$worker");
autoAddDeps(LibraryImageDecodeChunks, '$imageInstances');
autoAddDeps(LibraryImageDecodeChunks, '$rectVector4s');
autoAddDeps(LibraryImageDecodeChunks, '$currentRectV4');
autoAddDeps(LibraryImageDecodeChunks, '$errorCode');
mergeInto(LibraryManager.library, LibraryImageDecodeChunks);