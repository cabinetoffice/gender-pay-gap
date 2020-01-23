(function ($) {
    'use strict';

    window.GOVUK = window.GOVUK || {};

    function AddRemoveComparsion(options) {
        this.bindEvents(options);
    }

    AddRemoveComparsion.prototype = {
        bindEvents: function (options) {
            $(document).on('click', options.addToTableButtonSelector, this.addToCompareClickHandler.bind(this));
            $(document).on('click', options.removeFromTableButtonSelector, this.removeFromCompareClickHandler.bind(this));
            $(document).on('click', options.removeFromBasketButtonSelector, this.removeFromComparisonBasketTableClickHandler.bind(this));

            window.addEventListener("load", function () {
                var basketCount = this.getBasketCount();
                $(".num-items-in-compare").html(basketCount);
                if (basketCount > 0) {
                    this.checkCellHeightOnExistingBasketRows();
                }
            }.bind(this));
        },
        getBasket: function () {
            var emptyBasket = {
                primary: 0,
                secondary: 0,
                ks5: 0
            };
            if (!GOVUK || !GOVUK.getCookie) return emptyBasket;
            var data = GOVUK.getCookie("comparisonItemIds");
            if (data && typeof (data) === "string") {
                var phaseIds = data.split(';');
                if (phaseIds.length !== 3) {
                    return emptyBasket;
                } else {
                    return {
                        primary: phaseIds[0] == '' ? 0 : phaseIds[0].split(',').length,
                        secondary: phaseIds[1] == '' ? 0 : phaseIds[1].split(',').length,
                        ks5: phaseIds[2] == '' ? 0 : phaseIds[2].split(',').length
                    }
                }

            }
            else {
                return emptyBasket;
            }
        },
        getBasketCount: function () {
            if (!GOVUK || !GOVUK.getCookie) return 0;
            var data = GOVUK.getCookie("comparisonItemIds");
            if (data && typeof (data) === "string") {
                while (data.indexOf(";") > -1)
                    data = data.replace(";", ",");

                var items = data.split(",");
                var temp = [];
                for (var i = 0; i < items.length; i++) {
                    var found = false;
                    for (var j = 0; j < temp.length; j++) {
                        if (items[i] === temp[j]) {
                            found = true;
                            break;
                        }
                    }
                    if (!found && items[i] && items[i] !== "") temp.push(items[i]);
                }
                return temp.length;
            }
            else return 0;
        },
        addToCompareClickHandler: function (e) {
            e.preventDefault();
            var elementIdSelector = '#' + e.currentTarget.id.replace('Add', 'Remove');
            var partialUrl = $(e.target).attr("data-js-url");
            var parentElemHeight = $(e.target).parents('th').height();
            var schoolNameTextHeight = $(e.target).parent().siblings('.result-school-link').outerHeight();
            
            $.ajax({
                url: partialUrl
            }).done(function (htmlContent) {
                if (parentElemHeight < schoolNameTextHeight + 15) {
                    $(e.target).parents('th').height(parentElemHeight + 15);
                }
                $(e.target).closest("div.comparsion-button-container").html(htmlContent);
                new GOVUK.ViewComparisonViewModel().incrementItemsCount();
                $(elementIdSelector).focus();
            }).error(function () {
                $(e.target).closest("div.add-remove-error").show();
            });

            DfE.Util.Analytics.TrackEvent('comparison-basket', null, 'add');
        },
        countConfig: function(number, isKs5){
            if(isKs5 && isKs5 === true) {
                return number === 1 ? "(1 school or college)" : "(" + number + " schools or colleges)";
            } else {
                return number === 1 ? "(1 school)" : "(" + number + " schools)";
            }
        },
        removeFromCompareClickHandler: function (e) {
            e.preventDefault();
            var elementIdSelector = '#' + e.currentTarget.id.replace('Remove', 'Add');
            var partialUrl = $(e.target).attr("data-js-url");
            $(e.target).parents('th').removeAttr('style');
            $.ajax({
                url: partialUrl
            }).done(function (htmlContent) {
                $(e.target).closest("div.comparsion-button-container").html(htmlContent);
                new GOVUK.ViewComparisonViewModel().decrementItemsCount();

                $(elementIdSelector).focus();
            }).error(function () {
                $(e.target).closest("div.add-remove-error").show();
            });

            DfE.Util.Analytics.TrackEvent('comparison-basket', null, 'remove');
        },
        removeFromComparisonBasketTableClickHandler: function (e) {            
            if ($('tr[data-row-id="SchoolsResultsRow"]').length > 0) {
                e.preventDefault();
                var urn = $(e.target).closest("th").attr("data-estab-urn");
                var partialUrl = $(e.target).attr("data-js-url") + e.target.search;
                var primaryCount = $('span.primary-count');
                var secondaryCount = $('span.secondary-count');
                var ks5Count = $('span.16to18-count');
                var that = this;
                var saveCTLink = $('.js-modal-linkurl');
                var emailCTLink = $('a.email');
                $.ajax({
                    url: partialUrl
                }).done(function () {
                    $(e.target).closest("tr").remove();
                    new GOVUK.ViewComparisonViewModel().decrementItemsCount();
                    if (primaryCount && secondaryCount && ks5Count) {
                        var basket = that.getBasket();
                        if (basket) {
                            primaryCount.html(that.countConfig(basket.primary));
                            secondaryCount.html(that.countConfig(basket.secondary));
                            ks5Count.html(that.countConfig(basket.ks5, true));
                        }
                    }

                    if (that.getBasketCount() < 2) {
                        $('.save-ct-list').css("display", "none");
                    }
                    else {

                        var copyUrns = saveCTLink.val();
                        copyUrns = copyUrns.replace(urn, "").replace("--", "-");
                        saveCTLink.attr("value", copyUrns);
                        emailCTLink.attr("href", "mailto: ?body=" + copyUrns);

                        var $saveList = $('div.save-list-modal');
                        if (copyUrns.length < 2000 && !$saveList.hasClass("active")) {
                            $('div.modal-view').removeClass("active");
                            $saveList.addClass("active");
                        }
                    }

                }).error(function () {
                    $(e.target).closest("div.add-remove-error").show();
                });

                DfE.Util.Analytics.TrackEvent('comparison-basket', null, 'remove');
            }
        },
        checkCellHeightOnExistingBasketRows: function() {
            $('#SortableTable').find('.row-item-in-basket').each(function(n, rowHeader) {
                var schoolNameHeight = $(rowHeader).find('.result-school-link').outerHeight();
                var cellHeight = $(rowHeader).height();
                if (cellHeight < schoolNameHeight + 15) {
                    $(rowHeader).height(cellHeight + 15);
                }
            });
        }
    };

    GOVUK.AddRemoveComparsion = AddRemoveComparsion;

}(jQuery));
