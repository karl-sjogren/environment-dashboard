import Component from '@ember/component';
import { computed } from '@ember/object';
import { run } from '@ember/runloop';
import { inject } from '@ember/service';

export default Component.extend({
  session: inject(),
  classNames: ['latest-camera-image'],
  tagName: 'div',
  cameraId: '',
  showPlayButton: false,
  imageLoaded: false,
  imageFailed: false,
  playing: false,

  imageSrc: computed('camera', 'playing', function() {
    let token = this.session.data.authenticated.token;
    let cameraId = this.cameraId;

    if(this.playing)
      return `/admin/api/cameras/${cameraId}/image-stream?token=${encodeURIComponent(token)}`;

    return `/admin/api/cameras/${cameraId}/latest-image?token=${encodeURIComponent(token)}`;
  }),

  didInsertElement() {
    this._super(...arguments);

    let img = this.$('img');
    let loaded = false;
    img.one('load', () => {
      if(loaded) {
        return;
      }
      loaded = true;

      if(!!img[0].complete && !!img[0].naturalWidth) {
        run(() => {
          this.set('imageLoaded', true);
        });
      }
    });

    if(!!img[0].complete && !!img[0].naturalWidth) {
      if(loaded) {
        return;
      }
      loaded = true;

      run(() => {
        this.set('imageLoaded', true);
      });
    }

    img.on('error', () => {
      run(() => {
        this.set('imageFailed', true);
      });
    });
  },

  actions: {
    play() {
      this.set('playing', true);
    }
  }
});