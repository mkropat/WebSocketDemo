'use strict';

window.WebSocketDemo = window.WebSocketDemo || {};

window.WebSocketDemo.initApp = (selectors) => {
  let elements = window.WebSocketDemo.wrapSelectors(selectors);;
  let store = new window.WebSocketDemo.Store();

  const initialize = () => {
    store.onUpdate(render);

    store.update({
      isLoading: false,
      jobHistory: [],
      result: null,
    });

    elements.hashButton.update(b => b.addEventListener('click', handleClick));
  };

  const render = () => {
    const { isLoading, jobHistory, result } = store.state;

    elements.hashButton.update(button => {
      button.disabled = isLoading ? 'disabled' : null;
    });

    elements.jobHistory.update(container => {
      container.innerText = jobHistory.join(', ');
    });

    elements.results.update(container => {
      container.style.display = result ? '' : 'none';

      if (result) {
        Object.keys(result).forEach(key => {
          container.querySelectorAll('.' + key).forEach(element => {
            element.innerText = result[key];
          });
        });
      }
    });
  };

  const handleClick = () => {
    store.update({ isLoading: true, jobHistory: [], result: null });

    fetch('/api/hash', { method: 'POST', body: elements.dataInput.value })
      .then(pollUntilComplete)
      .then(result => store.update({ result }))
      .finally(() => store.update({ isLoading: false }));
  }

  const pollUntilComplete = (jobResponse) => jobResponse.json()
    .then(job => {
      store.update({ jobHistory: [...store.state.jobHistory, job.status] });

      if (job.status !== 'complete') {
        let selfLink = job._links.find(x => x.rel === 'self');
        if (!selfLink) {
          throw new Error('Expected a rel=self link');
        }
        return delay(1000)
          .then(() => fetch(selfLink.href))
          .then(pollUntilComplete);
      }

      return job.result;
    });

  const delay = ms => new Promise(r => setTimeout(r, ms));

  initialize();
};
