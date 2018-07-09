import Component from '@ember/component';
import { computed } from '@ember/object';
import { inject } from '@ember/service';

export default Component.extend({
  session: inject(),
  classNames: ['image-stream'],
  attributeBindings: ['src'],
  tagName: 'img',

  src: computed(function() {
    let token = this.session.data.authenticated.token;

    return '/admin/api/images/image-stream?token=' + encodeURIComponent(token);
  })
});