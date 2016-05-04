var querystringarray = getUrlVars(document.location.href)

$(document).ready(function () {

    Metronic.init(); // init metronic core componets
    Layout.init(); // init layout

    toastr.options = {
        "closeButton": true,
        "debug": false,
        "positionClass": "toast-top-right",
        "onclick": null
    }

    UrlFieldsFunctions();

    if ($(".form_date > input").length > 0) {
        var dFormat = $(".form_date > input").attr("data-beforedateformat").replace(/M/g, 'm');

        $('.form_date').datepicker({
            rtl: Metronic.isRTL(),
            orientation: "right top",
            autoclose: true,
            format: dFormat
        });
    }

    if ($(".form_datetime > input").length > 0) {
        var dFormat = $(".form_datetime > input").attr("data-beforedateformat").replace(/M/g, 'm');
        var tFormat = "", showmeridian = "";

        if ($(".form_datetime > input").attr("data-beforetimeformat").indexOf('tt') != -1) {
            tFormat = "HH:ii P"
            showmeridian = true
        }
        else {
            tFormat = "hh:ii";
            showmeridian = false
        }
        $(".form_datetime").datetimepicker({
            isRTL: Metronic.isRTL(),
            format: dFormat + " " + tFormat,
            showMeridian: showmeridian,
            autoclose: true,
            pickerPosition: "bottom-left",
        });
    }
    $(".lookuplinkclicker").unbind();
    $(".lookuplinkclicker").on("click", function () {
        LookupLinker($(this));
    });
    $('body').removeClass("modal-open");

    $('input[data-type=datetime]').each(function () {
        var timepart = $(this).attr("data-timepicker");

        var dateformat = $(this).attr("data-dateformat");
        var timeformat = $(this).attr("data-timeformat");
        $(this).datetimepicker({

            timepicker: timepart == "dateandtime" ? true : false,
            format: timepart == "dateandtime" ? dateformat + " " + timeformat : dateformat,
            step: 60
        });
    });
    BindDataToEditForm(TempLookupLinkDetailId);

    //Bind Custom Actions
    $(".customactions").unbind();
    $(".customactions").on("click", function () {
        CustomActions($(this), TempLookupLinkDetailId);
    });

    $(".updater").unbind();
    $(".updater").on("click", function () {
        UpdateRecord($(this),false, TempLookupLinkDetailId);
    });

    $(".clicker").unbind();
    //Lookup Control
    $(".clicker").on("click", function (event, datas) {
        //block main page
        MakeLookup(this, datas, true);

    });

    $(".memo").each(function () {

        var value = $(this).val().split(/\r|\r\n|\n/).length;
        if (value < parseInt($(this).attr("rows"))) {
            $(this).attr("rows", value);
        }
    });

    $(".notesbrowser").unbind();
    $(".notesbrowser").on("change", function (handleFileSelect) {
        BrowseNote($(this), handleFileSelect, $(this).attr("data-count"));
    });

    $(".notesupdater").unbind();
    $(".notesupdater").on("click", function () {
        BindNotesUpdate($(this), TempLookupLinkDetailId);
    });

    $(".newer").unbind();
    $(".newer").on("click", function () {
        AddNotesToPage($(this));
    });

    $(".deleter").unbind();
    $(".deleter").on("click", function () {
        DeleteNotes($(this))
    });

    $(".notesdownloader").unbind();
    $(".notesdownloader").on("click", function () {
        DownloadData($(this).attr("data-mimetype"), $(this).attr("data-src"), $(this).attr("data-filename"))
    });

});
