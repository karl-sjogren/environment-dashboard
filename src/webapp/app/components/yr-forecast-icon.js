import Component from '@ember/component';
import { computed } from '@ember/object';

export default Component.extend({
  classNames: ['yr-forecast-icon'],
  attributeBindings: ['src'],
  tagName: 'img',

  src: computed('period', function() {
    let period = this.period;

    if(!period) {
      return;
    }

    return `/assets/yr-icons/${period.yrIcon}.svg`
  })
});