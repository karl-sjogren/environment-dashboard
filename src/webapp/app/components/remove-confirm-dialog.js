import Component from '@ember/component';

export default Component.extend({
  classNames: ['remove-confirm-dialog'],
  open: false,
  itemName: '',

  actions: {
    closeDialog() {
      this.get('closeAction')();
    },

    confirm() {
      this.get('confirmAction')();
    }
  }
});