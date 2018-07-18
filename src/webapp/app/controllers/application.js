import Controller from '@ember/controller';
import { inject } from '@ember/service';

export default Controller.extend({
  session: inject(),
  menuOpen: false,

  actions: {
    invalidateSession() {
      this.get('session').invalidate();
    },
    toggleMenu() {
      this.toggleProperty('menuOpen');
    }
  }
});
