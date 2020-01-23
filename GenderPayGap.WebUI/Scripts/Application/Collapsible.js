//Collapsible
(function ($) {
    "use strict";
    window.GOVUK = window.GOVUK || {};

    function Collapsible(options) {
        if (!options.elementId || options.elementId == "")
            throw new ReferenceError("options.elementId is not present.");

        this.collapseId = options.elementId;
        if (!this.getCollapsibleElement())
            throw new ReferenceError("collapsibleElement did not resolve using the id: " + options.elementId);

        if (options.useAccessibleButton == true)
            // Replace div.container-head with a button
            this.replaceHeadWithButton();

        // attached the aria controls
        this.bindAriaControls();

        if (!options.ignoreEvents)
            this.bindEvents();

        // update the current state
        this.setExpanded(!this.isClosed());
    }

    Collapsible.prototype = {

        getCollapsibleElement: function () {
            return document.getElementById(this.collapseId);
        },

        replaceHeadWithButton: function () {
            /* Replace the div at the head with a button element. This is based on feedback from Leonie Watson.
             * The button has all of the accessibility hooks that are used by screen readers and etc.
             * We do this in the JavaScript because if the JavaScript is not active then the button shouldn't
             * be there as there is no JS to handle the click event.
             */
            var collapsibleElement = this.getCollapsibleElement();
            var jsCtrlElementHTML = collapsibleElement.innerHTML;
            var button = this.getCollapsibleElement();

            if (collapsibleElement.tagName != "BUTTON") {
                // Create button and replace the preexisting html with the button.
                button = document.createElement("button");

                // clone the attributes
                Array.prototype.forEach.call(collapsibleElement.attributes, function (attr) {
                    var namedItem = document.createAttribute(attr.name);
                    namedItem.value = attr.value;
                    button.attributes.setNamedItem(namedItem)
                });

                button.id = this.collapseId;
                button.className = collapsibleElement.className;
                button.innerHTML = jsCtrlElementHTML;
                collapsibleElement.parentNode.replaceChild(button, collapsibleElement);
            }
        },

        bindAriaControls: function () {
            var controlsId = this.getCollapsibleElement().getAttribute("aria-controls");
            if (!controlsId || controlsId == "")
                return;
            this.ariaControlsElement = document.getElementById(controlsId);
        },

        hasAriaControls: function () {
            return this.ariaControlsElement != undefined && this.ariaControlsElement != null;
        },

        bindEvents: function () {
            // bind click event listener
            this.getCollapsibleElement().addEventListener("click", this.expanderClickHandler.bind(this));
            $("input[type=radio]", this.getCollapsibleElement()).click(this.expanderClickHandler.bind(this));
            $("span", this.getCollapsibleElement()).click(this.expanderClickHandler.bind(this));
            this.listenForKeys();
        },

        isClosed: function () {
            return $(this.getCollapsibleElement()).hasClass("selected") != true;
        },

        expanderClickHandler: function (e) {
            e.preventDefault();
            if (this.isClosed()) {
                this.open();
            } else {
                this.close();
            }
        },

        open: function () {
            if (this.isClosed())
                this.setExpanded("true");
        },

        close: function () {
            this.setExpanded("false");
        },

        setExpanded: function (flag) {
            if (this.hasAriaControls() == true) {
                var $ariaControlsElement = $(this.ariaControlsElement);
                if (flag == "true" || flag === true) {
                    $ariaControlsElement.addClass("js-expanded");
                    $ariaControlsElement.removeClass("js-collapsed");
                    $ariaControlsElement.attr("aria-hidden", "false");
                    $ariaControlsElement.attr("aria-expanded", "true");
                }
                else {
                    $ariaControlsElement.removeClass("js-expanded");
                    $ariaControlsElement.addClass("js-collapsed");
                    $ariaControlsElement.attr("aria-hidden", "true");
                    $ariaControlsElement.attr("aria-expanded", "false");
                }
            }
            var $collapsibleEl = $(this.getCollapsibleElement());

            if (flag == "true" || flag === true) {
                $collapsibleEl.addClass("selected");
            } else {
                $collapsibleEl.removeClass("selected");
            }
            var $childSelectableInputs = $collapsibleEl.find("input[type='radio'], input[type='checkbox']");
            if ($childSelectableInputs.length > 0) {
                $childSelectableInputs.attr("checked", flag);
            }
        },

        getTarget: function () {
            return document.getElementById(this.controlsId);
        },

        listenForKeys: function () {
            this.specialKeyHandler = this.checkForSpecialKeys.bind(this);
            this.getCollapsibleElement().addEventListener("keypress", this.specialKeyHandler);
        },

        checkForSpecialKeys: function (e) {
            if (e.keyCode == 13) {
                // keyCode 13 is the return key.
                this.expanderClickHandler(e);
            }
        },

        stopListeningForKeys: function () {
            this.getCollapsibleElement().removeEventListener("keypress", this.specialKeyHandler);
        }
    };

    // Instantiate an option select for each one found on the page
    Collapsible.bindElements = function (selector) {
        var collapsibleElements = $(selector);
        Array.prototype.forEach.call(collapsibleElements, function (element) {
            new Collapsible({ elementId: element.id, useAccessibleButton: true });
        });
    };

    GOVUK.Collapsible = Collapsible;

}(jQuery));
