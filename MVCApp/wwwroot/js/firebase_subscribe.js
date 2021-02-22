firebase.initializeApp({
    messagingSenderId: '366688769796'
});
let messaging;

if ('Notification' in window) {
    messaging = firebase.messaging();

    if (Notification.permission === 'granted') {
        subscribe();
    }

    $('#subscribe').on('click', function () {
        subscribe();
    });
}

function subscribe() {
    messaging.requestPermission()
        .then(function () {
            messaging.getToken()
                .then(function (currentToken) {
                    console.log({ currentToken });

                    if (currentToken) {
                        sendTokenToServer(currentToken);
                    } else {
                        console.warn('Error on token receiving');
                        setTokenSentToServer(false);
                    }
                })
                .catch(function (err) {
                    console.warn('Error on token receiving', err);
                    setTokenSentToServer(false);
                });
        })
        .catch(function (err) {
            console.warn('Notification access isnt granted', err);
        });
}

function sendTokenToServer(currentToken) {
    if (!isTokenSentToServer(currentToken)) {
        console.log('Sending token to server');

        const url = '';
        //$.post(url, {
        //    token: currentToken
        //});

        setTokenSentToServer(currentToken);
    } else {
        console.log('Token already sent to the server');
    }
}

function isTokenSentToServer(currentToken) {
    return window.localStorage.getItem('sentFirebaseMessagingToken') == currentToken;
}

function setTokenSentToServer(currentToken) {
    window.localStorage.setItem(
        'sentFirebaseMessagingToken',
        currentToken ? currentToken : ''
    );
}