/* eslint-env browser */
/* global document, localStorage */
/**
 * theme-switch.js – Marine & Ports theme toggle
 * Runs synchronously on load to avoid flash-of-wrong-theme.
 * Injects toggle button into whatever header is present on each page.
 * Theme choice persists in localStorage so it applies site-wide.
 */
(function () {
  'use strict';

  var STORAGE_KEY = 'mp_theme';
  var DEFAULT     = 'legacy';

  function getTheme()   { return localStorage.getItem(STORAGE_KEY) || DEFAULT; }
  function saveTheme(t) { localStorage.setItem(STORAGE_KEY, t); }

  function applyTheme(t) {
    document.documentElement.setAttribute('data-theme', t);
    var btn = document.getElementById('mp-theme-btn');
    if (!btn) return;
    var isNew = (t === 'new');
    btn.textContent = isNew ? '\u2190 Legacy Theme' : '\u{1F3DB} Gov Theme';
    btn.title       = isNew ? 'Switch to legacy theme' : 'Switch to government theme';
    btn.classList.toggle('mp-btn-active', isNew);
  }

  /* Apply immediately to prevent flash-of-wrong-theme */
  document.documentElement.setAttribute('data-theme', getTheme());

  function injectToggle() {
    var btn  = document.createElement('button');
    btn.id   = 'mp-theme-btn';
    btn.type = 'button';
    btn.className = 'mp-theme-btn';

    btn.addEventListener('click', function () {
      var next = (getTheme() === 'new') ? 'legacy' : 'new';
      saveTheme(next);
      applyTheme(next);
    });

    /* Try each page's header button-group in priority order */
    var host =
      document.querySelector('.header-right')   ||  /* admin.html   */
      document.querySelector('.header-actions') ||  /* index.html   */
      document.querySelector('header .actions') ||  /* profile.html */
      null;

    /* map.html – inject into header's .header-actions span */
    if (!host) {
      var ha = document.querySelector('header .header-actions');
      if (ha) {
        host = ha;
      } else {
        var hdr = document.querySelector('header');
        if (hdr) { host = hdr; }
      }
    }

    if (host) {
      /* Prepend so it sits at the left of the button group */
      host.insertBefore(btn, host.firstChild);
    } else {
      /* login.html / register.html – float in bottom-right corner */
      btn.style.cssText =
        'position:fixed;bottom:1.2rem;right:1.2rem;z-index:9998;';
      document.body.appendChild(btn);
    }

    applyTheme(getTheme());
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', injectToggle);
  } else {
    injectToggle();
  }
}());
