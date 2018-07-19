import Component from '@ember/component';
import { computed } from '@ember/object';

export default Component.extend({
  classNames: ['yr-forecast'],
  expanded: false,
  tagName: 'article',

  firstPeriod: computed('forecast', function() {
    if(!this.forecast) {
      return;
    }

    let period = this.forecast.timeperiods[0];

    return period;
  }),

  actions: {
    toggleExpanded() {
      this.toggleProperty('expanded');
    }
  }
});