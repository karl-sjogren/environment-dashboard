import Component from '@ember/component';
import { later } from '@ember/runloop';
import { inject } from '@ember/service';

export default Component.extend({
  sensorService: inject(),
  tagName: 'tr',
  graph: null,
  loading: false,
  empty: false,
  sensor: null,

  didInsertElement() {
    this._super(...arguments);

    this.loadSensorValues();
  },

  loadSensorValues() {
    if(this.loading) {
      return;
    }

    this.set('loading', true);

    this.sensorService.findValues(this.sensor.id, 0, 1).then(result => {
      this.set('loading', false);
      if(result.totalCount === 0) {
        this.set('empty', true);
        return;
      }

      this.set('model', result.items[0]);
    });
  }
});
