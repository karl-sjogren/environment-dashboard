import Controller from '@ember/controller';
import { computed } from '@ember/object';

export default Controller.extend({
  apiKeyToRemove: null,
  confirmDialogOpen: false,

  apiKeyToRemoveName: computed('apiKeyToRemove', function() {
    let apiKey = this.apiKeyToRemove;

    if(!apiKey) {
      return '';
    }

    return `the key belonging to ${apiKey.name}`;
  }),

  actions: {
    showConfirmDialog(apiKey) {
      this.setProperties({
        apiKeyToRemove: apiKey,
        confirmDialogOpen: true
      });
    },

    confirmRemove() {
      this.send('removeApiKey', this.apiKeyToRemove);
      this.setProperties({
        apiKeyToRemove: null,
        confirmDialogOpen: false
      });
    },

    closeConfirmDialog() {
      this.setProperties({
        apiKeyToRemove: null,
        confirmDialogOpen: false
      });
    }
  }
});