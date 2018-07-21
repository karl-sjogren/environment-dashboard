const argv = require('yargs').argv;
const sensor = require('node-dht-sensor');
const EnvironmentClient = require('../src/env-client');

const client = new EnvironmentClient(process.env.ED_API_URL, process.env.ED_API_KEY);

const sensorId = argv.sensorId;
const sensorType = argv.sensorType;
const sensorPin = argv.sensorPin;

sensor.read(sensorType, sensorPin, (err, temperature, humidity) => {
  if(!!err) {
    console.error(err);
    return;
  }
  client.uploadTemperatureReading(sensorId, temperature, humidity);
});