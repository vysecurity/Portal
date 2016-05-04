///<reference path="GeneralFunctions.js" />
var unsupportedformat = "";
var notescounter = 1;

jQuery(document).ready(function () {
    //For Toastr Options General Definition.
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "positionClass": "toast-top-right",
        "onclick": null
    }

    //Get  Portal LangId and Localization XML
    var cookieval = readCookie(window.location.host + "langidcookie");
    LangId = $(".langname").attr("data-langid");
    BaseLangId = $(".langname").attr("data-baselangid");// cookieval.split("langid=")[1]
    LangIdNativeName = $(".langname").attr("data-nativename");

    $(".langnameelements[data-langid='" + LangId + "']").closest("li").css("display", "none");
    $(".lookuplinkclicker").on("click", function () {
        LookupLinker($(this));
    });
    //$(".langclicker").closest("a").on("click", function () {
    //    $.cookie(window.location.host + "languagecookie",$(this).find("span").attr("data-langid"));
    //});

    GetLocalizationXML(LangId);

    // For Signature
    if (document.getElementById("signature-pad") != null) {
        var wrapper = document.getElementById("signature-pad"),
           clearButton = wrapper.querySelector("[data-action=clear]"),
           CanvasParam = wrapper.querySelector("canvas"),
           SpadParam;

        function resizeCanvas() {
            var ratio = window.devicePixelRatio || 1;
            CanvasParam.width = CanvasParam.offsetWidth * ratio;
            CanvasParam.height = CanvasParam.offsetHeight * ratio;
            CanvasParam.getContext("2d").scale(ratio, ratio);
        }

        window.onresize = resizeCanvas;
        resizeCanvas();

        SpadParam = new SignaturePad(CanvasParam);
        SpadParamGeneral = SpadParam;
        clearButton.addEventListener("click", function (event) {
            SpadParam.clear();
        });

    }
    $(".menuclick > a").on("click", function () {
        if ($(this).attr("target") == "_blank") {
            return;
        }
        if ($(this).attr("href") != "")
            Metronic.blockUI({
                overlayColor: 'none',
                animate: true
            });
        if ($(this).attr("data-isparent") == "true" && $(this).attr("href") != "")
            window.location.href = $(this).attr("href")
    });
    if (navigationid != undefined) {

        $(".menuclick > a").each(function () {

            try {
                var h = $(this).attr("href");
                var url = h.split("/")

                if (url.length > 2) {
                    if (decodeURIComponent(url[2].toString()) == navigationid) {
                        //if ($(this).parent().parent().parent().prop('tagName').toLowerCase() == "li") {
                        //    $(this).parent().parent().parent().addClass("active");
                        //}
                        //else {
                        //    $(this).parent().addClass("active");
                        //}
                        $(this).closest(".parentmenu").addClass("active");
                    }
                }


            } catch (err) {

            }

        });
    }
    //personel form
    $(".personel").on("click", function () {
        window.location.href = "/Page/PersonelInfo/" + LangIdNativeName;
    });

    //charts
    $('div[data-type=chart]').each(function () {

        if ($(this).hasClass('isBeforeRendered')) {
            return false;
        } else {
            $(this).addClass('isBeforeRendered');
        }

        var id = $(this).attr("data-id");

        var chart = $.grep(chartarray, function (element, index) {
            return element.Id == id;
        });
        MakeChart(chart, id);
        
    });
    //for html widget
    $('div[data-type=htmlwidget]').each(function () {

        if ($(this).hasClass('isBeforeRendered')) {
            return false;
        } else {
            $(this).addClass('isBeforeRendered');
        }

        var id = $(this).attr("data-id")

        var html = $.grep(htmlwidgetarray, function (element, index) {
            return element.Id == id;
        });
        if (html.length > 0)
            $(html[0].data).appendTo($(this))
    });

    //for linkwidget - form link
    $('div[data-type=linkwidgetform]').unbind('click');
    $('div[data-type=linkwidgetform]').on('click', function () {

        var pagewidgetid = $(this).attr('data-pagewidgetid');
        var formid = $(this).attr('data-clickformid');
        var widgetid = $(this).attr('data-widgetid');
        var widgetguid = $(this).attr('data-widgetguid');

            Metronic.blockUI();
            $.ajax({
                url: "/Page/GetCreateForm",
                type: "GET",
                data: {
                    FormId: formid,
                    WidgetGuid: widgetguid,
                    WidgetId: widgetid,
                    PageWidgetId: pagewidgetid,
                    Language: window.location.href.split("/")[5].split("?")[0]
                },
                dataType: "html",
                async: true,
                success: function (result) {

                    $('#linkwidgetFormBody_' + pagewidgetid).html(result);
                    $('#responsiveLinkWidget_' + pagewidgetid).modal("toggle");

                    Metronic.unblockUI();
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    Metronic.unblockUI();
                }
            });

    });

    //for initial values and disable note field in create
    SetInitialValue(forminitialvaluearray);
    $('div[data-type=calculatedfield], div[data-type=linkwidgetgrid]').unbind("click");
    $('div[data-type=calculatedfield], div[data-type=linkwidgetgrid]').on("click", function () {
        if ($(this).attr("data-click") == "true") {
            
            if ($(this).attr("data-type") != 'linkwidgetgrid') {
                var value = $(this).find('div').find('.number').html().trim();

                if (value == "0")
                    return false;
            }

            var recordCount;
            if ($(this).attr("data-type") == 'linkwidgetgrid') {
                recordCount = parseInt($(this).attr("data-recordcount"));
            } else {
                recordCount = 10;
            }
            
            Metronic.blockUI({
                overlayColor: 'none',
                animate: true
            });

            var widgetid = $(this).attr("data-widgetid");
            var guid = $(this).attr("data-clickformid");
            var entityname = $(this).attr("data-entityname");
            var tableid = "calculated_" + widgetid;
            $.ajax({
                url: "/Page/GetGridForModal",
                type: "POST",
                data: JSON.stringify({ 'CalculatedWidgetId': widgetid, 'WidgetGuid': guid, "PageNumber": 1, "RecordCount": 10 }),
                dataType: "json",
                async: true,
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var d = [], columnsarray = [];

                    $("#" + tableid).closest(".scroller").css("height", "auto")
                    $("#" + tableid).closest(".slimScrollDiv").css("height", "auto")

                    //override scroller plugin to fit the lookup!
                    $("#" + tableid).closest(".scroller").css("height", "auto")
                    $("#" + tableid).closest(".slimScrollDiv").css("height", "auto")

                    if ($("#" + tableid + ' > thead').length > 0) {
                        $("#" + tableid).dataTable().fnDestroy();
                    }
                    else {
                        var obj = {};
                        obj.Id = tableid;
                        obj.data = data;
                        CalculatedFirstPageData.push(obj);

                    }

                    $('#' + tableid).empty();

                    BuildLookupDataAndColumn(JSON.parse(data.Content), tableid, d, entityname)

                    var table = $("#" + tableid);
                    var dtable = table.dataTable({
                        "bStateSave": false, // save datatable state(pagination, sort, etc) in cookie.
                        "data": d,
                        "bLengthChange": false,
                        // set the initial value
                        "pageLength": recordCount,
                        "pagingType": "simple",
                        "language": {
                            "search": searchfordatatable,
                            "lengthMenu": "  _MENU_ records",
                            "sInfo": totalrecords,
                            "paginate": {
                                "previous": prev,
                                "next": next,
                                "last": "Last",
                                "first": "First"
                            }
                        },

                        "order": [
                            [1, "asc"]
                        ],// set first column as a default sort by asc

                        fnRowCallback: function (nRow, aData, iDisplayIndex, iDisplayIndexFull) {
                            var tid = $(this).closest('table').attr("id");

                            var internalsearchvalue = $.grep(searchvalues, function (element, index) {
                                return element.Id == tid;
                            });
                            if (internalsearchvalue.length > 0) {
                                $("#" + tid + "_filter > label >input").val(internalsearchvalue[0].Value)

                            }


                        }
                    });
                    //disable name and id columns
                    if (d.length == 0) {
                        var newl = $("#" + tableid + " > thead > tr > th").length;
                        dtable.fnSetColumnVis(newl - 2, false);
                        dtable.fnSetColumnVis(newl - 1, false);

                    }
                    else {
                        dtable.fnSetColumnVis(d[0].length - 2, false);
                        dtable.fnSetColumnVis(d[0].length - 1, false);
                    }
                    $("#" + tableid).unbind('page.dt')
                    BindPageChangingOnCalcWidget(tableid, entityname, JSON.parse(this.data).CalculatedWidgetId, JSON.parse(this.data).WidgetGuid);

                    $('#divcalculated_' + widgetid).modal('toggle');
                    Metronic.unblockUI();
                    $("#" + $(table).attr("id") + "_filter > label > input").unbind();

                    $("#" + $(table).attr("id") + "_filter > label > input").bind('keyup', { tableid: tableid, WidgetId: JSON.parse(this.data).WidgetId }, function (e) {
                        toastr.options = {
                            "closeButton": true,
                            "debug": false,
                            "positionClass": "toast-top-right",
                            "onclick": null
                        }

                        if (e.keyCode == 13) {
                            if (this.value.length < 3 && this.value != "") {
                                toastr["error"]("", "Error")
                                return;
                            }

                            var idarr = $(this).closest("div").attr("id").split("_");
                            var id = "";
                            if (idarr.length == 2)
                                id = idarr[0];
                            else
                                id = idarr[0] + "_" + idarr[1];
                            Metronic.blockUI({
                                target: $("#" + tableid)
                            });

                            $("#" + e.data.tableid).dataTable().fnClearTable();
                            var searchvaluesobj = {};
                            searchvaluesobj.Id = tableid;
                            searchvaluesobj.Value = this.value;
                            
                            searchvalues = [];
                            searchvalues.push(searchvaluesobj);

                            if (this.value == "") {
                                previous = [];
                                flagwidget = "";
                                var griddata = $.grep(CalculatedFirstPageData, function (element, index) {
                                    return element.Id == tableid;
                                });
                                var d = [];
                                var x = $("#" + tableid).dataTable();
                                BuildLookupDataAndColumn(JSON.parse(griddata[0].data.Content), tableid, d, entityname)
                                AddDataToExistingDataTable(d, x);
                                Metronic.unblockUI($("#" + tableid));
                                toastr["success"](SearchMessage, SuccessMessageHeader)
                                return;
                            }
                            $.ajax({
                                url: "/Page/GetSearchGridForModal",
                                type: "POST",
                                data: JSON.stringify({ 'CalculatedWidgetId': widgetid, 'WidgetGuid': guid, "PageNumber": 1, "RecordCount": 10, "SearchValue": this.value }),
                                dataType: "json",
                                async: true,
                                contentType: "application/json; charset=utf-8",
                                success: function (data) {
                                    var d = [];

                                    var x = $("#" + tableid).dataTable();
                                    BuildLookupDataAndColumn(JSON.parse(data.Content), tableid, d, entityname)
                                    AddDataToExistingDataTable(d, x);
                                    Metronic.unblockUI($("#" + tableid));
                                    toastr["success"](SearchMessage, SuccessMessageHeader)
                                },
                                error: function (XMLHttpRequest, textStatus, errorThrown) {
                                    Metronic.unblockUI($("#" + tableid));
                                }
                            });

                        }
                    });
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    Metronic.unblockUI();
                }
            });
        }
    });

    $('table[data-type=grid]').each(function () {
        
        if ($(this).hasClass('isBeforeRendered')) {
            return false;
        } else {
            $(this).addClass('isBeforeRendered');
        }

        var table = $(this);
        var columns = [];
        $("#" + $(this).attr("id") + " > thead > tr > th").each(function () {
            var obj = {};
            obj.orderable = true;
            columns.push(obj);
        })
        // begin first table
        var tableid = $(this).attr("id");
        var griddata = $.grep(gridarray, function (element, index) {
            return element.Id == tableid;
        });
        var d = [], columnsarray = [], width = [];

        ChangeMvcDataToDataTableGridData(griddata, columnsarray, d, width);

        var dtable = MakeDataTableForGrid(table, d, columnsarray, $(this).attr("data-recordperpage"), width)

        dtable.fnSetColumnVis(dtable.DataTable().columns()[0].length - 1, false);


        if ($(this).attr("data-onclick") == "true") {
            dtable.on('click', 'tr', function () {
                GridOperations($(this));
            });
        }
    });
    //Bind Custom Actions
    $(".customactions").unbind();
    $(".customactions").on("click", function () {
        CustomActions($(this), $(this).closest(".formwidgetclass").attr("data-entityid"));
    });

    //select all calendars in a page
    $('div[data-type=calendar]').each(function () {

        if ($(this).hasClass('isBeforeRendered')) {
            return false;
        } else {
            $(this).addClass('isBeforeRendered');
        }

        var id = $(this).attr("id");

        var h = {};

        if ($('#' + id).width() <= 400) {
            $('#' + id).addClass("mobile");
            h = {
                left: 'title, prev, next',
                center: '',
                right: 'today,month,agendaWeek,agendaDay'
            };
        } else {
            $('#' + id).removeClass("mobile");
            if (Metronic.isRTL()) {
                h = {
                    right: 'title',
                    center: '',
                    left: 'prev,next,today,month,agendaWeek,agendaDay'
                };
            } else {
                h = {
                    left: 'title',
                    center: '',
                    right: 'prev,next,today,month,agendaWeek,agendaDay'
                };
            }
            $('#' + id).fullCalendar('destroy'); // destroy the calendar

            var events = [];
            //init data
            var startyear = "", startmonth = "", startday = "";
            // Important --> calendar array parameters fill in mvc razor.Ref --> BuildPage.cshtml
            if (calendararray.length > 0) {
                for (var i = 0; i < calendararray[0].data.length; i++) {
                    var d = calendararray[0].data[i];
                    var newstartdate = new Date(d.StartDateValue)

                    var newEvent = new Object();

                    newEvent.title = d.Value;

                    newEvent.start = d.StartDateValue;
                    newEvent.end = d.EndDateValue;
                    newEvent.allDay = false;
                    events.push(newEvent);
                }
            }
            calendarloc = LangIdNativeName.split("-")[0];

            $('#' + id).fullCalendar({ //re-initialize the calendar                
                disableDragging: true,
                header: h,
                editable: false,
                events: events,
                allDay: false,
                timeFormat: 'H(:mm)', // uppercase H for 24-hour clock
                lang: calendarloc,
                eventRender: function (event, element) {
                }
            });

        }
    });

    $.getScript("/assets/global/plugins/bootstrap-datepicker/js/locales/bootstrap-datepicker." + LangIdNativeName.split("-")[0] + ".js", function () {
        if ($(".form_date > input").length > 0) {
            var dFormat = $(".form_date > input").attr("data-beforedateformat").replace(/M/g, 'm');

            $('.form_date').datepicker({
                rtl: Metronic.isRTL(),
                orientation: "right top",
                autoclose: true,
                format: dFormat,
                language: LangIdNativeName.split("-")[0],
                scrollInput: false
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
                language: LangIdNativeName.split("-")[0],
                scrollInput: false
            });
        }

        $('input[data-type=datetime]').each(function () {
            var timepart = $(this).attr("data-timepicker");

            var dateformat = $(this).attr("data-dateformat");
            var timeformat = $(this).attr("data-timeformat");
            $(this).datetimepicker({

                timepicker: timepart == "dateandtime" ? true : false,
                format: timepart == "dateandtime" ? dateformat + " " + timeformat : dateformat,
                step: 60,
                language: LangIdNativeName.split("-")[0],
                scrollInput: false
            });
        });
    });  

    //Lookup Control
    $(".clicker").on("click", function (event, datas) {
        //block main page
        MakeLookup(this, datas, false);
    });
    //change profile picture
    $(".picturemenu").on("click", function () {
        try {
            $.ajax({
                url: "/Dashboard/ChangePicture",
                type: "GET",
                dataType: "html",
                async: true,
                success: function (result) {
                    $("#table_responsive_changepicture").closest(".scroller").css("height", "auto")
                    $("#table_responsive_changepicture").closest(".slimScrollDiv").css("height", "auto")
                    $("#table_responsive_changepicture").html(result)
                    $("#responsive_changepicture").modal('toggle');
                    Metronic.unblockUI();
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    Metronic.unblockUI();
                }
            });
        }
        catch (err) {
            alert(err);
        }
    })
    //for subgrid
    RenderSubGrids();
    //send data to crm
    $(".submitter").on("click", function () {
        CreateRecord($(this));
    });
    //make checkboxes unchecked
    $(".booleanclicker").on("click", function () {
        $(this).parent().siblings().find("input").removeAttr("checked");
        $(this).attr("checked", "checked");
    });
    //change file
    $("#files").on("change", function (handleFileSelect) {
        var files = handleFileSelect.target.files;

        if (!files[0].type.match('image.*')) {
            unsupportedformat = "unsupportedformat";
            toastr["error"](UnSupportedFileFormat, ErrorMessageHeader);
            return;
        }
        unsupportedformat = "";
        $("#browsetext").text(files[0].name);

        var reader = new FileReader();

        reader.onloadend = function () {
            $("#profilerinside").attr("src", $(this).attr("result"));
        }

        var str = reader.readAsDataURL(files[0]);

    });
    // Make URL clickable
    UrlFieldsFunctions();

    $("#updatepicture").on("click", function () {
        if (unsupportedformat != "") {
            toastr["error"](UnSupportedFileFormat, ErrorMessageHeader);
            return;
        }
        Metronic.blockUI({
            target: $("#picturedialog")
        });

        $.ajax({
            url: "/Page/UpdateEntityImage",
            type: "POST",
            data: JSON.stringify({ 'Image': $("#profilerinside").attr("src").split(",")[1] }),
            dataType: "json",
            async: true,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                var d = JSON.parse(data.Content);
                if (d != "") {
                    toastr["error"](d, ErrorMessageHeader);

                }
                else {
                    $("#profiler").attr("src", $("#profilerinside").attr("src"))
                    toastr["success"](UpdateImageMessage, SuccessMessageHeader);
                }
                Metronic.unblockUI($("#picturedialog"));
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                toastr["error"]("", ErrorMessageHeader);
                Metronic.unblockUI($("#picturedialog"));
            }
        });
    });

    $(".notesbrowser").on("change", function (handleFileSelect) {
        BrowseNote($(this), handleFileSelect, $(this).attr("data-count"));
    });

    $(".notesupdater").on("click", function () {
        BindNotesUpdate($(this), $(".formwidgetclass").attr("data-entityid"), $(".formwidgetclass").attr("data-entityid"));
    });

    $(".newer").on("click", function () {
        AddNotesToPage($(this));
    });

    $(".deleter").on("click", function () {
        DeleteNotes($(this))
    });

    $(".notesdownloader").on("click", function () {
        DownloadData($(this).attr("data-mimetype"), $(this).attr("data-src"), $(this).attr("data-filename"))
    });

    // For counterup - calculated widget
    $('[data-counter="counterup"]').counterUp({
        delay: 10,
        time: 1000
    });

    $('[data-isexternelurl="true"]').on('click', function () {
        window.open($(this).attr('data-externelurl'));
    });
    
});

