import Route from '@ember/routing/route';
import { inject } from '@ember/service';
import AuthenticatedRouteMixin from 'ember-simple-auth/mixins/authenticated-route-mixin';

export default Route.extend(AuthenticatedRouteMixin, {
  session: inject(),
  userService: inject(),

  model() {
    return this.userService.find(this.session.data.authenticated.userId);
  },

  actions: {
    save() {
      let model = this.controller.model;

      this.set('controller.accountChanged', false);
      this.set('controller.accountChangeFailed', false);

      this.userService.save(model).then(() => {
        this.set('controller.accountChanged', true);
      }).catch(() => {
        this.set('controller.accountChangeFailed', true);
      });
    },

    updatePassword() {
      let model = this.controller.model;
      let password = this.controller.newPassword;

      this.set('controller.passwordChanged', false);
      this.set('controller.passwordChangeFailed', false);

      this.userService.updatePassword(model.id, password).then(() => {
        this.set('controller.passwordChanged', true);
      }).catch(() => {
        this.set('controller.passwordChangeFailed', true);
      });
    }
  }
});