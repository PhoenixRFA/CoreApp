self.addEventListener('install', event => {
    console.log('Installing [Service Worker]', event);

    self.skipWaiting();

    //задержать service worker в состоянии installing 
    event.waitUntil(
        caches.open('v1').then(cache => {
            cache.addAll([
                '/img/sw1.png',
                '/img/sw2.png',
                '/img/sw3.png'
            ]);

            //т.е. установка задержится на 1 сек.
            return new Promise((resolve, reject) => setTimeout(() => resolve(), 1000));
        })
    );
});

self.addEventListener('activate', event => {
    console.log('Service Worker installed and ready to handle fetches!', event);

    clients.claim();

    //get caches
    caches.keys().then(keys => console.log(keys));
    //get cache data
    caches.open('v1').then(cache => cache.keys()).then(res => console.log(res));
    //delete cache
    //caches.delete('v1');
});

const ignoreUrls = ['/home/serviceworker', '/js/serviceworker.js', '/js/indexeddb.js'];

self.addEventListener('fetch', event => {
    const url = new URL(event.request.url);
    const path = url.pathname.toLocaleLowerCase();
    if (ignoreUrls.includes(path)) return;

    console.debug('new fetch', url.toString(), event);

    event.respondWith(
        //need to return response
        //new Response('Hello from your friendly neighbourhood service worker!')

        //if cache hit - return from cache, otherwise - make request
        caches.match(event.request).then(resp => resp || fetch(event.request)
            .then(resp => 
                //if we miss cache - make request and cache response
                caches.open('v1').then(cache => {
                    cache.put(event.request, resp.clone());
                    return resp;
                })
            ))
    );
});

self.addEventListener('message', event => {
    console.log('Service Worker receive message:', { data: event.data });
    console.debug('Service Worker receive message event:', event);

    if (typeof event.data === 'object' && event.data.hasOwnProperty('command')) {
        console.log(event.data.command);
        switch(event.data.command) {
            case 'clients':
                clients.matchAll()
                    .then(clients => console.log(clients));
                
                //works only if click on push
                //clients.openWindow('https://localhost:44324/Home/ServiceWorker');
                //clients.matchAll().then(clients => client[0].focus());
                break;
            case 'ping':
                clients.matchAll({ type: 'window' })
                    .then(clients =>
                        clients.forEach(client => client.postMessage('pong'))
                    );
                break;
        }
    }
});

self.addEventListener('periodicsync', event => {
    console.log('Periodic sync event:', event);
});

self.addEventListener('sync', event => {
    console.log('Sync event:', event.tag);
    console.debug('Sync event:', event);
});