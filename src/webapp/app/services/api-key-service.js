import Service from '@ember/service';
import { inject } from '@ember/service';

export default Service.extend({
  ajax: inject(),

  getApiKey: function(id) {
    let url = `/admin/api/api-keys/${id}`;
    return this.get('ajax').request(url);
  },

  listApiKeys: function(pageIndex, pageSize) {
    let url = `/admin/api/api-keys?pageIndex=${pageIndex}&pageSize=${pageSize}`;
    return this.get('ajax').request(url);
  },

  removeApiKey: function(id) {
    let url = `/admin/api/api-keys/${id}`;
    return this.get('ajax').delete(url);
  },

  save(apiKey) {
    if(!!apiKey.id) {
      let url = `/admin/api/api-keys/${apiKey.id}`;
      return this.get('ajax').put(url, { data: apiKey });
    } else {
      let url = `/admin/api/api-keys/`;
      return this.get('ajax').post(url, { data: apiKey }).then(newApiKey => {
        apiKey.id = newApiKey.id;
        return newApiKey;
      });
    }
  }
});