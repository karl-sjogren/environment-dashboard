import Component from '@ember/component';
import { computed } from '@ember/object';
import { inject } from '@ember/service';

export default Component.extend({
  session: inject(),
  classNames: ['latest-camera-image'],
  attributeBindings: ['src'],
  tagName: 'img',
  cameraId: '',

  src: computed(function() {
    let token = this.session.data.authenticated.token;
    let cameraId = this.cameraId;

    return `/admin/api/cameras/${cameraId}/latest-image?token=${encodeURIComponent(token)}`;
  })
});