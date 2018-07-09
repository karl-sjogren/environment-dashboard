const EnvironmentClient = require('../src/env-client');

const client = new EnvironmentClient(process.env.ED_API_URL, process.env.ED_API_KEY);
return client.uploadPhoto('./sample-image.jpg');