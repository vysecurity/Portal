$(document).ready(function () {
    $(document).prop('title', 'Anadolu Hayat Emeklilik - Acenta Portali');
    $('#littlelogo').css('top', '10%');
    if ($(location).attr('href').toString().indexOf("Page/ExternalBuildPage") != -1) {
        $("#littlelogo").attr("src", "/Content/Images/AHE/AHE_Logo-s.png")
        $("#littlelogo").removeClass("logo-default");

    }
    else {
        $("#littlelogo").attr("src", "/Content/Images/AHE/AHE_Logo-s.png")
        $("#littlelogo").removeClass("logo-default");
    }


    var call = "<li class='dropdown dropdown-extended dropdown-dark dropdown-notification' >"
    call += "<a href='tel:+905353030529' class='dropdown-toggle'>"
    call += "<i class='fa fa-phone'></i>"
    call += "</a></li>";
    $(call).insertBefore($(".droddown.dropdown-separator"))
});