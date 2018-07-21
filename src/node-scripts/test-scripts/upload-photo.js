const EnvironmentClient = require('../src/env-client');

const client = new EnvironmentClient(process.env.ED_API_URL, process.env.ED_API_KEY);
const cameraId = '5b4fb843c2bdad205e303257';

return client.uploadPhoto(cameraId, './sample-image.jpg');