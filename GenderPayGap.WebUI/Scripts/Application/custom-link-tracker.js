/*
 * Original from https://github.com/alphagov/govuk_frontend_toolkit/blob/master/javascripts/govuk/analytics/download-link-tracker.js
 */
(function (global) {
  'use strict';

  var $ = global.jQuery;
  var GOVUK = global.GOVUK || {};

  GOVUK.analyticsPlugins = GOVUK.analyticsPlugins || {};
  GOVUK.analyticsPlugins.customLinkTracker = function (options) {
    options = options || {};
    var customLinkSelector = options.selector || "[rel*='track']"; // finds elements who's rel attr contains 'track'
    var customLinkCategory = options.category || "Link Clicked";

    if (customLinkSelector) {
      $('body').on('click', customLinkSelector, trackDownload)
    }

    function trackDownload (evt) {
      var $link = getLinkFromEvent(evt);
      var linkCategory = $link.data('track-category') || customLinkCategory;
      var href =  $link.attr('href');
      var evtOptions = {transport: 'beacon'};
      var linkText = $.trim($link.text());
      
      if (linkText) {
        evtOptions.label = linkText
      }
      
      GOVUK.analytics.trackEvent(linkCategory, href, evtOptions)
    }

    function getLinkFromEvent (evt) {
      var $target = $(evt.target);

      if (!$target.is('a')) {
        $target = $target.parents('a')
      }

      return $target
    }
  };

  global.GOVUK = GOVUK
})(window);
