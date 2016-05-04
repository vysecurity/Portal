$(document).ready(function () {

    $(".exporter").css("display", "none");
    if ($(location).attr('href').toString().indexOf("Page/ExternalBuildPage") != -1) {
        $("#littlelogo").attr("src", "/Content/Images/Mimaki_Logo_in.png")
        $("#littlelogo").removeClass("logo-default");
        
    }
    else {
        $("#littlelogo").attr("src", "/Content/Images/Mimaki_Logo_in.png")
    }

});