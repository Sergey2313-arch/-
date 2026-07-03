(() => {
  window.addEventListener('error', (event) => console.warn('ReviMarket runtime warning:', event.message));
  document.addEventListener('click', (event) => {
    const nav = document.querySelector('#nav');
    const menu = document.querySelector('#menuBtn');
    if (!nav || !menu || !nav.classList.contains('open')) return;
    if (nav.contains(event.target) || menu.contains(event.target)) return;
    nav.classList.remove('open');
  });
})();
