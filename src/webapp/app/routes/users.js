import Route from '@ember/routing/route';
import { inject } from '@ember/service';
import { set } from '@ember/object';
import AuthenticatedRouteMixin from 'ember-simple-auth/mixins/authenticated-route-mixin';

export default Route.extend(AuthenticatedRouteMixin, {
  userService: inject(),

  model() {
    return this.userService.findAll(0, 100);
  },

  actions: {
    removeUser(user) {
      this.userService.remove(user.id);
      set(user, 'deleted', true);
    }
  }
});