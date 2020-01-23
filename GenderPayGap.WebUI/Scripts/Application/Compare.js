(function () {
    
    window.GOVUK = window.GOVUK || {};

    var posHeader = [];
    var tableOffset, tableHeight, posHeaderLen, flag, pos;

    function AppendHead() {
        bindScrollEvents();

        $("table").each(function (index, element) {

            head = "#" + element.id + " thead";
            tableHeight = $(element).height();
            tableOffset = $(element).offset();

            posHeader[3 * index] = tableOffset.top;
            posHeader[3 * index + 1] = element.id;
            posHeader[3 * index + 2] = tableHeight + posHeader[3 * index];

            /* Add a class to the table to identify the processed table */
            if ($(element).hasClass("table-fixed")) {
                return;
            } else {
                $("#" + element.id).addClass("table-fixed");
            }

            var headerCopy = $(".header-copy");
            $("#" + element.id + " thead").clone().addClass('header-copy header-fixed no-print').stop()
                .appendTo("#" + element.id);

            var attributes = $("#" + element.id + " thead").prop("attributes");

            $.each(attributes,
                function () {
                    if (this.name == "class") return;
                    headerCopy.attr(this.name, this.value);
                });
            var style = [];
            $(element).find('thead > tr:first > th').each(function (i, h) {
                return style.push($(h).width());
            });
            $.each(style,
                function (i, w) {
                    return $(element)
                        .find('thead > tr > th:eq(' + i + '), thead.header-copy > tr > th:eq(' + i + ')').css({
                            width: w
                        });
                });
            $(element).find('thead.header-copy').css({
                margin: '0 auto',
                width: $(element).width(),
                top: tableOffset
            });

            posHeaderLen = parseInt(posHeader.length / 3);

        });
    }

    function bindScrollEvents() {

        $(window).scroll(function () {

            var scrollAmount = $(window).scrollTop();
            for (var j = 0; j <= posHeaderLen; j++) {
                var pos = j * 3;
                if (posHeader[pos] < scrollAmount) {
                    // inside the table
                    flag = true;

                    if (posHeader[2 + pos] > scrollAmount) {
                        // still inside the table
                        $("#" + posHeader[1 + pos]).addClass("visible");

                    } else {
                        // reached the end of the table
                        flag = false;
                        $("#" + posHeader[1 + pos]).removeClass("visible");
                    }
                } else {

                    flag = false;

                    $("#" + posHeader[1 + pos]).removeClass("visible");

                    $(".header-copy").css('left', tableOffset.left);
                }
            }

        });

        $(".overflowx").scroll(function () {
            $(".header-copy").css('left', ((-1) * $(".overflowx").scrollLeft()) + tableOffset.left);
        });

        $(window).resize(function () {

            for (var k = 0; k < posHeaderLen; k++) {
                pos = k * 3;
                tableId = "#" + posHeader[1 + pos];

                var headerCopy = $(tableId + " .header-copy");
                var attributes = $(tableId + " thead").prop("attributes");

                $.each(attributes,
                    function () {
                        if (this.name == "class") return;
                        headerCopy.attr(this.name, this.value);
                    });
                var style = [];
                $(tableId).find('thead > tr:first > th').each(function (i, h) {
                    return style.push($(h).width());
                });
                $.each(style,
                    function (i, w) {
                        return $(tableId)
                            .find('thead > tr > th:eq(' + i + '), thead.header-copy > tr > th:eq(' + i + ')').css({
                                width: w
                            });
                    });
                $(tableId).find('thead.header-copy').css({
                    margin: '0 auto',
                    width: $(tableId).width(),
                    top: tableOffset
                });
            }

            $(".header-copy").css('left', tableOffset.left);

        });

    }

    GOVUK.AppendHead = AppendHead;

})(jQuery);