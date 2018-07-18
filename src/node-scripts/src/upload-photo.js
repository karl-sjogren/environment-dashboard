const PiCamera = require('pi-camera');
const os = require('os');
const argv = require('yargs').argv;
const EnvironmentClient = require('../src/env-client');

const client = new EnvironmentClient(process.env.ED_API_URL, process.env.ED_API_KEY);

const imagePath = `${os.tmpdir()}/tmp-photo.jpg`;

const cameraId = argv.cameraId;
const hflip = argv.hflip;
const vflip = argv.vflip;

const camera = new PiCamera({
  mode: 'photo',
  output: imagePath,
  width: 1280,
  height: 720,
  nopreview: true,
  hflip: hflip,
  vflip: vflip
});

camera.snap()
  .then(() => {
    return client.uploadPhoto(cameraId, imagePath);
  }).catch(error => {
    console.error(error);
  });