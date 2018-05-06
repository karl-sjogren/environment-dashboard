import { inject } from '@ember/service';
import { computed } from '@ember/object';
import AjaxService from 'ember-ajax/services/ajax';

export default AjaxService.extend({
  session: inject(),
  contentType: 'application/json; charset=utf-8',

  headers: computed('session.isAuthenticated', {
    get() {
      let headers = {};
      if(this.get('session.isAuthenticated')) {
        let authToken = this.get('session.data.authenticated.token');
        headers['Authorization'] = 'Bearer ' + authToken;
      }
      return headers;
    }
  }),

  isSuccess(status) {
    let isSuccess = this._super(...arguments);
    if(!isSuccess) {
      if(status === 401 && this.get('session.isAuthenticated')) {
        this.get('session').invalidate();
      }
    }
    return isSuccess;
  }
});