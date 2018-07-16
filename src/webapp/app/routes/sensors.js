import Route from '@ember/routing/route';
import { inject } from '@ember/service';
import { set } from '@ember/object';
import AuthenticatedRouteMixin from 'ember-simple-auth/mixins/authenticated-route-mixin';

export default Route.extend(AuthenticatedRouteMixin, {
  sensorService: inject(),

  model() {
    return this.sensorService.findAll(0, 100);
  },

  actions: {
    removeSensor(sensor) {
      this.sensorService.remove(sensor.id);
      set(sensor, 'deleted', true);
    }
  }
});