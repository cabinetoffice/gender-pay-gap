(function ($) {
    'use strict';

    window.GOVUK = window.GOVUK || {};
    var inCompare = window.location.toString().indexOf('compare-schools') > 0;
    function ViewComparisonViewModel() {
        this.update = function (num) {
            var compBasket = $('#comparison-basket');
            var compBasketLabel = $('.comparison-basket-label p');
            var compBasketButton = $('.comparison-basket-button');
            var compBasketButtonLink = compBasketButton.children('a');
            var numItems = parseInt(compBasket.attr('data-basket-count'));
            var addCompButton = $('.add-comparison-button');
            var removeCompButton = $('.remove-comparison-button');

            if (!isNaN(numItems) && !isNaN(num)) {
                numItems = numItems + num;
                compBasket.attr('data-basket-count', numItems);
                if (numItems === 0 && compBasket.hasClass('show-empty')) {
                    compBasketLabel.html('Add a school or college to start your comparison list');
                }
                else if (numItems === 0) {
                    compBasket.addClass('visuallyhidden');
                    compBasketLabel.html('Add a school or college to start your comparison list');
                }
                else if (numItems === 1) {
                    compBasket.removeClass('visuallyhidden');
                    compBasketButton.addClass('visuallyhidden');
                    compBasketButtonLink.attr("tabindex", "-1");
                    compBasketLabel.html('Your comparison list contains <strong>1 school or college</strong>');
                }
                else {
                    compBasketButton.removeClass('visuallyhidden');
                    compBasketButtonLink.removeAttr("tabindex");
                    compBasketLabel.html('Your comparison list contains <strong>' + numItems + ' schools or colleges</strong>');
                }

                if (num > 0 && addCompButton) {
                    addCompButton.addClass('visuallyhidden');
                    removeCompButton.removeClass('visuallyhidden');
                }
                else if (removeCompButton) {
                    addCompButton.removeClass('visuallyhidden');
                    removeCompButton.addClass('visuallyhidden');
                }

                this.updateLegacy(numItems);
            }
        };

        this.updateLegacy = function (num) {
            var viewCompButton = $('.button-view-comparison');
            var numItemsInCompare = $('span.num-items-in-compare');
            var currentTabCount = $('#SortableTable').find('[data-row-id="SchoolsResultsRow"]').length;
            var tabControls = $('.tabs').find('a');
            numItemsInCompare.html(num);

            if (num === 0 && inCompare) {
                var location = window.location.toString();
                var navigateTo = location.split('?')[0];
                return window.location = navigateTo;
            }

            if (inCompare && currentTabCount === 0 && num > 0) { // last estab on tab removed> switch to 1st tab with estabs
                window.setTimeout(function(){
                    tabControls.each(function(n, elem){
                        if (parseInt($(elem).find('.legend-message').text().replace(/\D/g,''), 10) > 0) {
                            $(elem).click();
                            return false;
                        }
                    });
                }, 100);

            }

            if (num < 1) {
                viewCompButton.addClass('zero');
                $('#comp-banner-clear-button').addClass('visually-hidden');
            } else {
                viewCompButton.removeClass('zero');
                $('#comp-banner-clear-button').removeClass('visually-hidden');
            }

        }
    }

    ViewComparisonViewModel.prototype = {
        decrementItemsCount: function () {
            this.update(-1);
        },
        incrementItemsCount: function () {
            this.update(1);
        }
    };

    ViewComparisonViewModel.Load = function () {
        return new GOVUK.ViewComparisonViewModel()
    };

    GOVUK.ViewComparisonViewModel = ViewComparisonViewModel;

}(jQuery));