import Route from '@ember/routing/route';
import { inject } from '@ember/service';
import AuthenticatedRouteMixin from 'ember-simple-auth/mixins/authenticated-route-mixin';

export default Route.extend(AuthenticatedRouteMixin, {
  cameraService: inject(),

  model(params) {
    let cameraId = params.camera_id;
    if(cameraId === 'new') {
      return { };
    } else {
      return this.cameraService.find(cameraId);
    }
  },

  actions: {
    save() {
      this.set('controller.saving', true);
      this.set('controller.error', false);

      let model = this.controller.model;
      this.cameraService
        .save(model)
        .then(() => {
          this.set('controller.saving', false);
          alert('The camera was saved.');
        }).catch(() => {
          this.set('controller.saving', false);
          this.set('controller.error', true);
          alert('Something when wrong when saving the camera.');
        });
    }
  }
});