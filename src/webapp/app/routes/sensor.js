import Route from '@ember/routing/route';
import { inject } from '@ember/service';
import AuthenticatedRouteMixin from 'ember-simple-auth/mixins/authenticated-route-mixin';

export default Route.extend(AuthenticatedRouteMixin, {
  sensorService: inject(),

  model(params) {
    let sensorId = params.sensor_id;
    if(sensorId === 'new') {
      return { };
    } else {
      return this.sensorService.find(sensorId);
    }
  },

  actions: {
    save() {
      this.set('controller.saving', true);
      this.set('controller.error', false);

      let model = this.controller.model;
      this.sensorService
        .save(model)
        .then(() => {
          this.set('controller.saving', false);
          alert('The sensor was saved.');
        }).catch(() => {
          this.set('controller.saving', false);
          this.set('controller.error', true);
          alert('Something when wrong when saving the sensor.');
        });
    }
  }
});