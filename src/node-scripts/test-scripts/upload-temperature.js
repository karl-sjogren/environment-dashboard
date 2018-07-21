const EnvironmentClient = require('../src/env-client');

const client = new EnvironmentClient(process.env.ED_API_URL, process.env.ED_API_KEY);
const sensorId = '5b53ae0c3d75134ffa1cf811';

const temperature = Math.random() * 30;
const humidity = Math.random() * 100;
return client.uploadTemperatureReading(sensorId, temperature, humidity);