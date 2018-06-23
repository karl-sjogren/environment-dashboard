import Route from '@ember/routing/route';
import { inject } from '@ember/service';
import AuthenticatedRouteMixin from 'ember-simple-auth/mixins/authenticated-route-mixin';

export default Route.extend(AuthenticatedRouteMixin, {
  apiKeyService: inject(),

  model(params) {
    let apiKeyId = params.api_key_id;
    if(apiKeyId === 'new') {
      return { };
    } else {
      return this.apiKeyService.find(apiKeyId);
    }
  },

  actions: {
    save() {
      this.set('controller.saving', true);
      this.set('controller.error', false);

      let model = this.controller.model;
      this.apiKeyService
        .save(model)
        .then(() => {
          this.set('controller.saving', false);
          alert('The API key was saved.');
        }).catch(() => {
          this.set('controller.saving', false);
          this.set('controller.error', true);
          alert('Something when wrong when saving the API key.');
        });
    }
  }
});