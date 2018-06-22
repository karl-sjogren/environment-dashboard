import EnvironmentClient from '../src/env-client';
import Promise from 'bluebird';

var readFile = Promise.promisify(require("fs").readFile);

let client = new EnvironmentClient(process.env.API_URL, process.env.API_KEY);

readFile('sample-image.jpg')
  .then(result => {
    return client.uploadPhoto(result);
  }).catch(error => {

  });