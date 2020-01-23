// SuggestEmployer
(function (GOVUK, $) {
    'use strict';

    function SuggestEmployer(options) {
        this.formId = '#' + options.formId;
        this.keywordsId = '#' + options.keywordsId;
        this.employerType = options.employerType;
        this.onEmployerTypeChanged = options.onEmployerTypeChanged;
        this.searchTypeActions = [
            null,
            {
                /*
                 * EMPLOYER NAME SUGGESTIONS
                 */

                // relative url to retrieve employer name suggestions from server
                suggestUrl: '/viewing/suggest-employer-name-js?search=',

                // template used for drop down employer name entry
                suggestionTemplate: function (suggestion) {
                    // direct link to employer page
                    var url = '/employer/' + suggestion.Id;

                    // build the html for drop down entry
                    var text = ['<a href="' + url + '">' + suggestion.Text];
                    if (suggestion.PreviousName.length > 0) {
                        text.push([
                            '<div class="suggest-prev-entry">',
                            '<span class="suggest-prevmatching">previously </span>',
                            '<span class= "suggest-prevname">' + suggestion.PreviousName + '</span>',
                            '</div>'
                        ].join(''));
                    }
                    text.push('</a>');
                    return '<div>' + text.join('') + '</div>';
                }
            }, {
                /*
                 * EMPLOYER TYPE AND SIC CODE SUGGESTIONS
                 */

                // relative url to retrieve employer type and sic code suggestions from server
                suggestUrl: '/viewing/suggest-sic-code-js?search=',

                // template used for drop down employer type and sic code entry
                suggestionTemplate: function (suggestion) {
                    // link to search by sic codes
                    var url = '/search-results?t=2&search=' + suggestion.SicCodeDescription;
                    var text = ['<a href="' + url + '">' + suggestion.SicCodeDescription + ' (SIC CODE ' + suggestion.SicCodeId + ')'];
                    if (suggestion.SicCodeMatchingSynonyms.length > 0) {
                        text.push([
                            '<div class="suggest-prev-entry">',
                            '<span class="suggest-prevmatching">-- </span>',
                            '<span class= "suggest-prevname">' + suggestion.SicCodeMatchingSynonyms + '</span>',
                            '</div>'
                        ].join(''));
                    }
                    text.push('</a>');
                    return '<div>' + text.join('') + '</div>';
                }
            }
        ];

        this.bindEvents();
    }

    SuggestEmployer.prototype = {

        bindEvents: function () {
            if (this.formId && this.formId.length > 0) {
                // bind search type
                var $form = $(this.formId);
                $form.on('change', 'input[name=t]', this.onSearchTypeChanged.bind(this));
            }

            if (this.employerType) {
                this.bindSuggestInput(this.searchTypeActions[this.employerType]);
            } else {
                // bind default keywords to suggest employer names
                this.bindSuggestInput(this.searchTypeActions[1]);
            }

            if (this.onEmployerTypeChanged) this.onEmployerTypeChanged(this.employerType);
        },

        onSearchTypeChanged: function (e) {
            var employerTypeIndex = e.currentTarget.value;
            var action = this.searchTypeActions[employerTypeIndex];
            this.bindSuggestInput(action);
            if (this.onEmployerTypeChanged) this.onEmployerTypeChanged(employerTypeIndex);
            if (e && e.preventDefault) e.preventDefault();
        },

        getEmployerNameSuggestionHandler: function (url, keywords, callback) {
            return $.get(encodeURI(url + keywords), function (response) {
                return callback(response.Matches);
            });
        },

        bindSuggestInput: function (action) {
            var self = this;

            // calls server to get matches
            var suggestionHandler = this.getEmployerNameSuggestionHandler;

            $(this.keywordsId).unbind();
            $(this.keywordsId).typeahead({
                hint: false,
                highlight: true,
                minLength: 2,
                classNames: {
                    menu: 'tt-menu form-control form-control-1',
                    highlight: 'bold-small'
                },
                ariaOwnsId: "arialist_" + Math.random()
            }, {
                    limit: 10,
                    display: 'Text',
                    source: function (query, syncResultsFn, asyncResultsFn) {
                        return suggestionHandler.call(self, action.suggestUrl, query, asyncResultsFn);
                    },
                    // template for each suggestion in the autocomplete list
                    templates: {
                        suggestion: action.suggestionTemplate
                    }
                });
        },

    };

    GOVUK.SuggestEmployer = SuggestEmployer;

}(GOVUK, jQuery));
