import Route from '@ember/routing/route';
import { inject } from '@ember/service';
import { hash } from 'rsvp';
import AuthenticatedRouteMixin from 'ember-simple-auth/mixins/authenticated-route-mixin';

export default Route.extend(AuthenticatedRouteMixin, {
  cameraService: inject(),
  weatherService: inject(),

  model() {
    return hash({
      cameras: this.cameraService.findAll(0, 100),
      forecast: this.weatherService.forecast()
    });
  },

  setupController(controller, models) {
    controller.setProperties(models);
  }
});