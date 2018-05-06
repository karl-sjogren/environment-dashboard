import Route from '@ember/routing/route';
import { inject } from '@ember/service';
import { set } from '@ember/object';
import AuthenticatedRouteMixin from 'ember-simple-auth/mixins/authenticated-route-mixin';

export default Route.extend(AuthenticatedRouteMixin, {
  apiKeyService: inject(),

  model() {
    return this.get('apiKeyService').listApiKeys(0, 100);
  },

  actions: {
    removeApiKey(apiKey) {
      this.get('apiKeyService').removeAuthor(apiKey.id);
      set(apiKey, 'deleted', true);
    }
  }
});