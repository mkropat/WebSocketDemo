'use strict';

window.WebSocketDemo = window.WebSocketDemo || {};

(() => {
  window.WebSocketDemo.wrapSelectors = (selectors) => (
    Object.assign(...Object.keys(selectors).map(key => ({
      [key]: new ElementWrapper(selectors[key]),
    })))
  );

  class ElementWrapper {
    constructor(selector) {
      this._selector = selector;
    }

    update(updateFunc) {
      document.querySelectorAll(this._selector).forEach(updateFunc);
    }

    get value() {
      const { value } = document.querySelector(this._selector) || {};
      return value;
    }
  }
})();