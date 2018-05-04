'use strict';

window.WebSocketDemo = window.WebSocketDemo || {};

WebSocketDemo.Store = class Store {
  constructor() {
    this._callbacks = [];
    this._queued = false;
    this._state = {};
  }

  get state() {
    return this._state;
  }

  update(newState) {
    this._state = { ...this._state, ...newState };

    if (!this._queued) {
      this._queued = true;
      setTimeout(() => {
        this._queued = false;
        this._callbacks.forEach(cb => cb());
      });
    }
  }

  onUpdate(callback) {
    this._callbacks.push(callback);
    return () => this._callbacks = this._callbacks.filter(x => x !== callback);
  }
};
