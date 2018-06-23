import { inject } from '@ember/service';
import { resolve, reject } from 'rsvp';
import Base from 'ember-simple-auth/authenticators/base';

export default Base.extend({
  ajax: inject(),

  restore(data) {
    if(!data && !data.userId) {
      return reject();
    }

    let url = `/admin/api/user/${data.userId}`;
    let authToken = data.token;

    return this.ajax.request(url, { headers: { 'Authorization': 'Bearer ' + authToken } }).then(() => {
      return data;
    }).catch(() => {
      return reject();
    });
  },

  authenticate(username, password) {
    let url = '/admin/api/user/authenticate';
    let json = JSON.stringify({
      username: username,
      password: password
    });
    
    return this.ajax.post(url, { data: json });
  },

  invalidate(/*data*/) {
    return resolve();
  }
});