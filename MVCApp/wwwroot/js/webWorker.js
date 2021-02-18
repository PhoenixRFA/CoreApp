//onmessage = function(e) {
//    console.log('[Worker] Message received from main script', e.data);

//    const res = processData(e.data);

//    postMessage({ result: res });
//}

self.addEventListener('message', function(e) {
    console.log('[Worker] Message received from main script', e.data);

    if (typeof e.data === 'string') {
        postMessage(e.data);
        return;
    }

    const res = processData(e.data);

    postMessage({ result: res });
});


function processData(data) {
    if (typeof data !== 'object') {
        console.warn('[Worker] Not object passed');
        return null;
    }

    const res = [];
    for (let key in data) {
        if (Object.hasOwnProperty.call(data, key)) {
            res.push({ key, value: data[key] });
        }
    }

    return res;
}