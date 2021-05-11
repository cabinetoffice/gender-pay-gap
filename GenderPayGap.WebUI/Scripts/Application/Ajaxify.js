/* 
  Binds clickable buttons to Ajax call to Action Url which returns multiple HTML elements
  which are then replaced on the page

Javascript parameters:
       options.selectors: This parameter is an array of HTML selectors which specifies the trigger buttons
                Note: If empty binds to all elements with a 'data-js-url' attribute.
       options.onRefresh: This parameter is a function which is executed after a succcessful HTML replacement.
    Example Initialisation:
       new GOVUK.Ajaxify({
               onRefresh: function () {
                   GOVUK.stickAtTopWhenScrolling.init();
                   DfE.Util.Analytics.TrackEvent('comparison-basket', null, 'add');
                }
            });
       options.OnError: This parameter is a function which is executed whenever there is an Ajax error.

HTML arguments:
       'data-js-url' attribute: This attribute of the button specifies the Url of the Action for the Ajax call.
       'data-js-targets' attribute: This atteribute is a semicolon delimited list of HTML id selectors for the elements which are to be replaced.
       Note: If the html returned from Ajax also contains an element of the same id as the button then the button HTML itself is also automatically replaced.
       For example: 
        <a class="button button-comparison-list-add" id="AddRemoveEmployer1234" href="/search-results" data-js-url="/Viewing/AddEmployerJS/1234" data-js-targets="#comparison-basket">

Errors:
      
 */
(function () {
    'use strict';

    window.GOVUK = window.GOVUK || {};

    function Ajaxify(options) {

   
        //dont Ajaxify mobile safari 
        if (isAppleMobileSafari()) return;

        var selectors = options.selectors;
        this.onRefresh = options.onRefresh;
        this.onError = options.onError;

        if (selectors == null || selectors == 'undefined') selectors = ["[data-js-url]"];
        this.bindEvents(selectors, this);
    }

    Ajaxify.prototype.bindEvents = function bindEvents(selectors, root) {
        for (var i = 0; i < selectors.length; i++) {
            var selector = selectors[i];
            $(document).on('click', selector, this.callAjaxClickHandler.bind(root));
        }
    };

    Ajaxify.prototype.callAjaxClickHandler = function (e) {
        //Disable the default click event
        e.preventDefault();
        //Disable to prevent double click
        $(e).prop('disabled', true);
        var sourceSelector = '#' + escape(e.currentTarget.id);
        //Get the url to call
        var partialUrl = $(e.target).attr("data-js-url");

        //Get the list of selectors to replace
        var targetSelectors = $(e.target).attr("data-js-targets").split(';');

        var onRefresh = this.onRefresh;
        var onError = this.onError;

        //Disable all ajaxified buttons
        $("[data-js-url]").attr("disabled", "disabled");

        $.ajax({
            cache: false,
            url: partialUrl
        }).done(function (htmlContent) {

            var div = document.createElement("div");
            div.innerHTML = htmlContent;
            var $htmlContent = $(div);

            //Replace the source html
            var $partialContent = sourceSelector == '#' ? null : $htmlContent.find(sourceSelector);

            //Replace the target html
            var $target = sourceSelector == '#' ? null : $(document).find(sourceSelector);
            if ($partialContent != null && $partialContent.length > 0 && $target.length > 0) $target[0].outerHTML = $partialContent[0].outerHTML;

            for (var i = 0; i < targetSelectors.length; i++) {
                var targetSelector = targetSelectors[i];

                //Get the new html
                $partialContent = $htmlContent.find(targetSelector);

                for (var elementIndex = 0; elementIndex < $partialContent.length; elementIndex++) {
                    var $elementContent = $($partialContent[elementIndex]);
                    if ($elementContent.length > 0) {
                        targetSelector = '#' + $elementContent.attr('id');
                        if (targetSelector != '#') {
                            //Replace the target html
                            $target = $(document).find(targetSelector);
                            if ($elementContent.length > 0 && $target.length > 0) $target[0].outerHTML = $elementContent[0].outerHTML;
                        }
                    }
                }
            }

            //Call any onrefresh functions
            if (onRefresh) {
                onRefresh();
            }

            //Set focus back to original clicked element
            $(sourceSelector).focus();

        }).error(function (requestObject, error, errorThrown) {
            //Call any onError functions
            if (onError) {
                onError(requestObject, error, errorThrown);
            }
            else {
                window.location = '/error';
            }
        }).complete(function () {
            //Ensable all other ajaxified buttons
            $("[data-js-url]").removeAttr("disabled");
        });

        return false;
    };

    GOVUK.Ajaxify = Ajaxify;

}(jQuery));
