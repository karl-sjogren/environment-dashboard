import { helper } from '@ember/component/helper';

export default helper(function(value) {
  let array = value[0];
  if(!Array.isArray(array)) {
    return [];
  }

  let length = value[1];

  if(!Number.isInteger(length)) {
    return [];
  }

  if(array.length <= length) {
    return array;
  }

  return array.slice(0, length);
});
