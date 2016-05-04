var myvar = {}
$(document).ready(function () {
    MakeRequired();
    $(".submitter").on("click", function () {
        myvar = setInterval(function () { Redirect() }, 500);
    });

    function Redirect() {
        if ($("input[name='emailaddress']").val() == "") {
            clearInterval(myvar);
            if (window.location.href.indexOf("isyatirim-dev.xrm.link") != -1)
                window.location.href = "/E/ana-sayfa-KGeD9/tr-TR";
            else if (window.location.href.indexOf("isyatirim.xrm.link") != -1) {
                window.location.href = "/E/ana-sayfa-KGeD9/tr-TR";
            }
            else if (window.location.href.indexOf("kariyer.isyatirim.com.tr") != -1) {
                window.location.href = "/E/ana-sayfa-KGeD9/tr-TR";
            }
        }

        //location.protocol + "//" + location.host;
    }
});


function MakeRequired() {
    var Label = $("input[name='new_portalpassword']").parent().prev()
    var sHTml = "<span class='required' aria-required='true'>*</span>"
    if ($(Label).find("span").length == 0) {
        $(Label).attr("data-required", "applicationrequired")
        $(sHTml).appendTo($(Label))
    }


}

