const fs = require('fs');
const https = require('https');



function updateMetadata() {
    const fileContents = fs.readFileSync('./extensions.json');
    const signatureJSON = JSON.parse(fileContents);

    var data = Object.keys(signatureJSON)
        .sort()
        .reduce((extensions, extension) => {
            let extensionData = signatureJSON[extension];

            extensions[extension] = extensionData;

            return extensions;
        }, {});


    Object.keys(data)
        // .filter(extension => extension === '3gp')
        .reduce((promise, extension) => {
            return promise
                .then(_ => requestInfo(extension))
                .then(detail => {
                    if (detail) {
                        data[extension] = Object.assign({}, data[extension], detail);
                    }

                    return data;
                })
        }, Promise.resolve())
        .then(_ => {
            fs.writeFileSync('./extensions-new.json', JSON.stringify(data));
        });
}

function requestInfo(extension) {
    return new Promise(function (resolve, reject) {
        https.get(`https://fileinfo.com/extension/${extension}`, res => {
            var body = '';

            res.on('data', d => {
                try {
                    body += d.toString();
                }
                catch (e) {
                    console.error(e);
                    resolve();
                }
            });

            res.on('end', _ => {
                const matches = /<span itemprop="name">(.*?)<\/span>/.exec(body);
                if (matches && matches.length > 1) {
                    resolve({
                        'name': matches[1]
                    })
                }
                else {
                    console.warn(`no match for ${extension}`)
                    resolve();
                }
            });
        })
            .on('error', e => {
                console.error(e);
                resolve();
            });
    });
}