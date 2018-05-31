'use strict';

window.WebSocketDemo = window.WebSocketDemo || {};

window.WebSocketDemo.initApp = (selectors) => {
  let elements = window.WebSocketDemo.wrapSelectors(selectors);;
  let store = new window.WebSocketDemo.Store();
  let websocket;

  const initialize = () => {
    store.onUpdate(render);

    store.update({
      isLoading: false,
      pollJobHistory: [],
      pollResult: null,
      pushJobHistory: [],
      pushResult: null,
    });

    elements.hashButton.update(b => b.addEventListener('click', handleClick));

    let antiCswshToken = parseCookie(document.cookie).antiCswshToken || '';
    websocket = new WebSocket(getWebsocketUrl('/websocket', `?antiCswshToken=${encodeURIComponent(antiCswshToken)}`));
    websocket.addEventListener('open', console.log('websocket opened'));
    websocket.addEventListener('error', console.error.bind(console, 'WebSocket error'));
  };

  const render = () => {
    const { isLoading, pollJobHistory, pollResult, pushJobHistory, pushResult } = store.state;

    elements.hashButton.update(button => {
      button.disabled = isLoading ? 'disabled' : null;
    });

    elements.pollJobHistory.update(container => {
      container.innerText = pollJobHistory.join(', ');
    });

    elements.pollResult.update(container => {
      container.style.display = pollResult ? '' : 'none';

      if (pollResult) {
        Object.keys(pollResult).forEach(key => {
          container.querySelectorAll('.' + key).forEach(element => {
            element.innerText = pollResult[key];
          });
        });
      }
    });

    elements.pushJobHistory.update(container => {
      container.innerText = pushJobHistory.join(', ');
    });

    elements.pushResult.update(container => {
      container.style.display = pushResult ? '' : 'none';

      if (pushResult) {
        Object.keys(pushResult).forEach(key => {
          container.querySelectorAll('.' + key).forEach(element => {
            element.innerText = pushResult[key];
          });
        });
      }
    });
  };

  const handleClick = () => {
    store.update({
      isLoading: true,
      pollJobHistory: [],
      pollResult: null,
      pushJobHistory: [],
      pushResult: null,
    });

    let hashResult = fetch('/api/hash', { credentials: 'include', method: 'POST', body: elements.dataInput.value })
      .then(response => response.json());

    let polling = hashResult
      .then(pollUntilComplete)
      .then(result => store.update({ pollResult: result }));

    let push = hashResult
      .then(originalJob => new Promise(resolve => {
        let eventHandler = handlePushUpdates(originalJob.id, result => resolve({ eventHandler, result }));
        websocket.addEventListener('message', eventHandler);
      }))
      .then(({ eventHandler, result }) => {
        websocket.removeEventListener('message', eventHandler);
        store.update({ pushResult: result });
      });

    Promise.all([polling, push])
      .finally(() => store.update({ isLoading: false }));
  };

  const pollUntilComplete = (job) => {
    store.update({ pollJobHistory: [...store.state.pollJobHistory, job.status] });

    if (job.status !== 'complete') {
      let selfLink = job._links.find(x => x.rel === 'self');
      if (!selfLink) {
        throw new Error('Expected a rel=self link');
      }
      return delay(1000)
        .then(() => fetch(selfLink.href, { credentials: 'include' }))
        .then(r => r.json())
        .then(pollUntilComplete);
    }

    return Promise.resolve(job.result);
  };

  const handlePushUpdates = (jobId, onCompletion) => message => {
    let job = JSON.parse(message.data);

    if (job.id !== jobId)
      return;

    store.update({ pushJobHistory: [...store.state.pushJobHistory, job.status] });

    if (job.status === 'complete') {
      onCompletion(job.result);
    }
  };

  const delay = ms => new Promise(r => setTimeout(r, ms));

  const getWebsocketUrl = (pathname='', search='') => {
    let url = new URL(window.location.href);
    url.protocol = (window.location.protocol === 'http:') ? 'ws:' : 'wss:';
    url.pathname = pathname;
    url.search = search;
    return url.toString();
  }

  const parseCookie = (cookie='') => {
    let result = {};
    cookie.split(/;\s*/).forEach(part => {
      let [key, ...rest] = part.split(/=/);
      let value = rest.join('=');
      result[decodeURIComponent(key)] = decodeURIComponent(value);
    });
    return result;
  };

  initialize();
};
