//Disable form when submitted to prevent-double click
(function () {
    'use strict';

    $(document).ready(function () {

        $(document).on('submit', "form", function (e) {

            //Must use a timout here otherwise the 'value' field of submitted buttons is never submitted
            setTimeout(function () { $("[type=submit]").attr("disabled", true); }, 0);
        });

    });

}());
