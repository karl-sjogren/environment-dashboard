import AjaxService from 'ember-ajax/services/ajax';
import { computed } from '@ember/object';
import { inject } from '@ember/service';

export default AjaxService.extend({
  session: inject(),
  contentType: 'application/json; charset=utf-8',

  headers: computed('session.isAuthenticated', {
    get() {
      let headers = {};
      if(!!this.session.isAuthenticated) {
        let authToken = this.session.data.authenticated.token;
        headers['Authorization'] = 'Bearer ' + authToken;
      }
      return headers;
    }
  }),

  isSuccess(status) {
    let isSuccess = this._super(...arguments);
    if(!isSuccess) {
      if(status === 401 && this.session.isAuthenticated) {
        this.session.invalidate();
      }
    }
    return isSuccess;
  }
});