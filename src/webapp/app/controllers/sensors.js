import Controller from '@ember/controller';
import { computed } from '@ember/object';

export default Controller.extend({
  sensorToRemove: null,
  confirmDialogOpen: false,

  sensorToRemoveName: computed('sensorToRemove', function() {
    let sensor = this.sensorToRemove;

    if(!sensor) {
      return '';
    }

    return `the key belonging to ${sensor.name}`;
  }),

  actions: {
    showConfirmDialog(sensor) {
      this.setProperties({
        sensorToRemove: sensor,
        confirmDialogOpen: true
      });
    },

    confirmRemove() {
      this.send('removeSensor', this.sensorToRemove);
      this.setProperties({
        sensorToRemove: null,
        confirmDialogOpen: false
      });
    },

    closeConfirmDialog() {
      this.setProperties({
        sensorToRemove: null,
        confirmDialogOpen: false
      });
    }
  }
});