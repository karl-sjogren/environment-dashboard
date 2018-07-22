import { helper } from '@ember/component/helper';

export default helper(function(value) {
  if(value.length !== 2) {
    console.warn('Invalid number or arguments passed to eq helper. Defaulting to false.');
    return false;
  }
  
  return value[0] === value[1];
});
