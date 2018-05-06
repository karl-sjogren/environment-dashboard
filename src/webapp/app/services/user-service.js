import Service from '@ember/service';
import { inject } from '@ember/service';

export default Service.extend({
  ajax: inject(),

  getUser: function(id) {
    let url = `/admin/api/user/${id}`;
    return this.get('ajax').request(url);
  },

  listUsers: function(pageIndex, pageSize) {
    let url = `/admin/api/user?pageIndex=${pageIndex}&pageSize=${pageSize}`;
    return this.get('ajax').request(url);
  },

  save(user) {
    if(!!user.id) {
      let url = `/admin/api/user/${user.id}`;
      return this.get('ajax').put(url, { data: user });
    } else {
      let url = `/admin/api/user/`;
      return this.get('ajax').post(url, { data: user }).then(newUser => {
        user.id = newUser.id;
        return newUser;
      });
    }
  },

  updatePassword(id, password) {
    let url = `/admin/api/user/${id}/password`;
    return this.get('ajax').put(url, { data: { password: password } });
  }
});