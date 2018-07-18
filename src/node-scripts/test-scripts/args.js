const argv = require('yargs').argv;

const cameraId = argv.cameraId;
const hflip = argv.hflip;
const vflip = argv.vflip;

console.log('cameraId: ' + cameraId);
console.log('hflip: ' + hflip);
console.log('vflip: ' + vflip);
