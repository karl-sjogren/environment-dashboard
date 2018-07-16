import Controller from '@ember/controller';
import { set } from '@ember/object';

export default Controller.extend({
  actions: {
    selectType(type) {
      set(this.model, 'type', type);
    }
  }
});