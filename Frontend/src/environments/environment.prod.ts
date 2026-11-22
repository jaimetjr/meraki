export const environment = {
  production: true,
  maintenanceMode: false,
  /** The API serves this site, so it lives on the same address. */
  apiBaseUrl: "/api",
  /**
   * TEMPORARY: Cloudflare's public test site key (the widget always passes; pairs with the API's test
   * secret). Replace it with the real key from the Cloudflare dashboard before the site is announced.
   */
  turnstileSiteKey: "1x00000000000000000000AA",
};
