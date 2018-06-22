import fetch from 'node-fetch';
import url from 'url';

export default class EnvironmentClient {
  constructor(baseUrl, apiKey) {
    this.baseUrl = baseUrl;
    this.apiKey = apiKey;   
  }

  uploadPhoto(buffer) {
    return fetch(url.resolve(this.baseUrl, '/admin/api/images/image-stream'), { // Your POST endpoint
      method: 'POST',
      headers: {
        'Authorization': 'Bearer ' + this.apiKey,
        'Content-Type': 'image/jpeg'
      },
      body: binary
    })
    .then(result => result.json())
    .then(result => {
      console.info('Successfully uploaded binary to dashboard.');
      return result;
    })
    .catch(error => {
      console.error('An error occured while uploading a binary file.', error);
    });
  }
}