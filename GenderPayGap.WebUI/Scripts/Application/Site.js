function windowClose() {
    window.open('', '_self').close();
    window.close();
}

function isAppleMobileSafari() {
    var isAppleMobile = navigator.userAgent.indexOf("iPad") != -1 || navigator.userAgent.indexOf("iPhone") != -1 || navigator.userAgent.indexOf("iPod") != -1;
    var isMobileSafari = navigator.userAgent.indexOf("Mobile") != -1 && navigator.userAgent.indexOf("Safari") != -1;
    return isAppleMobile && isMobileSafari;
}

//Include document initiation below:
$(document).ready(function () {
    $(".script-only").show();
});


