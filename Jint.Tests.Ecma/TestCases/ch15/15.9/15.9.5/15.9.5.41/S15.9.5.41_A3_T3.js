// Copyright 2009 the Sputnik authors.  All rights reserved.
/**
 * The Date.prototype.setUTCFullYear property "length" has { ReadOnly, DontDelete, DontEnum } attributes
 *
 * @path ch15/15.9/15.9.5/15.9.5.41/S15.9.5.41_A3_T3.js
 * @description Checking DontEnum attribute
 */

if (Date.prototype.setUTCFullYear.propertyIsEnumerable('length')) {
  $ERROR('#1: The Date.prototype.setUTCFullYear.length property has the attribute DontEnum');
}

for(x in Date.prototype.setUTCFullYear) {
  if(x === "length") {
    $ERROR('#2: The Date.prototype.setUTCFullYear.length has the attribute DontEnum');
  }
}


