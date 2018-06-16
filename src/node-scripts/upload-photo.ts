import PiCamera from 'pi-camera';
import EnvironmentClient from './src/env-client';

let client = new EnvironmentClient('https://stugan.herokuapp.com/', '1234');

let camera = new PiCamera({
  mode: 'photo',
  output: `${__dirname}/tmp-photo.jpg`,
  width: 1280,
  height: 720,
  nopreview: true,
});

camera.snap()
  .then(result => {
    return client.uploadPhoto(result);
  }).catch(error => {

  });