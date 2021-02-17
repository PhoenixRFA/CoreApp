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

window.addEventListener('load', function() {
    installSW();
});

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