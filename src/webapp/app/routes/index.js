import Route from '@ember/routing/route';
import { inject } from '@ember/service';
import { hash } from 'rsvp';
import AuthenticatedRouteMixin from 'ember-simple-auth/mixins/authenticated-route-mixin';

export default Route.extend(AuthenticatedRouteMixin, {
  cameraService: inject(),
  sensorService: inject(),
  weatherService: inject(),

  model() {
    return hash({
      cameras: this.cameraService.findAll(0, 100),
      forecast: this.weatherService.forecast(),
      sensors: this.sensorService.findAll(0, 100)
    });
  },

  setupController(controller, models) {
    controller.setProperties(models);
  }
});