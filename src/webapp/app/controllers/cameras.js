import Controller from '@ember/controller';
import { computed } from '@ember/object';

export default Controller.extend({
  cameraToRemove: null,
  confirmDialogOpen: false,

  cameraToRemoveName: computed('cameraToRemove', function() {
    let camera = this.cameraToRemove;

    if(!camera) {
      return '';
    }

    return `the camera named ${camera.name}`;
  }),

  actions: {
    showConfirmDialog(camera) {
      this.setProperties({
        cameraToRemove: camera,
        confirmDialogOpen: true
      });
    },

    confirmRemove() {
      this.send('removeCamera', this.cameraToRemove);
      this.setProperties({
        cameraToRemove: null,
        confirmDialogOpen: false
      });
    },

    closeConfirmDialog() {
      this.setProperties({
        cameraToRemove: null,
        confirmDialogOpen: false
      });
    }
  }
});