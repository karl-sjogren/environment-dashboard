import PiCamera from 'pi-camera';
import EnvironmentClient from './env-client';

let client = new EnvironmentClient(process.env.API_URL, process.env.API_KEY);

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
    console.error(error);
  });