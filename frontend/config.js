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
  const PROD = 'https://marine-ports.onrender.com'; // Render API
  const DEV  = 'http://localhost:5000';

  // Highest-priority override for deployments/custom hosting.
  if (typeof window.__API_BASE === 'string' && window.__API_BASE.trim()) {
    window.API_BASE = window.__API_BASE.trim();
    return;
  }

  const host = window.location.hostname;
  const protocol = window.location.protocol;
  const params = new URLSearchParams(window.location.search);
  const forceLocal = params.get('api') === 'local';

  if (forceLocal || host === 'localhost' || host === '127.0.0.1') {
    window.API_BASE = DEV;
    return;
  }

  // Opening pages directly from file:// should use the deployed API by default.
  if (protocol === 'file:') {
    window.API_BASE = PROD;
    return;
  }

  window.API_BASE = PROD;
})();
