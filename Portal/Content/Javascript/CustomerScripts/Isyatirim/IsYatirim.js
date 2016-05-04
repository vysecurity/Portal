var generalFlag = ""

$(document).ready(function () {
    $(document).prop('title', 'Iş Yatırım Kariyer Portali');
    if ($(location).attr('href').toString().indexOf("/E/") != -1) {
        $("#littlelogo").attr("src", "/Content/Images/IsYatirim_Logo_s.png")
        $("#littlelogo").removeClass("logo-default");
        $("#submitbutton").removeClass("uppercase");
        $("#forget-password").removeClass("uppercase");
    }
    else {
        $("#littlelogo").attr("src", "/Content/Images/IsYatirim_Logo_s.png")
    }
   
});