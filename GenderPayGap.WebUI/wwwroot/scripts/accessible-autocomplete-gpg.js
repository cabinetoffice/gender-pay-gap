function generateOrganisationSearchbar (element, id) {
    accessibleAutocomplete({
        element: document.querySelector(element),
        id: id,  // To match it to the existing <label>.
        source: organisationList,
        name: 'search',
        minLength: 2,
        displayMenu: 'overlay',
        templates: {
            inputValue: organisationInputTemplate,
            suggestion: organisationSuggestionTemplate
        }
    });

    function organisationList (query, syncResults) {
        var organisationListUrl = '/viewing/suggest-employer-name-js?search=';
        return getSuggestions(organisationListUrl, query, syncResults);
    }

    function getSuggestions(url, keywords, callback) {
        return $.get(encodeURI(url + keywords), function (response) {
            return callback(response.Matches);
        });
    }

    function organisationInputTemplate (suggestion) {
        return suggestion && suggestion.Text;
    }

    function organisationSuggestionTemplate (suggestion) {
        var suggestionElement = document.createElement("span");
        suggestionElement.textContent = suggestion.Text;

        if (suggestion.PreviousName.length > 0) {
            var previousNameElement = document.createElement("div");
            previousNameElement.textContent = "previously ";
            previousNameElement.classList.add("previous-name-marker", "govuk-body-s");

            var previousNameChildElement = document.createElement("span");
            previousNameChildElement.textContent = suggestion.PreviousName;
            previousNameChildElement.classList.add("previous-name");

            previousNameElement.appendChild(previousNameChildElement);
            suggestionElement.appendChild(previousNameElement)

        }
        return suggestionElement.innerHTML;
    }
}



