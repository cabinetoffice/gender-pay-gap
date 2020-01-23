(function (global) {
  'use strict';

  var $ = global.jQuery;
  var GOVUK = global.GOVUK || {};

  GOVUK.analyticsPlugins = GOVUK.analyticsPlugins || {};
  GOVUK.analyticsPlugins.detailsTracker = function (options) {
    options = options || {};
      var detailsSelector = options.selector || "summary[aria-expanded]"; // find summary elements with aria-expanded
      var detailsCategory = options.category || "Details Clicked";

    if (detailsSelector) {
      $('body').on('click', detailsSelector, trackDetails)
    }

    function trackDetails (evt) {
      var $summary = getSummaryFromEvent(evt);
      var evtOptions = {transport: 'beacon'};
      var summaryText = $.trim($summary.text());

        if (summaryText) {
            evtOptions.label = $summary.data('track-label') || summaryText;
        }

        var $details = $summary.closest("details");
        var trackAction = $summary.data('track-action') || ($details.attr("open")==null ? "DetailsSummary Expand" : "DetailsSummary Collapse");
        var trackCategory = $summary.data('track-category') || detailsCategory;

        GOVUK.analytics.trackEvent(trackCategory, trackAction, evtOptions)
    }

    function getSummaryFromEvent (evt) {
      var $target = $(evt.target);

      if (!$target.is('summary')) {
          $target = $target.parents('summary')
      }

      return $target
    }
  };

  global.GOVUK = GOVUK
})(window);
