window.idb = (function() {
    const dbFactory = window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB || window.msIndexedDB;
    
    const self = {
        open (name, version, successCallback, upgradeCallback, errorCallback) {
            const openRequest = dbFactory.open(name, version);

            openRequest.onerror = errorCallback || (event => console.error(`Error on opening IndexedDB [${name}] v${version}`, event));
            
            openRequest.onsuccess = function(event) {
                console.log(`IndexedDB [${name}] v${version} initialised`);
                successCallback && successCallback(this.result);
            }

            //Fired when an attempt was made to open a database with a version number higher than its current version
            openRequest.onupgradeneeded = function(event) {
                const db = this.result;
                db.onerror = console.error(`Error on upgrading IndexedDB [${name}] v${version}`, event);
                db.onversionchange = () => {
                    console.warn(`Database [${name}] v${version} version was changed. Current connection is closed!`);
                    db.close();
                };

                upgradeCallback && upgradeCallback(db);
            }

            //Fired when an open connection to a database is blocking a versionchange transaction on the same database
            openRequest.onblocked = function(event) {
                console.warn(`IndexedDB [${name}] v${version} is blocked`, event);
            }
        },

        delete(name, callback) {
            const deleteRequest = dbFactory.deleteDatabase(name);
            deleteRequest.onerror = event => {
                console.error(`Error on deleting IndexedDB [${name}]`, event);
                callback && callback(event);
            }
            deleteRequest.onsuccess = event => {
                console.log(`IndexedDB [${name}] v${version} is deleted`, event);
                callback && callback();
            }
        }
    };
    return self;
})();

window.addEventListener('load', function() {
    idb.open('test_db', 2, doSomeWork, initDb);

    
});

function doSomeWork(db) {
    const writeTransaction = db.transaction('items', 'readwrite');
    let itemsStore = writeTransaction.objectStore('items');
    
    let request = itemsStore.add({
        id: 1,
        created: new Date(),
        name: 'Item 1'
    });
    request.onsuccess = () => console.log('Item 1 is added to items store');

    itemsStore.add({
        id: 2,
        created: new Date(),
        name: 'Item 2'
    });

    writeTransaction.commit();

    const readTransaction = db.transaction('items', 'readonly');
    itemsStore = readTransaction.objectStore('items');

    request = itemsStore.count();
    request.onsuccess = event => console.log(`There is ${event.target.result} items`);

    request = itemsStore.get(1);
    request.onsuccess = event => console.log('Item 1:', event.target.result);
}
function initDb(db) {
    console.log('db object store names', db.objectStoreNames);
    
    if (!db.objectStoreNames.contains('items')) {
        db.createObjectStore('items', { keyPath: 'id', autoIncrement: true });
    }
}