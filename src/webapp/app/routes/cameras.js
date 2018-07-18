import Route from '@ember/routing/route';
import { inject } from '@ember/service';
import { set } from '@ember/object';
import AuthenticatedRouteMixin from 'ember-simple-auth/mixins/authenticated-route-mixin';

export default Route.extend(AuthenticatedRouteMixin, {
  cameraService: inject(),

  model() {
    return this.cameraService.findAll(0, 100);
  },

  actions: {
    removeCamera(camera) {
      this.cameraService.remove(camera.id);
      set(camera, 'deleted', true);
    }
  }
});