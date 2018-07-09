const fetch = require('node-fetch');
const url = require('url');
const createReadStream = require('fs').createReadStream;

module.exports = class EnvironmentClient {
  constructor(baseUrl, apiKey) {
    this.baseUrl = baseUrl;
    this.apiKey = apiKey;   
  }

  uploadPhoto(path) {
    const stream = createReadStream(path);
    return fetch(url.resolve(this.baseUrl, '/admin/api/images/image-stream'), { // Your POST endpoint
      method: 'POST',
      headers: {
        'Authorization': 'Bearer ' + this.apiKey,
        'Content-Type': 'image/jpeg'
      },
      body: stream
    })
    .then(result => {
      console.info('Successfully uploaded binary to dashboard.');
      return result;
    })
    .catch(error => {
      console.error('An error occured while uploading a binary file.', error);
    });
  }
}