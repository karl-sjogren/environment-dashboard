import Service from '@ember/service';
import { inject } from '@ember/service';

export default Service.extend({
  ajax: inject(),

  find(id) {
    let url = `/admin/api/sensors/${id}`;
    return this.ajax.request(url);
  },

  findAll(pageIndex, pageSize) {
    let url = `/admin/api/sensors?pageIndex=${pageIndex}&pageSize=${pageSize}`;
    return this.ajax.request(url);
  },

  remove(id) {
    let url = `/admin/api/sensors/${id}`;
    return this.ajax.delete(url);
  },

  save(model) {
    if(!!model.id) {
      let url = `/admin/api/sensors/${model.id}`;
      return this.ajax.put(url, { data: model });
    } else {
      let url = `/admin/api/sensors/`;
      return this.ajax.post(url, { data: model }).then(newModel => {
        model.id = newModel.id;
        return newModel;
      });
    }
  }
});