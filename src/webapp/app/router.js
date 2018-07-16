import EmberRouter from '@ember/routing/router';
import config from './config/environment';

const Router = EmberRouter.extend({
  location: config.locationType,
  rootURL: config.rootURL
});


Router.map(function() {
  this.route('login');
  this.route('account');

  this.route('api-keys');
  this.route('api-key', { path: '/api-key/:api_key_id' });

  this.route('sensors');
  this.route('sensor', { path: '/sensor/:sensor_id' });
});

export default Router;
