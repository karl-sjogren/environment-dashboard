import Controller from '@ember/controller';
import { computed } from '@ember/object';

export default Controller.extend({
  apiKeyToRemove: null,
  confirmDialogOpen: false,

  apiKeyToRemoveName: computed('apiKeyToRemove', function() {
    let apiKey = this.get('apiKeyToRemove');

    if(!apiKey) {
      return '';
    }

    return `nyckeln tillhörande ${apiKey.firstName} ${apiKey.lastName}`;
  }),

  actions: {
    showConfirmDialog(apiKey) {
      this.setProperties({
        apiKeyToRemove: apiKey,
        confirmDialogOpen: true
      });
    },

    confirmRemove() {
      this.send('removeApiKey', this.get('apiKeyToRemove'));
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