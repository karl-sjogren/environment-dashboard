import moment from 'moment';
import { helper } from '@ember/component/helper';

export default helper(function(value, hash) {
  let date = moment(value[0]);

  if(!date.isValid()) {
    return '';
  }

  if(hash.format) {
    return date.format(hash.format);
  }

  if(hash.utc) {
    return date.utc().format('YYYY-MM-DD HH:mm:ss');
  } else {
    return date.format('YYYY-MM-DD HH:mm:ss');
  }
});
