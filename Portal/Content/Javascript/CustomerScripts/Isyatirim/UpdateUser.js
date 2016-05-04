$(document).ready(function () {
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "positionClass": "toast-top-right",
        "onclick": null
    }

    var fieldstobeformatted = ["hs_cellphone", "hs_homephone", "hs_workphone", "hs_fathercontactphone", "hs_spousecontactphone", "hs_mothercontactphone"]
    
    var formattedvaluesarray = []

    for (var i = 0; i < fieldstobeformatted.length; i++) {
        var elem = fieldstobeformatted[i]
        var formattedvaluesobject = {}
        $("input[name='" + elem + "']").attr("data-formatted", 1)
        formattedvaluesobject.OldValue = $("input[name='" + elem + "']").val()
        MakeFormat($("input[name='" + elem + "']"))
        formattedvaluesobject.NewValue = $("input[name='" + elem + "']").val()
        formattedvaluesobject.Id = elem
        formattedvaluesarray.push(formattedvaluesobject)
    }

    MakeRequired($("select[name='hs_gender']").val())
    $("select[name='hs_gender']").on("change", function () {
        MakeRequired($(this).val())

    });
  
    $("input").on("change", function () {
        var name = $(this).attr("name")
        if ($(this).attr("data-formatted") == 1) {
            var r = MakeFormat(this)
            if (r == false) {
                var dataArr = $.grep(formattedvaluesarray, function (element, index) {
                    return element.Id == name;
                });
                $(this).val(dataArr[0].OldValue)
                MakeFormat($("input[name='" + dataArr[0].Id + "']"))
                toastr["error"]("Geçersiz Format", "Hata")
            }
        }
    })
});

function MakeFormat(attr) {
    var tenDigitFormat = /^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{2})[-. ]?([0-9]{2})$/;
    var elevenDigitFormat = /^\(?([0-9]{4})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{2})[-. ]?([0-9]{2})$/;
    var digitedFormat = ""
    var returnelem = "";

    if (tenDigitFormat.test($(attr).val() )) {
      
        digitedformat = $(attr).val().replace(tenDigitFormat, "($1) $2 $3 $4");
        $(attr).val(digitedformat)
        returnelem = true;
    }
    else if (elevenDigitFormat.test($(attr).val())) {
     
        digitedformat = $(attr).val().replace(elevenDigitFormat, "($1) $2 $3 $4");
        $(attr).val(digitedformat)
        returnelem = true;
    }
    else {
        returnelem = false;
    }
    return returnelem;
}

function MakeRequired(value) {
    var Label = $("select[name='hs_militarystatus']").parent().prev()
    var sHTml = "<span class='required' aria-required='true'>*</span>"
    if (value == 50178) {
        if ($(Label).find("span").length == 0) {
            $(Label).attr("data-required", "applicationrequired")
            $(sHTml).appendTo($(Label))
        }    }
    else {
        $(Label).attr("data-required", "none")
        var removeLabel = $(Label).find("span")
        $(removeLabel).remove()

    }
}