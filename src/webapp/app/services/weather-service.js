import Service from '@ember/service';
import { inject } from '@ember/service';

export default Service.extend({
  ajax: inject(),

  forecast() {
    let url = '/admin/api/weather/forecast';
    return this.ajax.request(url);
  }
});