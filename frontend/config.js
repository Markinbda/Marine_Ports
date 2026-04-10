/**
 * API_BASE — single place to change the backend URL.
 *
 * LOCAL DEV  : open the HTML files directly in a browser (file://) or via
 *              a local server — API runs on http://localhost:5000
 *
 * PRODUCTION : set window.__API_BASE before this file loads, OR update the
 *              PROD value below to your Railway URL after deploying the API.
 *
 * Railway URL format: https://<project-name>.up.railway.app
 */

(function () {
  const PROD = 'https://marineports-api.up.railway.app'; // ← update after Railway deploy
  const DEV  = 'http://localhost:5000';

  const isLocal = ['localhost', '127.0.0.1', ''].includes(window.location.hostname);
  window.API_BASE = isLocal ? DEV : PROD;
})();
