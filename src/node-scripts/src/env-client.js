const fetch = require('node-fetch');
const url = require('url');
const createReadStream = require('fs').createReadStream;

module.exports = class EnvironmentClient {
  constructor(baseUrl, apiKey) {
    this.baseUrl = baseUrl;
    this.apiKey = apiKey;   
  }

  uploadPhoto(cameraId, path) {
    const stream = createReadStream(path);
    let uploadUrl = url.resolve(this.baseUrl, `/admin/api/cameras/${cameraId}`);
    console.info(`Uploading binary to ${uploadUrl}.`);
    return fetch(uploadUrl, {
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

  uploadTemperatureReading(sensorId, temperature, humidity) {
    let uploadUrl = url.resolve(this.baseUrl, `/admin/api/sensors/${sensorId}`);
    console.info(`Uploading temperature reading to ${uploadUrl}.`);
    return fetch(uploadUrl, {
      method: 'POST',
      headers: {
        'Authorization': 'Bearer ' + this.apiKey,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        sensorId: sensorId,
        temperature: temperature,
        humidity: humidity
      })
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