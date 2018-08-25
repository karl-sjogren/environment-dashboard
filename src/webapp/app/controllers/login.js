import Controller from '@ember/controller';
import { inject } from '@ember/service';

export default Controller.extend({
  session: inject(),
  username: '',
  password: '',
  error: false,

  actions: {
    login() {
      this.set('error', false);
      let { username, password } = this.getProperties('username', 'password');
      this.get('session').authenticate('authenticator:custom', username, password)
        .then(() => {
          this.transitionToRoute('index');
        })
        .catch(() => {
          this.set('error', true);
        });
    }
  }
});
