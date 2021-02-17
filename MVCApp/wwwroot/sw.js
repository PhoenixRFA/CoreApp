self.addEventListener('install', event => {
    console.log('Installing [Service Worker]', event);

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

    //get caches
    caches.keys().then(keys => console.log(keys));
    //get cache data
    caches.open('v1').then(cache => cache.keys()).then(res => console.log(res));
    //delete cache
    //caches.delete('v1');
});

self.addEventListener('fetch', event => {
    const url = new URL(event.request.url);
    if (url.pathname.toLocaleLowerCase() === '/home/serviceworker') return;

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