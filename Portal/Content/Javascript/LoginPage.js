$(document).ready(function () {

    //For Toastr Options General Definition.
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "positionClass": "toast-top-right",
        "onclick": null
    }

    Metronic.init(); // init metronic core components
    Layout.init(); // init current layout
    $.ajax({
        url: "/Login/ClearCache",
        type: "POST",
        dataType: "json",
        async: false,
        contentType: "application/json; charset=utf-8",
        success: function (data) {
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
        }
    });

    $("#username").focus();
    $("#submitbutton").on("click", function (e) {
   
        try {
       
            if ($("#username").val() == "" || $("#password").val() == "") {
                $('.alert-danger').show();
                $('.alert-wrong').hide();
                return false;
            }
            else {
                $('.alert-danger').hide();
            }
           
            Metronic.blockUI({
                boxed: true
            });

            var obj = {};
            obj.UserName = $("#username").val();
            obj.Password = $("#password").val();
            $.ajax({
                url: "/Login/AttempLogin",
                type: "POST",
                data: JSON.stringify({ 'model': obj }),
                dataType: "json",
                async: true,
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    
                    Metronic.unblockUI();
                    if (data[0] == false) {
                        $("#username").effect("shake", { times: 4 }, 200);
                        $("#password").effect("shake", { times: 4 }, 200);

                        e.preventDefault(); //
                        $('.alert-wrong').show();
                        return false;
                    }
                    $('.alert-wrong').hide();
                    $('.alert-success').show();
                    $("#submitbutton").prop("disabled", true);
                    $("#forget-password").prop("disabled", true);
                    var url = "/Dashboard/Index";
                    if (window.location.href.split("returnurl=").length > 1) {
                        if (window.location.href.split("returnurl=")[1].indexOf("EWL") != -1 ||
                            window.location.href.split("returnurl=")[1].indexOf("ewl") != -1 ||
                            window.location.href.split("returnurl=")[1].indexOf("IsComingFromExternal=1") != -1) {
                            window.location.href = window.location.href.split("returnurl=")[1];
                        }
                    }
                    else
                        window.location.href = url;
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {


                    e.preventDefault();
                    alert(XMLHttpRequest.responseText);
                    Metronic.unblockUI();
                    return false;
                }
            });
        }
        catch (err) {
            return false;
        }
    });


    $("#password").keypress(function (e) {
        var key = e.which;
        if (key == 13)  // the enter key code
        {
            $("#submitbutton").click();
        }
    });
    $("#username").keypress(function (e) {
        var key = e.which;
        if (key == 13)  // the enter key code
        {
            $("#submitbutton").click();
        }
    });
    $("#forget-password").on("click", function () {
        $(".forget-form").show();
        $(".mainform").hide();
    });

    $("#back-btn").on("click", function () {
        $(".forget-form").hide();
        $(".mainform").show();


    });
    $("#submitreset").on("click", function () {
        if ($("#usernameoremail").val() == "") {
            toastr["error"](EmptyEmailMessage);
            return;
        }
        Metronic.blockUI();
       
        $.ajax({
            url: "/Login/ResetPassword",
            type: "POST",
            data: JSON.stringify({ 'Value':$("#usernameoremail").val() , 'LangId' : $(".langname").attr("data-nativename") }),
            dataType: "json",
            async: true,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data != "") {
                    toastr["error"](NoUserMessage);
                }
                else {
                    toastr["success"](SuccessMailMessage);
                }
                Metronic.unblockUI();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                toastr["error"](GetMeaningfulErrorMessage(XMLHttpRequest.responseText));
                Metronic.unblockUI();
            }
        });


    });
      
});



