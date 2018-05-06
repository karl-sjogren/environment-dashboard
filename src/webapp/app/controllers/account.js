import Controller from '@ember/controller';
import { computed } from '@ember/object';

export default Controller.extend({
  newPassword: '',
  newPasswordRepeat: '',

  accountChanged: false,
  accountChangeFailed: false,

  passwordChanged: false,
  passwordChangeFailed: false,

  changePasswordDisabled: computed('newPassword', 'newPasswordMismatch', 'newPasswordComplexityTooLow', function() {
    let newPassword = this.get('newPassword');
    let newPasswordMismatch = this.get('newPasswordMismatch');
    let newPasswordComplexityTooLow = this.get('newPasswordComplexityTooLow');

    if(newPassword.length === 0) {
      return true;
    }

    if(newPasswordMismatch) {
      return true;
    }

    if(newPasswordComplexityTooLow) {
      return true;
    }

    return false;
  }),

  newPasswordMismatch: computed('newPassword', 'newPasswordRepeat', 'newPasswordComplexityTooLow', function() {
    let newPassword = this.get('newPassword');
    let newPasswordRepeat = this.get('newPasswordRepeat');
    let newPasswordComplexityTooLow = this.get('newPasswordComplexityTooLow');

    if(newPasswordComplexityTooLow) {
      return false;
    }

    if(newPassword.length === 0) {
      return false;
    }

    return newPassword !== newPasswordRepeat;
  }),

  newPasswordComplexityTooLow: computed('newPassword', function() {
    let newPassword = this.get('newPassword');

    if(newPassword.length === 0) {
      return false;
    }

    if(newPassword.length < 6) {
      return true;
    }

    if(newPassword.length < 6) {
      return true;
    }

    let uniqueChars = newPassword.split('').filter((item, i, ar) => { return ar.indexOf(item) === i; }).join('');
    if(uniqueChars.length < 3) {
      return true;
    }

    return false;
  })
});
