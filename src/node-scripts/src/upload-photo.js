const PiCamera = require('pi-camera');
const os = require('os');
const EnvironmentClient = require('../src/env-client');

const client = new EnvironmentClient(process.env.ED_API_URL, process.env.ED_API_KEY);

const imagePath = `${os.tmpdir()}/tmp-photo.jpg`;

const camera = new PiCamera({
  mode: 'photo',
  output: imagePath,
  width: 1280,
  height: 720,
  nopreview: true
});

camera.snap()
  .then(result => {
    return client.uploadPhoto(imagePath);
  }).catch(error => {
    console.error(error);
  });