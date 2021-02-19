window.addEventListener('load', function() {
    installSW();

    navigator.serviceWorker.addEventListener('message', event => {
        console.log('Received data from Sevice Worker:', event.data);
        console.debug('Received event from Sevice Worker:', event);
    });

    navigator.serviceWorker.addEventListener('controllerchange', event => {
        console.log('Sevice Worker controller change:', event);
        //console.debug('Sevice Worker controller change:', event);
    });

    navigator.connection.addEventListener('change', event => {
        console.log('Internet connection changed: %c%s %c%s speed: %o mbps', (navigator.onLine ? 'color: green;' : 'color: red;'), (navigator.onLine ? 'online' : 'offline'), '', event.target.effectiveType, event.target.downlink);
    });

    runWorker();
    runSharedWorker();
    broadcastMessage('some data');
});

function installSW() {
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register('/sw.js', { scope: './' })
            .then(event => {
                console.log('Service worker registered for scope:', event.scope, event);
                
                let serviceWorker;
                if (event.installing) {
                    serviceWorker = event.installing;
                    document.querySelector('#sw-status').textContent = 'installing';
                } else if (event.waiting) {
                    serviceWorker = event.waiting;
                    document.querySelector('#sw-status').textContent = 'waiting';
                } else if (event.active) {
                    serviceWorker = event.active;
                    document.querySelector('#sw-status').textContent = 'active';
                }

                if (serviceWorker) {
                    window._sw = serviceWorker;
                    console.log('service worker state:', serviceWorker.state);
                    serviceWorker.addEventListener('statechange', function(e) {
                        console.log('service worker state change:', e.target.state);
                    });
                }
            })
            .catch(err => { console.error('Error on service worker registration', err); });
    } else {
        console.error('ServiceWorker is not supported');
    }
}

function loadAllImages() {
    loadImage('/img/sw1.png');
    loadImage('/img/sw2.png');
    loadImage('/img/sw3.png');
}

function loadImage(src) {
    fetch(src).then(resp => resp.blob()).then(blob => {
        const img = document.createElement('img');
        const imgUrl = window.URL.createObjectURL(blob);
        img.src = imgUrl;
        img.style.display = 'block';
        img.style.maxWidth = '500px';
        img.style.borderRadius = '10px';
        img.style.boxShadow = '0 0 5px 4px rgba(0, 0, 0, .1)';
        img.style.margin = '10px 0 20px';

        const wrap = document.getElementById('images-wrap');
        wrap.append(img);
    });
}

function sendDataToSW(data) {
    console.log('Send data to Sevice Worker:', data);
    navigator.serviceWorker.controller.postMessage(data);
}

//only when app is installed!
function registerPeriodicTask(tag) {
    console.log('Register periodic task:', tag);
    navigator.permissions.query({ name: 'periodic-background-sync' })
        .then(permission=> console.log('Periodic Background Sync:', permission.state));

    navigator.serviceWorker.ready
        .then(reg =>                                    //5 sec
            reg.periodicSync.register(tag, { minInterval: 5 * 1000 })
                .catch(err => console.warn('registration periodic task error', err))
        );
}

function checkPeriodicTask() {
    navigator.serviceWorker.ready.then(reg => 
        reg.periodicSync.getTags().then(tags => console.log('Periodic sync tasks:', tags))
    );
}

function removePeriodicTask(tag) {
    navigator.serviceWorker.ready.then(reg => 
        reg.periodicSync.unregister(tag)
        .then(res => console.log('Periodic task removed', res))
        .catch(err => console.warn('error on unregister priodic task', err))
    );
}



function registerSync(tag) {
    console.log('Register sync:', tag);

    navigator.serviceWorker.ready
        .then(reg =>
            reg.sync.register(tag, {
                allowOnBattery: true,
                id: tag + '' + Date.now(),
                idleRequired: false,
                maxDelay: 0, //without max delay
                minDelay: 0,
                minPeriod: 0,
                minRequiredNetwork: 'network-online'
            })
            .catch(err => console.warn('registration sync error', err))
        );
}

function checkSync() {
    navigator.serviceWorker.ready.then(reg => 
        reg.sync.getTags().then(tags => console.log('Sync tasks:', tags))
    );
}


function runWorker(param) {
    if (!window.Worker) return;

    const myWorker = new Worker('/js/webWorker.js');

    myWorker.onerror = console.warn;
    myWorker.onmessageerror = console.warn;

    myWorker.onmessage = function(event) {
        console.log('Received data from worker:', event.data);
        myWorker.terminate();
    };

    const data = { foo: 1, bar: 3.14 };
    console.log('Post message to worker:', { data: param || data });
    myWorker.postMessage(param || data);
}

function runSharedWorker(param) {
    if (!window.SharedWorker) return;
    
    const sharedWorker = new SharedWorker('/js/sharedWorker.js');
    
    sharedWorker.onerror = console.warn;
    sharedWorker.port.onmessageerror = console.warn;
    
    sharedWorker.port.onmessage = function(event) {
        console.log('Received data from shared worker:', event.data);
        
        if (event.data === 'close') {
            sharedWorker.port.close();
        }
    };

    const data = { foo: 1, bar: 3.14 };
    console.log('Post message to shared worker:', { data: param || data });
    sharedWorker.port.start();
    sharedWorker.port.postMessage(param || data);
}

function broadcastMessage(msg) {
    if (!window.SharedWorker) return;
    
    const bc = new BroadcastChannel('test_channel');

    const data = { msg, name };
    console.log('Broadcast:', data);
    bc.postMessage(data);

    bc.onmessage = function(e) {
        console.log('Received broadcasted message:', e.data);

        if (event.data === 'close') {
            bc.close();
        }
    }
}