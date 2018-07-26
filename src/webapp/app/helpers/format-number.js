import { helper } from '@ember/component/helper';

export default helper(function(value, hash) {
  var number = parseFloat(value[0]);

  if(isNaN(number)) {
    return 0;
  }

  if(!hash.decimals) {
    return number.toFixed(0);
  }

  return number.toFixed(hash.decimals);
});
