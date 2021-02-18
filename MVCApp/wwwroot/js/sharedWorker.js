let connections = 0;

self.addEventListener('connect', function(e) {
    const port = e.ports[0];
    connections++;
    //const count = e.ports.length;

    port.onmessage = function(msg) {
        if (typeof msg.data === 'string') {
            port.postMessage({ data: msg.data, connections: connections });
        }

        const workerResult = processData(msg.data);
        port.postMessage(workerResult);
    }

    //port.onmessage = function(msg) {
    //    port.postMessage({
    //        name: msg.name,
    //        data: msg.data,
    //        evId: msg.lastEventId,
    //        origin: msg.origin,
    //        ports: msg.ports,
    //        source: msg.source,
    //        count
    //    });
    //}

    port.start();
});

//onconnect = function(e) {
//    const port = e.ports[0];

//    port.onmessage = function(e) {

//        const workerResult = processData(e.data);
//        port.postMessage(workerResult);
//    }

//    port.start();
//}


function processData(data) {
    if (typeof data !== 'object') {
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