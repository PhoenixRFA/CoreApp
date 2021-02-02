import Module from './module1';

const module = new Module('account');

module.echo('Echo from account !');

if (ISDEV) {
    console.warn(`${new Date()} App in dev mode! (sended from account)`);
}

export default module;