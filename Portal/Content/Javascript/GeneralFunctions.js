var blockelement = [];
var SpadParamGeneral = {}, CanvasParamGeneral = {};
var signature = "";
var griddataArray = [];
var pagenumberfordatatable = "";
var currenttable = "";
var recordperpagefordatatable = "";
var flag = "", flagwidget = "";
var previous = [];
var searchvalues = [];
var lookupfirstpagedata = [], CalculatedFirstPageData = [];
var click = "";
var querystringarray = getUrlVars(document.location.href);
var navigationid = "", redirecturl = "";
var customerEntity = ["Account", "Contact"];
var selectedvalue = "";
var LocalizationXML = "";
var LangId = "", BaseLangId = "", LangIdNativeName = "";
var TempSubGridTableId = "";
var TempLookupLinkDetailId = "", SubGridDetailId = "";
//Jquery DataTable Parameters
var prev = "", next = "", searchfordatatable = "", totalrecords = "", EmptyTable = "", InfoEmpty = "";
//Toastr Messages
var SearchMessage = "", SuccessMessageHeader = "", SearchCriteria = "", ErrorMessageHeader = "", UpdateMessage = "", UpdateMessageError = "", CreateMessage = "", ErrorCreateMessage = "", UpdateImageMessage = "", UnSupportedFileFormat = "",
 AddAttachment = "", DeleteAttachment = "", NoFile = "", NoFileForDelete = "", RequiredMessage = "", SubGridContent = "", NewButton = "", UpdateButton = "", NoSubGridCreateForm = "", NoSubGridUpdateForm = "", EmptyEmailMessage = "", NoUserMessage = "",
 SuccessMailMessage = "", Actions = "", GeneralSuccessMessage = "";
//Init from value array
var initFormValueArray = [];

function getUrlVars(url) {
    var vars = [], hash;
    var hashes = url.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

function GetLocalizationXML(langid) {
    $.ajax({
        type: "GET",
        url: "/Helper/Localization.xml",
        dataType: "xml",
        async: false,
        success: function (xml) {
            LocalizationXML = $(xml).find("lang[code='" + langid + "']");
            if (LocalizationXML.length == 0) {
                LocalizationXML = $(xml).find("lang[code='1033']");
            }
            //set jquery datable localization variables
            prev = $(LocalizationXML).find("Prev").text();
            next = $(LocalizationXML).find("Next").text();
            searchfordatatable = $(LocalizationXML).find("SearchValue").text();
            totalrecords = $(LocalizationXML).find("TotalRecords").text();
            SearchMessage = $(LocalizationXML).find("SearchMessage").text();
            SuccessMessageHeader = $(LocalizationXML).find("SuccessMessageHeader").text();
            SearchCriteria = $(LocalizationXML).find("SearchCriteria").text();
            ErrorMessageHeader = $(LocalizationXML).find("ErrorMessageHeader").text();
            UpdateMessage = $(LocalizationXML).find("UpdateMessage").text();
            UpdateMessageError = $(LocalizationXML).find("ErrorUpdateMessage").text();
            CreateMessage = $(LocalizationXML).find("CreateMessage").text();
            ErrorCreateMessage = $(LocalizationXML).find("ErrorCreateMessage").text();
            UpdateImageMessage = $(LocalizationXML).find("UpdateImageMessage").text();
            UnSupportedFileFormat = $(LocalizationXML).find("UnSupportedFileFormat").text();
            AddAttachment = $(LocalizationXML).find("AddAttachment").text();
            DeleteAttachment = $(LocalizationXML).find("DeleteAttachment").text();
            NoFileForDelete = $(LocalizationXML).find("NoAttachmentForDelete").text();
            NoFile = $(LocalizationXML).find("NoAttachmentForInsert").text();
            RequiredMessage = $(LocalizationXML).find("Required").text();
            EmptyTable = $(LocalizationXML).find("EmptyTable").text();
            InfoEmpty = $(LocalizationXML).find("InfoEmpty").text();
            SubGridContent = $(LocalizationXML).find("SubGridContent").text();
            NewButton = $(LocalizationXML).find("NewRecordFromSubGrid").text();
            UpdateButton = $(LocalizationXML).find("Update").text();
            SubGridCreateForm = $(LocalizationXML).find("NoNewSubGridForm").text();
            NoSubGridUpdateForm = $(LocalizationXML).find("NoUpdateSubGridForm").text();
            EmptyEmailMessage = $(LocalizationXML).find("EmptyEmailMessage").text();
            NoUserMessage = $(LocalizationXML).find("NoUserMessage").text();
            SuccessMailMessage = $(LocalizationXML).find("SuccessMailMessage").text();
            Actions = $(LocalizationXML).find("Actions").text();
            GeneralSuccessMessage = $(LocalizationXML).find("GeneralSuccessMessage").text();
        },
        error: function (XMLHttpRequest, textStatus, errorThrow) {

        }

    });
}

function LookupLinker(obj) {
    var parentmodal = $(obj).closest(".portlet.light");

    Metronic.blockUI({
        target: parentmodal
    });

    var modaldiv = $("#responsive_lookupdetail_" + $(obj).attr("id") + "_" + $(obj).attr("data-count"));
    var willbeinsertteddiv = $("#responsive_lookupdetail_" + $(obj).attr("id") + "_" + $(obj).attr("data-count")).find("div[class='portlet-body divlookupcontent']");
    $(modaldiv).find(".close").css("display", "none")
    $(modaldiv).find(".modal-footer").find("button").attr("data-dismiss", "")
    $(modaldiv).find(".modal-footer").find("button").bind("click", function () {
        $(modaldiv).modal('hide');
        $(willbeinsertteddiv).empty();
    });

    $.ajax({
        url: "/Page/EditForm",
        type: "GET",
        data: {
            FormId: $(obj).attr("data-openformid"),
            WidgetId: $(obj).attr("data-widgetid"),
            PageWidgetId: $(obj).attr("data-pagewidgetid"),
            DataId: $(obj).attr("data-id"),
            NavigationId: navigationid != "" ? navigationid : querystringarray["NavigationId"],
            Editable: $(obj).attr("data-editable"),
            Ownership: "0",
            FormOpenType: "modal",
            Language: LangId,
            IsComingFromLookupDetail: "1"
        },
        dataType: "html",
        async: true,
        success: function (result) {
            TempLookupLinkDetailId = $(obj).attr("data-id");
            $(modaldiv).closest(".scroller").css("height", "auto");
            $(modaldiv).closest(".slimScrollDiv").css("height", "auto");
            $(willbeinsertteddiv).html(result);
            $(willbeinsertteddiv).find(".portlet-title").css("display", "none");

            $(modaldiv).modal('toggle');
            Metronic.unblockUI(parentmodal);
            //RenderSubGrids("true");
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            Metronic.unblockUI(parentmodal);
        }
    });


}

function readCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0)
            return c.substring(nameEQ.length, c.length);
    }
    return null;
}

function Validation(object, IsSubGrid) {
    var returnvalue = "";

    toastr.options = {
        "closeButton": true,
        "debug": false,
        "positionClass": "toast-top-right",
        "onclick": null
    }
    $(object).closest('.portlet.light').find("[data-attribute=yes][data-disabled='']").each(function () {
        if (IsSubGrid == undefined) {
            if ($(this).attr("data-subgrid") == "true") {
                return;

            }
        }

        var type = $(this).attr("data-type");

        if (type == "string" || type == "ınteger" || type == "integer" || type == "datetime" || type == "double" || type == "decimal" || type == "money" || type == "memo") {
            if ($(this).attr("disabled") == undefined) {
                if ($(this).parent().prev().attr("data-required") == "applicationrequired") {
                    if ($(this).val() == "") {
                        toastr["error"]($(this).parent().prev().text().replace("*", "") + RequiredMessage, ErrorMessageHeader);
                        $(this).focus();
                        returnvalue = "error";
                        return false;
                    }
                }
            }
        }
        else if (type == "lookup") {
            if ($(this).parent().parent().prev().attr("data-required") == "applicationrequired") {
                if ($(this).val() == "") {
                    toastr["error"]($(this).parent().parent().prev().text().replace("*", "") + RequiredMessage, ErrorMessageHeader);
                    $(this).next().find("button").focus();
                    returnvalue = "error";
                    return false;
                }
            }

        }
        else if (type == "picklist") {
            if ($(this).parent().prev().attr("data-required") == "applicationrequired") {
                if ($(this).attr("disabled") == undefined) {
                    if ($(this).val() == "null" || $(this).val() == null) {
                        toastr["error"]($(this).parent().prev().text().replace("*", "") + RequiredMessage, ErrorMessageHeader);
                        $(this).focus();
                        returnvalue = "error";
                        return false;
                    }
                }
            }
        }
        else if (type == "boolean") {
            if ($(this).parent().parent().parent().prev().attr("data-required") == "applicationrequired") {
                var inputs = $(this).parent().parent().find("input");
                var val = 0;
                $(inputs).each(function () {
                    if ($(this).attr("checked") == undefined) {
                        val = val + 1;
                    }
                });
                if (val == 2) {
                    toastr["error"]($(this).parent().parent().parent().prev().text().replace("*", "") + RequiredMessage, ErrorMessageHeader);
                    $(this).focus();
                    returnvalue = "error";
                    return false;
                }
            }
        }

    });
    return returnvalue;
}

function getHighest(array) {
    var max = {};
    for (var i = 0; i < array.length; i++) {
        if (array[i].Page > (max.Page || 0))
            max = array[i];
    }
    return max;
}

function MakeDataTableForSubGrid(table, CreateOrNot, cols) {

    var dtable = table.DataTable({
        destroy: true,
        columns: cols,
        //"bStateSave": true, // save datatable state(pagination, sort, etc) in cookie.
        "lengthMenu": [
            [5, 15, 20, -1],
            [5, 15, 20, "All"] // change per page values here
        ],
        // set the initial value
        "pageLength": 5,
        "pagingType": "bootstrap_full_number",
        "language": {
            "sInfoEmpty": InfoEmpty,
            "sEmptyTable": CreateOrNot == 1 ? SubGridContent : EmptyTable,
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
        "columnDefs": [{  // set default column settings
            'orderable': false,
            'targets': [0]
        }, {
            "searchable": true,
            "targets": [0]
        }],
        "order": [
            [1, "asc"]
        ], // set first column as a default sort by asc

    });
    $(".dataTables_filter > label").css("font-size", "12px")
    dtable.column($(table).find("th").length - 1).visible(false);

    return dtable;
}

function MakeDataTableForGrid(tableobject, data, columns, recordperpage, width) {

    var divs = "";

    var griddata = $.grep(gridarray, function (element, index) {
        return element.Id == $(tableobject).attr("id");
    });

    if (griddata.length > 0) {

        var actiondata = griddata[0].Actions;

        if (actiondata.length > 0) {
            divs = '<div class="row">';
            if (actiondata.length == 1)
                divs += '<div class="col-sm-offset-3">';
            else
                divs += '<div class="col-sm-offset-2">';

            divs += '<div class="btn-group" data-toggle="buttons">';
            for (var i = 0; i < actiondata.length; i++) {
                var openformid = "";

                if (actiondata[i].GridActions == "1")
                    openformid = actiondata[i].OpenFormId;
                var ownership = "";
                if (actiondata[i].UseOwnerShip == "true") {
                    ownership = "1";
                }
                else {
                    ownership = "0";
                }
                divs += '<button id="' + actiondata[i].Id + '" data-openformid="' + openformid + '" data-ownership="' + ownership + '" data-editable ="' + actiondata[i].IsEditable + '" data-openwidgettype="' + actiondata[i].OpeningWidgetType + '" data-openstyle = "' + actiondata[i].OpenStyle + '"';
                divs += 'class="btn btn-sm viewaction filter-submit margin-bottom" style="background-color:' + actiondata[i].Color + ';color:' + actiondata[i].FontColor + '">';
                divs += ' <i class="fa fa-search"></i>' + actiondata[i].DisplayName + '</button>';

            }
            divs += '</div></div></div>';

        }
    }

    var dtable = tableobject.dataTable({
        //"bStateSave": true, // save datatable state(pagination, sort, etc) in cookie.
        'bAutoWidth': false,
        "aoColumns": width,
        "data": data,
        "columns": columns == "" ? null : columns,
        "bLengthChange": false,
        // set the initial value
        "pageLength": parseInt(recordperpage),
        "caseInsen": true,
        "pagingType": "simple",//"bootstrap_full_number",
        "bResetDisplay": false,
        "language": {
            "sInfoEmpty": InfoEmpty,
            "sEmptyTable": EmptyTable,
            "sInfo": totalrecords,
            "search": searchfordatatable,
            "lengthMenu": "  _MENU_ records",
            "paginate": {
                "previous": prev,
                "next": next
            }
        },
        "order": []
        ,
        "fnCreatedRow": function (nRow, aData, iDataIndex) {
            if (divs != "") {
                var AddedButton = $('td:eq(' + (aData.length - 2) + ')', nRow).append(divs);
                $(AddedButton).find("button").attr("id", $(AddedButton).find("button").attr("id") + "_" + iDataIndex);
                $(AddedButton).find("button").on("click", function () {
                    var ComeFromCustomAction = {};
                    ComeFromCustomAction.OpeningWidgetType = $(this).attr("data-openwidgettype");
                    ComeFromCustomAction.Editable = $(this).attr("data-editable");
                    ComeFromCustomAction.OwnerShip = $(this).attr("data-ownership");
                    ComeFromCustomAction.OpenStyle = $(this).attr("data-openstyle");
                    ComeFromCustomAction.OpenFormId = $(this).attr("data-openformid");
                    GridOperations($(this).closest("tr"), ComeFromCustomAction);
                });
            }
        },

    });


    var tableID = $(tableobject).attr("id");
    var isExcelReport;
    $(gridarray).each(function (index, value) {
        if (value.Id == tableID) {
            isExcelReport = value.IsExcelExport;
            return false;
        }
    });

    if (isExcelReport != null && isExcelReport != '' && isExcelReport == 'true') {
        var html = "<div class='btn-group'>";;
        html += "<button id=exportexcel_" + $(tableobject).attr("id").replace("'", "") + " class='btn purple exporter'>"
        html += "<i class='fa fa-file-excel-o  pull-left'></i>" + $(LocalizationXML).find("ExportExcel").text();
        html += " </button>";
        html += "</div>";

        $(html).appendTo($("#" + $(tableobject).attr("id") + "_filter").parent().prev());

        $("#exportexcel_" + $(tableobject).attr("id")).on("click", function () {
            $("#" + $(tableobject).attr("id")).tableExport({ type: 'excel', escape: 'false' });

        });
    }

    $("#" + $(tableobject).attr("id") + "_filter > label > input").unbind();
    $("#" + $(tableobject).attr("id") + "_filter > label > input").bind('keyup', function (e) {

        if (e.keyCode == 13) {
            if (this.value.length < 3 && this.value != "") {

                toastr.options = {
                    "closeButton": true,
                    "debug": false,
                    "positionClass": "toast-top-right",
                    "onclick": null
                }
                toastr["error"](SearchCriteria, ErrorMessageHeader)
                return;
            }
            dtable.fnClearTable();
            var id = $(this).closest("div").attr("id").split("_")[0];
            Metronic.blockUI({
                target: $("#" + id)
            });

            var searchvaluesobj = {};
            searchvaluesobj.Id = id;
            searchvaluesobj.Value = this.value;

            searchvalues = [];
            searchvalues.push(searchvaluesobj);

            if (this.value == "") {
                var griddata = $.grep(gridarray, function (element, index) {
                    return element.Id == id;
                });
                var d = [], width = [];
                var x = $("#" + id).dataTable();
                ChangeMvcDataToDataTableGridData(griddata, null, d, width);
                AddDataToExistingDataTable(d, x);
                Metronic.unblockUI($("#" + id));
                return;
            }

            $.ajax({
                url: "/Page/GetSearchGridData",
                type: "POST",
                data: JSON.stringify({ 'WidgetId': $("#" + id).attr(("id")), 'PageNumber': "", 'RecordCount': $("#" + id).attr(("data-recordperpage")), 'SearchValue': this.value }),
                dataType: "json",
                async: true,
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var d = [];

                    var x = $("#" + id).dataTable();

                    ChangeWcfDataToDataTableGridData(JSON.parse(data.Content), d);
                    AddDataToExistingDataTable(d, x);
                    Metronic.unblockUI($("#" + JSON.parse(this.data).WidgetId));
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    Metronic.unblockUI($("#" + JSON.parse(this.data).WidgetId));
                }
            });
        }
    });

    $(tableobject).on('page.dt', function (e, a, c) {
        //jquery.datatables.js 3452 override to pass custom parameter.
        if (a.customaction == "previous") {
            var ob = {};
            ob.Page = (a._iDisplayStart / a._iDisplayLength) + 2;
            ob.Id = $(this).attr("id");
            previous.push(ob);
            return;
        }
        if (flag != "") {
            flag = "";
            return false;
        }
        var id = $(this).attr("id");
        var page = (a._iDisplayStart / a._iDisplayLength) + 1;

        var previousarray = $.grep(previous, function (element, index) {
            return element.Id == id
        });
        //daha önce previousa bastıysa
        if (previousarray.length != 0) {

            var highest = getHighest(previousarray);
            //previous yapıp tekrar next yaparsa fazladan data eklemeyi önlemek için
            if (highest.Page >= page)
                return;
        }

        Metronic.blockUI({
            target: $(this)
        });
        var internalsearchvalue = $.grep(searchvalues, function (element, index) {
            return element.Id == id;
        });

        if (internalsearchvalue.length > 0 && internalsearchvalue[0].Value != "") {

            $.ajax({
                url: "/Page/GetSearchGridData",
                type: "POST",
                data: JSON.stringify({ 'WidgetId': id, 'PageNumber': page, 'RecordCount': $(this).attr("data-recordperpage"), 'SearchValue': internalsearchvalue[0].Value }),
                dataType: "json",
                async: true,
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var d = [];

                    var x = $("#" + id).dataTable();
                    ChangeWcfDataToDataTableGridData(JSON.parse(data.Content), d);
                    AddDataToExistingDataTable(d, x);
                    flag = "click";
                    x.fnPageChange(page - 1, true);
                    $("#" + id + "_filter > label >input").val(internalsearchvalue[0].Value)
                    Metronic.unblockUI($("#" + JSON.parse(this.data).WidgetId));
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    Metronic.unblockUI($("#" + JSON.parse(this.data).WidgetId));
                }
            });
        }
        else {
            $.ajax({
                url: "/Page/GetGridData",
                type: "POST",
                data: JSON.stringify({ 'WidgetId': id, 'PageNumber': page, 'RecordCount': $(this).attr("data-recordperpage") }),
                dataType: "json",
                async: true,
                contentType: "application/json; charset=utf-8",
                success: function (data) {

                    var generalinsertdata = [];

                    //$("#" + id).DataTable().clear()
                    var x = $("#" + JSON.parse(this.data).WidgetId).dataTable();

                    var alldata = x.fnGetData();

                    ChangeWcfDataToDataTableGridData(JSON.parse(data.Content), generalinsertdata);
                    AddDataToExistingDataTable(generalinsertdata, x)
                    //MakeDataTableForGrid(x, generalinsertdata,null,25)
                    x.dataTable().fnDraw();
                    flag = "click";
                    x.fnPageChange(page - 1, true);
                    Metronic.unblockUI($("#" + JSON.parse(this.data).WidgetId));
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    Metronic.unblockUI($("#" + JSON.parse(this.data).WidgetId));
                }
            });
        }
    }).dataTable();

    return dtable;
}

function ChangeMvcDataToDataTableGridData(griddata, columnsarray, d, width) {

    for (var i = 0; i < griddata[0].data.length; i++) {
        var x = griddata[0].data[i].Data;
        if (columnsarray != null) {
            var totalwidth = 0;

            if (i == 0) {
                for (var t = 0; t < x.length; t++) {
                    var columnobject = {};
                    columnobject.title = x[t].DisplayName
                    columnobject.orderable = true;
                    columnsarray.push(columnobject);

                    var obj = {};
                    obj = { sWidth: x[t].Width + "%" };
                    totalwidth += parseFloat(x[t].Width);
                    width.push(obj);
                }
                if (parseInt(griddata[0].ActionButtonsCount) > 0) {
                    var columnobject = {};
                    columnobject.title = Actions;
                    columnobject.orderable = true;
                    columnsarray.push(columnobject);
                }

                var columnobject = {};
                columnobject.title = "MyIdColumn";
                columnobject.orderable = true;
                columnsarray.push(columnobject);

                if (griddata[0].ActionButtonsCount > 0) {
                    var obj = {};
                    if (griddata[0].PercentageTotalWidth == "")
                        griddata[0].PercentageTotalWidth = 10;

                    obj = { sWidth: ((totalwidth * parseInt(griddata[0].ActionButtonsCount) * parseInt(griddata[0].PercentageTotalWidth)) / 100).toString() + "%" }
                    width.push(obj);
                }

                var obj = {};
                obj = { sWidth: "50%" };
                width.push(obj);
            }
        }
        if (griddata[0].data[0].IsEmptyGrid != "1") {
            var subarr = [];
            for (var k = 0; k < x.length; k++) {
                subarr.push(x[k].Value);
            }
            if (parseInt(griddata[0].ActionButtonsCount) > 0)
                subarr.push("");

            subarr.push(x[0].RecordId);
            d.push(subarr);
        }

    }
}

function ChangeWcfDataToDataTableGridData(mydata, generalinsertdata) {

    if (mydata[0].IsEmptyGrid == "1") {
        return;
    }
    else {
        for (var i = 0; i < mydata.length; i++) {
            var myarraydata = mydata[i].Data;
            var insertdata = [];
            for (var j = 0; j < myarraydata.length; j++) {
                insertdata.push(myarraydata[j].Value);
            }
            if (mydata[0].HasAction == "true")
                insertdata.push("");
            insertdata.push(myarraydata[0].RecordId);
            generalinsertdata.push(insertdata);
        }
    }
}

function AddDataToExistingDataTable(generalinsertdata, table, Addactions) {
    for (var i = 0; i < generalinsertdata.length; i++) {
        var ii = generalinsertdata[i];
        table.fnAddData(ii);
    }
}

function BuildLookupDataAndColumn(data, tableid, d, entityname) {
    for (var i = 0; i < data.length; i++) {
        var x = data[i].Data;
        if (i == 0 && $("#" + tableid + ' > thead').length == 0) {
            var thead = $("<thead>").appendTo("#" + tableid);
            var tr = $("<tr>").appendTo(thead);

            for (var t = 0; t < x.length; t++) {
                if (x[t].ColumnName != entityname + "id") {
                    var th = $("<th>").appendTo(tr);
                    $(th).attr("data-logicalname", x[t].ColumnName)
                    $(th).text(x[t].DisplayName)
                }
            }
            var th = $("<th>").appendTo(tr);
            $(th).text("Name");
            var th = $("<th>").appendTo(tr);
            $(th).text("Id");
        }
        if (data[i].IsEmptyGrid != "1") {
            var subarr = [];
            var recordid = "", namevalue = "";
            for (var k = 0; k < x.length; k++) {
                if (x[k].ColumnName != entityname + "id") {
                    subarr.push(x[k].Value);
                }

                if (x[k].NameAttributeValue != null && namevalue == "")
                    namevalue = x[k].NameAttributeValue;
                if (x[k].RecordId != null && recordid == "")
                    recordid = x[k].RecordId;
            }
            subarr.push(namevalue);
            subarr.push(recordid);
            d.push(subarr);
        }

    }
}

function BindPageChangingOnLookup(tableid, entityname, widgetid, AttributeLogicalName) {
    $("#" + tableid).bind('page.dt', function (e, a, c) {
        var blocker = $("#" + tableid).closest('.portlet.light');
        Metronic.blockUI({
            target: $(blocker),
            overlayColor: 'none',
            animate: true
        });

        //jquery.datatables.js 3452 override to pass custom parameter.
        if (a.customaction == "previous") {
            var ob = {};
            ob.Page = (a._iDisplayStart / a._iDisplayLength) + 2;
            ob.Id = $(this).attr("id");
            previous.push(ob);
            Metronic.unblockUI($(blocker));
            return;
        }
        if (flag != "") {
            flag = "";
            Metronic.unblockUI($(blocker));
            return false;
        }
        var id = $(this).attr("id");
        var page = (a._iDisplayStart / a._iDisplayLength) + 1;
        if (page == 2) {
            var blockUITop = $(".block-spinner-bar").parent().parent().css("top");
            $(".block-spinner-bar").parent().parent().css("top", parseInt(blockUITop) - 80);
        }
        var previousarray = $.grep(previous, function (element, index) {
            return element.Id == id
        });
        //daha önce previousa bastıysa
        if (previousarray.length != 0) {

            var highest = getHighest(previousarray);
            //previous yapıp tekrar next yaparsa fazladan data eklemeyi önlemek için
            if (highest.Page >= page) {
                Metronic.unblockUI($(blocker));
                return;
            }

        }
        var internalsearchvalue = $.grep(searchvalues, function (element, index) {
            return element.Id == tableid;
        });
        if (internalsearchvalue.length > 0 && internalsearchvalue[0].Value != "") {
            $.ajax({
                url: "/Lookup/GetSearchCrmData",
                type: "POST",
                data: JSON.stringify({ 'LogicalName': entityname, "Page": page, "SearchValue": internalsearchvalue[0].Value, "WidgetId": widgetid, "AttributeLogicalName": AttributeLogicalName }),
                dataType: "json",
                async: true,
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var d = [];

                    var x = $("#" + id).dataTable();
                    BuildLookupDataAndColumn(data, id, d, entityname)
                    AddDataToExistingDataTable(d, x);
                    flag = "click";
                    x.fnPageChange(page - 1, true);
                    Metronic.unblockUI($(blocker));

                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    Metronic.unblockUI($(blocker));
                }
            });
        }
        else {
            $.ajax({
                url: "/Lookup/GetCrmData",
                type: "POST",
                data: JSON.stringify({ 'LogicalName': entityname, "Page": page, "WidgetId": widgetid, "AttributeLogicalName": AttributeLogicalName, "NavigationId": navigationid }),
                dataType: "json",
                async: true,
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var generalinsertdata = [];
                    var x = $("#" + id).dataTable();
                    var d = [];
                    BuildLookupDataAndColumn(data, id, d, entityname);
                    AddDataToExistingDataTable(d, x);
                    flag = "click";
                    x.fnPageChange(page - 1, true);
                    Metronic.unblockUI($(blocker));


                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    Metronic.unblockUI($(blocker));
                }
            });
        }


    }).dataTable();
}

function BindPageChangingOnCalcWidget(tableid, entityname, widgetid, guid) {
    
    
    var table = $("#" + tableid).dataTable();
    table.bind('page.dt', function (e, a, c) {
        var blocker = $("#" + tableid).closest('.portlet.light');
        Metronic.blockUI({
            overlayColor: 'none',
            animate: true,
            target: $(blocker)
        });

        //jquery.datatables.js 3452 override to pass custom parameter.
        if (a.customaction == "previous") {
            var ob = {};
            ob.Page = (a._iDisplayStart / a._iDisplayLength) + 2;
            ob.Id = $(this).attr("id");
            previous.push(ob);
            Metronic.unblockUI($("#" + tableid));
            return;
        }
        if (flagwidget != "") {
            flagwidget = "";
            Metronic.unblockUI($("#" + tableid));
            return false;
        }
        var id = $(this).attr("id");
        var page = (a._iDisplayStart / a._iDisplayLength) + 1;
        if (page == 2) {
            var blockUITop = $(".block-spinner-bar").parent().parent().css("top");
            $(".block-spinner-bar").parent().parent().css("top", parseInt(blockUITop) - 80);
        }
        var previousarray = $.grep(previous, function (element, index) {
            return element.Id == id
        });
        
        //daha önce previousa bastıysa
        if (previousarray.length != 0) {

            var highest = getHighest(previousarray);
            //previous yapıp tekrar next yaparsa fazladan data eklemeyi önlemek için
            if (highest.Page >= page) {
                Metronic.unblockUI($(blocker));
                return;
            }

        }
        var internalsearchvalue = $.grep(searchvalues, function (element, index) {
            return element.Id == tableid;
        });
        if (internalsearchvalue.length > 0 && internalsearchvalue[0].Value != "") {
            $.ajax({
                url: "/Page/GetSearchGridForModal",
                type: "POST",
                data: JSON.stringify({ 'CalculatedWidgetId': widgetid, 'WidgetGuid': guid, "PageNumber": page, "RecordCount": 10, "SearchValue": internalsearchvalue[0].Value }),
                dataType: "json",
                async: true,
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    var d = [];

                    var x = $("#" + id).dataTable();
                    BuildLookupDataAndColumn(JSON.parse(data.Content), id, d, entityname)
                    AddDataToExistingDataTable(d, x);
                    flagwidget = "click";
                    x.fnPageChange(page - 1, true);
                    Metronic.unblockUI($(blocker));
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    Metronic.unblockUI($(blocker));
                }
            });
        }
        else {
            $.ajax({
                url: "/Page/GetGridForModal",
                type: "POST",
                data: JSON.stringify({ 'CalculatedWidgetId': widgetid, 'WidgetGuid': guid, "PageNumber": page, "RecordCount": 10 }),
                dataType: "json",
                async: true,
                contentType: "application/json; charset=utf-8",
                success: function (data) {

                    var generalinsertdata = [];

                    //$("#" + id).DataTable().clear()
                    var x = $("#" + id).dataTable();
                    var d = [];
                    BuildLookupDataAndColumn(JSON.parse(data.Content), id, d, entityname)
                    AddDataToExistingDataTable(d, x)

                    flagwidget = "click";
                    x.fnPageChange(page - 1, true);
                    Metronic.unblockUI($(blocker));
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    Metronic.unblockUI($(blocker));
                }
            });
        }


    });
}

function MakeFieldsEmpty(attributes) {
    attributes.each(function () {
        if ($(this).attr("data-initial") != 1) {
            var type = $(this).attr("data-type");
            if (type == "lookup") {
                $(this).attr("data-id", "")
                $(this).val("")
            }
            else if (type == "boolean") {
                if ($(this).attr("type") == "radio") {
                    $(this).parent().removeClass("checked")
                    $(this).removeAttr("checked")
                }
                else if ($(this).attr("type") == "checkbox") {
                    $(this).parent().removeClass("checked")

                }
            } else if (type == "boolean") {
                $(this).val("null")
            }

            else {
                $(this).val("")
            }
        }


    });
}

function MakeLookup(clicker, datas, showornot) {
    previous = [];
    Metronic.blockUI({
        overlayColor: 'none',
        animate: true
    });
    Metronic.blockUI({
        target: $("#responsive_" + $(clicker).attr("data-widgetid")),
        animate: true,
        overlayColor: 'none',
    });
    //for linked widget
    var linkeds =  $("#responsiveLinkWidget_" + $(clicker).attr("data-widgetid")).find(".modal-content").first();
    Metronic.blockUI({
        target:  linkeds,
        animate: true,
        overlayColor: 'none',
    });
    
    //lookuplinks
    var lookuplinks = $(clicker).closest(".formwidgetclass");  
    if ($(lookuplinks).attr("data-iscomingfromlookup") != "") {
        Metronic.blockUI({
            target: lookuplinks
        });
    }
    var button = $(clicker);
    var modalid = "responsive_" + $(clicker).attr("data-widgetid");
    var entityname = $(clicker).attr("data-target");
    var counter = $(clicker).attr("data-count");
    var tableid = $(clicker).attr("data-target") + "_" + $(clicker).attr("data-count");
    var ismulti = $(clicker).attr("data-ismulti");
    var ismultitype = $(clicker).attr("data-ismultitype");
    if (showornot == true) {
        $("#responsive_" + tableid).find(".close").css("display", "none")
        $("#responsive_" + tableid).find(".modal-footer").find("button").attr("data-dismiss", "")
        $("#responsive_" + tableid).find(".modal-footer").find("button").bind("click", function () {
            $('#responsive_' + tableid).modal('hide');
        });
    }
    $.ajax({
        url: "/Lookup/GetCrmData",
        type: "POST",
        data: JSON.stringify({ 'LogicalName': entityname, "Page": 1, "WidgetId": $(clicker).attr("data-widgetid"), "AttributeLogicalName": $(clicker).parent().prev().attr("id"), "NavigationId": navigationid }),
        dataType: "json",
        async: true,
        contentType: "application/json; charset=utf-8",
        success: function (data) {

            //override scroller plugin to fit the lookup!
            $("#" + tableid).closest(".scroller").css("height", "auto")
            $("#" + tableid).closest(".slimScrollDiv").css("height", "auto")
            var d = [], columnsarray = [];
            var ifexist = $.fn.DataTable.isDataTable("#" + tableid);

            if (datas == undefined) {
                var obj = {};
                obj.Id = tableid;
                obj.data = data;
                lookupfirstpagedata.push(obj);

            }

            if (ifexist)
                $("#" + tableid).dataTable().fnDestroy();
            $('#' + tableid).empty();

            // #region build columns and data
            BuildLookupDataAndColumn(data, tableid, d, entityname);
            // #endregion
            var subtotalrecords = ""
            if (data.length > 0 && data[0].TotalRecord != null) {
                subtotalrecords = totalrecords.replace("_TOTAL_", data[0].TotalRecord);
            }
            var table = $("#" + tableid);

            var dtable = table.dataTable({
                "bStateSave": false, // save datatable state(pagination, sort, etc) in cookie.
                "data": d,
                "bLengthChange": false,
                // set the initial value
                "pageLength": 10,
                "pagingType": "simple",
                "order": [],
                "language": {
                    "sInfoEmpty": InfoEmpty,
                    "sEmptyTable": EmptyTable,
                    "sInfo": subtotalrecords == "" ? totalrecords : subtotalrecords,
                    "search": searchfordatatable,
                    "lengthMenu": "  _MENU_ records",
                    "paginate": {
                        "previous": prev,
                        "next": next,
                        "last": "Last",
                        "first": "First"
                    }
                },

                fnRowCallback: function (nRow, aData, iDisplayIndex, iDisplayIndexFull) {
                    var tid = $(this).closest('table').attr("id");
                    if ($(this).closest('table').attr("data-modaltype") == "lookup") {
                        var internalsearchvalue = $.grep(searchvalues, function (element, index) {
                            return element.Id == tid;
                        });
                        if (internalsearchvalue.length > 0) {
                            $("#" + tid + "_filter > label >input").val(internalsearchvalue[0].Value);

                        }
                    }
                    $(nRow).unbind("click");
                    $(nRow).on('click', function () {

                        var $tds = $(this).find("td");
                        var data = $("#" + $(this).closest('table').attr("id")).dataTable().fnGetData(this);
                        var input = $("#" + $(this).closest('table').attr("data-relatedfield"));
                        $(input).attr("data-id", data[data.length - 1]);
                        $(input).val(data[data.length - 2]);
                        var tid = $(input).attr("data-target") + "_" + $(input).attr("data-count");
                        $('#responsive_' + tid).modal('hide');
                        $(input).trigger('change');
                    });
                }
            });
            //this is for external pages.bind css in runtime
            $(".dataTable").css("font-size", "13px");

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
            Metronic.unblockUI();
            Metronic.unblockUI($("#" + tableid));
            Metronic.unblockUI($("#" + modalid));
            Metronic.unblockUI(lookuplinks);
            Metronic.unblockUI(linkeds);
            $("#" + tableid).unbind('page.dt');
            BindPageChangingOnLookup(tableid, entityname, JSON.parse(this.data).WidgetId, JSON.parse(this.data).AttributeLogicalName);

            $("#" + $(table).attr("id") + "_filter > label > input").unbind();

            $("#" + $(table).attr("id") + "_filter > label > input").bind('keyup', { tableid: tableid, WidgetId: JSON.parse(this.data).WidgetId, AttributeLogicalName: JSON.parse(this.data).AttributeLogicalName }, function (e) {              

                if (e.keyCode == 13) {
                    if (this.value.length < 3 && this.value != "") {

                        toastr["error"](SearchCriteria, ErrorMessageHeader)
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

                    var searchvaluesobj = {};
                    searchvaluesobj.Id = tableid;
                    searchvaluesobj.Value = this.value;

                    searchvalues = [];
                    searchvalues.push(searchvaluesobj);

                    if (this.value == "") {
                        $("#" + e.data.tableid).dataTable().fnClearTable();
                        previous = [];
                        flag = "";
                        var griddata = $.grep(lookupfirstpagedata, function (element, index) {
                            return element.Id == tableid;
                        });
                        var d = [];
                        var x = $("#" + tableid).dataTable();
                        BuildLookupDataAndColumn(data, tableid, d, entityname)
                        AddDataToExistingDataTable(d, x);
                        Metronic.unblockUI($("#" + tableid));
                        toastr["success"](SearchMessage, SuccessMessageHeader)
                        return;
                    }
                    $.ajax({
                        url: "/Lookup/GetSearchCrmData",
                        type: "POST",
                        data: JSON.stringify({ 'LogicalName': entityname, "Page": 1, "SearchValue": this.value, "WidgetId": e.data.WidgetId, "AttributeLogicalName": e.data.AttributeLogicalName }),
                        dataType: "json",
                        async: true,
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            $("#" + e.data.tableid).dataTable().fnClearTable();
                            var d = [];
                            var x = $("#" + tableid).dataTable();
                            BuildLookupDataAndColumn(data, tableid, d, entityname)
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

            if (ismulti != "") {
                if (ismultitype == "customer") {
                    var div = $("#" + tableid + "_filter").parent().prev()
                    var sdiv = $("<div class='col-md-6'>").appendTo($(div))
                    var left = $("<div class='btn-group pull-left'>").appendTo($(sdiv));
                    var s = $("<select class='form-control multier'>").appendTo($(left));

                    $(s).change(function () {
                        $(button).attr("data-target", $(this).val())

                        $("#" + tableid).attr("id", $(this).val() + "_" + counter);
                        $("#" + $("#" + $(this).val() + "_" + counter).attr("data-relatedfield")).attr("data-target", $(this).val());
                        $('#responsive_' + tableid).attr("id", "responsive_" + $(this).val() + "_" + counter)
                        selectedvalue = $(this).val();
                        $(button).trigger("click", [{ logicalname: $(this).val(), ismulti: "yes" }])
                    });
                    for (var k = 0; k < customerEntity.length; k++) {
                        var o = $("<option>").appendTo($(s));
                        $(o).text(customerEntity[k]);
                        $(o).val(customerEntity[k].toLowerCase());
                    }
                    //for initial value is contact set the combobox element contact
                    if ($(button).attr("data-initiallogicalname") != undefined) {
                        $(s).val($(button).attr("data-initiallogicalname"));
                        $(button).attr("data-initiallogicalname", undefined);
                    }
                    else if ($(button).attr("data-target") != undefined) {
                        selectedvalue = $(button).attr("data-target");
                    }
                    if (selectedvalue != "") {
                        $(s).val(selectedvalue);
                        selectedvalue = "";
                    }
                }
            }
            if (datas == undefined)
                $('#responsive_' + tableid).modal('toggle');


            $('#responsive_' + tableid).on('shown.bs.modal', function () {
                $(this).find("input").focus();
            })

            $('#responsive_' + tableid).on('hidden.bs.modal', function (e) {
                var t = $(this).attr("id").replace("responsive_", "");
                $("#" + t).dataTable().fnDestroy();
            })
            $('#responsive_' + tableid).find("input").focus();
            $("#" + tableid + "_filter >label> input").val("");
            searchvalues = [];

        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            Metronic.unblockUI();
            Metronic.unblockUI($("#" + tableid));
            Metronic.unblockUI($("#" + modalid));
            Metronic.unblockUI(lookuplinks);
        }
    });

   

}

function UrlFieldsFunctions() {
    $(".urlclicker").bind("mouseenter", function () {
        $(this).css("cursor", "pointer")

    });

    $(".urlclicker").bind("click", function () {
        $(this).select();

    });

    $(".urlclicker").bind("dblclick", function () {
        window.open($(this).val(), "_blank");
    });

    $(".urlclicker").bind("mouseleave", function () {
        $(this).css("cursor", "")

    });

    $(".urlclicker").on("change", function () {
        var url = "";
        var urlcheck = $(this).val().indexOf("http");
        if (urlcheck != -1) {
            url = $(this).val();
        }
        else {
            url = "http://" + $(this).val();
        }

    });

}

function convertToUtc(str) {
    var date = new Date(str);
    var year = date.getUTCFullYear();
    var month = date.getUTCMonth() + 1;
    var dd = dategetUTCDate();
    var hh = date.getUTCHours();
    var mi = date.getUTCMinutes();
    var sec = date.getUTCSeconds();

    // 2010-11-12T13:14:15Z

    theDate = year + "-" + (month[1] ? month : "0" + month[0]) + "-" +
              (dd[1] ? dd : "0" + dd[0]);
    theTime = (hh[1] ? hh : "0" + hh[0]) + ":" + (mi[1] ? mi : "0" + mi[0]);
    return [theDate, theTime].join("T");
}

function BindNotesUpdate(obj, id) {

    if ($(obj).parent().parent().find(".notesdownloader").attr("data-src") == undefined) {
        toastr["error"](NoFile, ErrorMessageHeader)
        return false
    }
    Metronic.blockUI();
    //first send attachment       
    var Attachment = {};
    Attachment.MimeType = $("#frame_" + $(obj).attr("data-count")).attr("data-mimetype");
    Attachment.DocumentBody = $("#frame_" + $(obj).attr("data-count")).attr("data-src").split(",")[1];
    Attachment.FileName = $("#frame_" + $(obj).attr("data-count")).attr("data-name");
    Attachment.Subject = $("#frame_" + $(obj).attr("data-count")).attr("data-subject");
    Attachment.EntityName = $(obj).closest('.portlet.light').find('.panel-body').attr("data-entityname");

    //if ($(obj).closest('.portlet.light').attr("data-personel") != 1) {
    //    var dataArr = $.grep(dataarray, function (element, index) {
    //        return element.Id == $(obj).closest('.portlet.light').attr("data-widgetid");
    //    });
    //    Attachment.RecordId = dataArr[0].EntityId;
    //}
    //else {
    //    Attachment.RecordId = dataarray[0].EntityId;
    //}
    Attachment.RecordId = id;
    $.ajax({
        url: "/Page/InsertAttachment",
        type: "POST",
        data: JSON.stringify({ 'Attachment': JSON.stringify(Attachment) }),
        dataType: "json",
        async: true,
        contentType: "application/json; charset=utf-8",
        beforeSend: function (XMLHttpRequest) {
            XMLHttpRequest.setRequestHeader("Accept", "application/json");
        },
        success: function (data) {
            var d = JSON.parse(data.Content);
            if (d.Status == "error") {
                toastr["error"](d.Content, ErrorMessageHeader)
                Metronic.unblockUI();
            }
            else {
                Metronic.unblockUI();
                toastr["success"](AddAttachment, SuccessMessageHeader);
                //hide browse and attach button 
                $(obj).parent().css("display", "none")
                $(obj).parent().prev().css("display", "none")
                //show delete  button
                $(obj).parent().next().css("display", "")
                $(obj).closest(".notediv").attr("data-canbedeleted", "1")
                $(obj).closest(".notediv").attr("data-noteid", d.Content)
            }
        },

        error: function (XMLHttpRequest, textStatus, errorThrown) {
            Metronic.unblockUI();
            toastr["error"]("Error during creating the attachment", "Error")

        }
    });

}

function DeleteNotes(obj) {
    if ($(obj).closest(".notediv").attr("data-canbedeleted") != "1") {
        toastr["error"](NoFileForDelete, ErrorMessageHeader)
        return false
    }
    //closest form-group
    Metronic.blockUI();
    $.ajax({
        url: "/Page/DeleteAttachment",
        type: "POST",
        data: JSON.stringify({ 'AttachmentId': $(obj).closest(".notediv").attr("data-noteid") }),
        dataType: "json",
        async: true,
        contentType: "application/json; charset=utf-8",
        beforeSend: function (XMLHttpRequest) {
            XMLHttpRequest.setRequestHeader("Accept", "application/json");
        },
        success: function (data) {
            var d = JSON.parse(data.Content);
            if (d != "") {
                toastr["error"](d, ErrorMessageHeader)
                Metronic.unblockUI();
            }
            else {
                Metronic.unblockUI();
                toastr["success"](DeleteAttachment, SuccessMessageHeader);
                $(obj).closest(".notediv").empty();
            }
        },

        error: function (XMLHttpRequest, textStatus, errorThrown) {
            Metronic.unblockUI();
            toastr["error"]("Error during creating the attachment", "Error")

        }
    });
}

function AddNotesToPage(obj) {
    //then add another notes control
    var idarr = [];

    $(".notediv").each(function () {
        idarr.push($(this).attr("data-count"))
    });
    var maxid = Math.max.apply(Math, idarr); // 3
    var button = $(obj)
    var id = parseInt(maxid) + 1

    $.ajax({
        url: "/Page/GetNotes",
        type: "POST",
        data: {
            Id: id,
            LogicalName: $(obj).parent().prev().prev().attr("name"),
            Type: $(obj).parent().prev().prev().attr("data-type")

        },
        dataType: "html",
        async: true,
        success: function (result) {
            var appendObject = $(button).closest(".notediv").parent()

            var d = $("<div>").appendTo($(appendObject))
            $(d).css("margin-bottom", "10px")
            $(d).addClass("notediv")
            $(d).attr("data-count", id)

            $(result).appendTo($(d))

            var button1 = $(appendObject).find(".notesupdater[data-count=" + id + "]");

            $("#" + $(button1).attr("id")).unbind();
            $("#" + $(button1).attr("id")).on("click", function () {
                BindNotesUpdate($(this), $(this).closest(".formwidgetclass").attr("data-entityid"));
            });

            var button2 = $(appendObject).find(".newer[data-count=" + id + "]")

            $("#" + $(button2).attr("id")).on("click", function () {
                AddNotesToPage($(this))
            });

            var button3 = $(appendObject).find(".deleter[data-count=" + id + "]")
            $("#" + $(button3).attr("id")).on("click", function () {
                DeleteNotes($(this))
            });
            var notesbrowser = $(button1).parent().prev().find(".notesbrowser")

            $(notesbrowser).on("change", function (handleFileSelect) {
                BrowseNote($(this), handleFileSelect, $(this).attr("data-count"))
            })
            Metronic.unblockUI();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            Metronic.unblockUI();
        }
    });
}

function BrowseNote(obj, fileobject, id) {
    var files = fileobject.target.files;
    var anchorelem = $("#frame_" + id).prev();
    $(anchorelem).text(files[0].name);


    $(anchorelem).attr("data-mimetype", files[0].type);
    $(anchorelem).attr("data-name", files[0].name);
    $(anchorelem).attr("data-subject", $(obj).parent().parent().prev().val());
    $(anchorelem).attr("data-filename", files[0].name);

    $("#frame_" + id).attr("data-mimetype", files[0].type);
    $("#frame_" + id).attr("data-name", files[0].name);
    $("#frame_" + id).attr("data-subject", $(obj).parent().parent().prev().val());
    var reader = new FileReader();

    var str = reader.readAsDataURL(files[0]);
    reader.onloadend = function () {
        $("#frame_" + id).attr("data-src", $(this).attr("result"));
        $(anchorelem).attr("data-src", $(this).attr("result"));
        $(anchorelem).on("click", function () {
            DownloadData($(this).attr("data-mimetype"), $(this).attr("data-src").split(",")[1], $(this).attr("data-filename"));
        })
    }

}

function DownloadData(mimetype, base64src, filename) {

    download("data:" + mimetype + ";base64," + base64src, filename, mimetype);
}

function CreateRecord(ButtonElement, IsSubGrid) {
    var br = BRValidation();
    var r = Validation($(ButtonElement), IsSubGrid);
    if (r != "" || br != "") {
        return;
    }

    //for signature part
    if ($("#signature-pad").length > 0) {

        if (SpadParamGeneral.isEmpty()) {
            alert("Please provide signature first.");
            return false;
        } else {
            signature = SpadParamGeneral.toDataURL();
        }
    }
    var smodalid = $(ButtonElement).closest('.portlet.light').attr("data-widgetid");


    if ($(ButtonElement).closest('.portlet.light').attr("data-createdid") == undefined) {
        Metronic.blockUI();
        Metronic.blockUI({
            target: $("#responsive_" + smodalid)
        });

        var senderarray = [];
        var entityname = "";

        var form = $(ButtonElement).closest('.portlet.light')
        var attributes = $(ButtonElement).closest('.portlet.light').find("[data-attribute=yes]");
        var redirect = $(ButtonElement).closest('.portlet.light').attr("data-isredirect")

        $(ButtonElement).closest('.portlet.light').find("[data-attribute=yes]").each(function () {
            var senderobject = {};
            entityname = $(this).closest('.panel-body').attr("data-entityname");
            $(this).attr("name", $(this).attr("name").replace("subgrid_", ""));
            $(this).attr("name", $(this).attr("name").replace("bpf_", ""));

            var type = $(this).attr("data-type");
            if (type == "string") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderarray.push(senderobject);
                }
            }
            else if (type == "lookup") {
                if ($(this).attr("data-id") != "" && $(this).attr("data-id") != undefined) {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).attr("data-id");
                    senderobject.entityname = $(this).attr("data-target");
                    senderarray.push(senderobject);
                }
            }
            else if (type == "customer") {
                if ($(this).attr("data-id") != "" && $(this).attr("data-id") != undefined) {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).attr("data-id");
                    senderobject.entityname = $(this).attr("data-target");
                    senderarray.push(senderobject);
                }
            }
            else if (type == "ınteger" || type == "integer") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderarray.push(senderobject);
                }
            }
            else if (type == "money") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderarray.push(senderobject);
                }
            }
            else if (type == "decimal") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderarray.push(senderobject);
                }
            }
            else if (type == "double") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderarray.push(senderobject);
                }
            }
            else if ($(this).attr("data-type") == "metronicdatetime") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = "datetime";
                    senderobject.value = $(this).val();
                    senderobject.datetimepicker = $(this).attr("data-timepicker");
                    senderobject.dateformat = $(this).attr("data-beforedateformat");
                    senderobject.timeformat = $(this).attr("data-beforetimeformat");
                    senderarray.push(senderobject);
                }

            }
            else if ($(this).attr("data-type") == "metronicdate") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = "datetime";
                    senderobject.value = $(this).val();
                    senderobject.datetimepicker = $(this).attr("data-timepicker");
                    senderobject.dateformat = $(this).attr("data-beforedateformat");
                    senderobject.timeformat = $(this).attr("data-beforetimeformat");
                    senderarray.push(senderobject);
                }

            }
            else if (type == "datetime") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderobject.datetimepicker = $(this).attr("data-timepicker");
                    senderobject.dateformat = $(this).attr("data-beforedateformat");
                    senderobject.timeformat = $(this).attr("data-beforetimeformat");
                    senderarray.push(senderobject);
                }
            }
            else if (type == "picklist") {
                if ($(this).val() != "null" && $(this).val() != null) {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).find(":selected").val();
                    senderarray.push(senderobject);
                }
            }
            else if (type == "memo") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderarray.push(senderobject);
                }
            }
            else if (type == "boolean") {
                if ($(this).attr("type") == "radio") {
                    if ($(this).attr("checked") == "checked") {
                        senderobject.logicalname = $(this).attr("name");
                        senderobject.type = type;
                        senderobject.value = $(this).val();
                        senderarray.push(senderobject);
                    }
                }
                else if ($(this).attr("type") == "checkbox") {

                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).parent().hasClass("checked") == true ? "1" : "0";
                    senderarray.push(senderobject);
                }

            }
            else if (type == "guid") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderarray.push(senderobject);
                }
            }
        });


        $.ajax({
            url: "/Page/SendFormDataToCrm",
            type: "POST",
            data: JSON.stringify({ 'FormData': JSON.stringify(senderarray), 'EntityName': entityname, 'Signature': signature, 'Ownership': $(ButtonElement).closest('.portlet.light').attr("data-ownership").trim(), 'RelationShipName': $(ButtonElement).closest('.portlet.light').attr("data-relationshipname"), 'ParentId': $(ButtonElement).closest('.portlet.light').attr("data-parentid") }),
            dataType: "json",
            async: true,
            contentType: "application/json; charset=utf-8",
            beforeSend: function (XMLHttpRequest) {
                XMLHttpRequest.setRequestHeader("Accept", "application/json");
            },
            success: function (data) {
                if (data.ErrorMessage != "") {
                    toastr["error"](data.ErrorMessage, ErrorMessageHeader)
                    Metronic.unblockUI();
                    Metronic.unblockUI($("#responsive_" + smodalid));
                }
                else {
                    var entityid = data.Id;
                    if (redirect == 0) {
                        MakeFieldsEmpty(attributes);
                        //add created data into datatable
                        if (IsSubGrid == true) {
                            var subgridid = $(ButtonElement).closest('.portlet.light').attr("data-subgridid");
                            //find the table
                            var t = $("body").find("table[data-subgridid='" + subgridid + "']")

                            var subgridcolumnsarr = $(t).find("th")
                            var columsArr = [];
                            subgridcolumnsarr.each(function () {
                                if ($(this).attr("data-logicalname") != "id")
                                    columsArr.push($(this).attr("data-logicalname"))

                            });

                            $.ajax({
                                url: "/Page/GetCreatedSubGriData",
                                type: "POST",
                                data: JSON.stringify({ 'EntityName': entityname, 'EntityId': entityid, 'Columns': JSON.stringify(columsArr) }),
                                dataType: "json",
                                async: true,
                                contentType: "application/json; charset=utf-8",
                                beforeSend: function (XMLHttpRequest) {
                                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                                },
                                success: function (data) {
                                    var r = JSON.parse(data.Content)
                                    var a = []
                                    for (var i = 0; i < r.length; i++) {
                                        a.push(r[i].Value);
                                    }
                                    a.push(entityid);
                                    $(t).dataTable().fnAddData(a);

                                },

                                error: function (XMLHttpRequest, textStatus, errorThrown) {

                                }
                            })
                        }
                    }
                    else {
                        var submitter = $(".submitter").text(UpdateButton);
                        $(submitter).removeClass("submitter");
                        $(submitter).addClass("updater");
                        $(submitter).on("click", function () {
                            UpdateRecord($(this));
                        });
                        //change datatable empty message 

                        $('table[data-type=subgrid]').find(".dataTables_empty").text(EmptyTable);

                        $(form).attr("data-createdid", entityid);

                        var subgrids = $('table[data-type=subgrid]');

                        var counter = 1;
                        subgrids.each(function () {
                            AddSubGridButton($(this), counter);

                            var table = $(this);
                            Metronic.blockUI();
                            $("#newsubgridbutton_" + counter).on("click", function () {

                                if ($(table).attr("data-newformid") == "" || $(table).attr("data-newformid") == undefined) {
                                    toastr["error"](SubGridCreateForm, ErrorMessageHeader);
                                    return;
                                }

                                BindSubgridCreateButton(table, entityid);

                            });
                            counter = counter + 1;

                        });
                        $(".dataTables_filter > label").css("font-size", "12px")

                    }
                    Metronic.unblockUI();
                    Metronic.unblockUI($("#responsive_" + smodalid));
                    toastr["success"](CreateMessage, SuccessMessageHeader);
                }


            },

            error: function (XMLHttpRequest, textStatus, errorThrown) {
                Metronic.unblockUI();
                Metronic.unblockUI($("#responsive_" + smodalid));
                toastr.options = {
                    "closeButton": true,
                    "debug": false,
                    "positionClass": "toast-top-right",
                    "onclick": null
                }
                toastr["error"](ErrorCreateMessage, ErrorMessageHeader)


                return false;

            }
        });
    }
}

function UpdateRecord(ButtonElement, IsSubGrid, ParamDataId) {

    var br = BRValidation();

    var r = Validation($(ButtonElement), IsSubGrid);
    if (r != "" || br != "") {
        return;
    }
    var smodalid = $(ButtonElement).closest('.portlet.light').attr("data-widgetid");
    var lookuplink = $(ButtonElement).closest(".formwidgetclass");

    Metronic.blockUI();
    Metronic.blockUI({
        target: $("#responsive_" + smodalid)
    });
    if ($(lookuplink).attr("data-iscomingfromlookup") != "") {
        Metronic.blockUI({
            target: $(lookuplink)
        });
    }
    var senderarray = [];
    var entityname = "";


    $(ButtonElement).closest('.portlet.light').find("[data-attribute=yes]").each(function () {
        var senderobject = {};
        entityname = $(this).closest('.panel-body').attr("data-entityname");
        if (IsSubGrid == undefined) {
            if ($(this).attr("data-subgrid") == "true") {
                return;

            }
        }
        var type = $(this).attr("data-type");
        if (type == "string") {

            if ($(this).attr("data-formatted") == 1) {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = $(this).val().replace("(", "").replace(")", "").replace(/ /g, '');
                senderarray.push(senderobject);
            }
            else {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = $(this).val();
                senderarray.push(senderobject);
            }

        }
        else if (type == "lookup") {
            if ($(this).attr("data-id") != "" && $(this).attr("data-id") != undefined) {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = $(this).attr("data-id");
                senderobject.entityname = $(this).attr("data-target");

                senderarray.push(senderobject);
            }
        }
        else if (type == "customer") {
            if ($(this).attr("data-id") != "" && $(this).attr("data-id") != undefined) {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = $(this).attr("data-id");
                senderobject.entityname = $(this).attr("data-target");
                senderarray.push(senderobject);
            }
        }
        else if (type == "ınteger" || type == "integer") {

            senderobject.logicalname = $(this).attr("name");
            senderobject.type = type;
            senderobject.value = $(this).val();
            senderarray.push(senderobject);

        }
        else if (type == "money") {

            senderobject.logicalname = $(this).attr("name");
            senderobject.type = type;
            senderobject.value = $(this).val();
            senderarray.push(senderobject);

        }
        else if (type == "decimal") {

            senderobject.logicalname = $(this).attr("name");
            senderobject.type = type;
            senderobject.value = $(this).val();
            senderarray.push(senderobject);

        }
        else if (type == "double") {

            senderobject.logicalname = $(this).attr("name");
            senderobject.type = type;
            senderobject.value = $(this).val();
            senderarray.push(senderobject);

        }
        else if ($(this).attr("data-type") == "metronicdatetime") {
            if ($(this).val() != "") {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = "datetime";
                senderobject.value = $(this).val();
                senderobject.datetimepicker = $(this).attr("data-timepicker");
                senderobject.dateformat = $(this).attr("data-beforedateformat");
                senderobject.timeformat = $(this).attr("data-beforetimeformat");
                senderarray.push(senderobject);
            }

        }
        else if ($(this).attr("data-type") == "metronicdate") {
            if ($(this).val() != "") {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = "datetime";
                senderobject.value = $(this).val();
                senderobject.datetimepicker = $(this).attr("data-timepicker");
                senderobject.dateformat = $(this).attr("data-beforedateformat");
                senderobject.timeformat = $(this).attr("data-beforetimeformat");
                senderarray.push(senderobject);
            }

        }
        else if (type == "datetime") {
            if ($(this).val() != "") {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = $(this).val();
                senderobject.datetimepicker = $(this).attr("data-timepicker");
                senderobject.dateformat = $(this).attr("data-beforedateformat");
                senderobject.timeformat = $(this).attr("data-beforetimeformat");
                senderarray.push(senderobject);
            }
        }
        else if (type == "picklist") {
            if ($(this).val() != null) {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = $(this).find(":selected").val();
                senderarray.push(senderobject);
            }
            else {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = null;
                senderarray.push(senderobject);

            }
        }
        else if (type == "memo") {

            senderobject.logicalname = $(this).attr("name");
            senderobject.type = type;
            senderobject.value = $(this).val();
            senderarray.push(senderobject);

        }
        else if (type == "boolean") {
            if ($(this).attr("type") == "radio") {
                if ($(this).parent().hasClass("checked") == true) {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderobject.twooptiontype = $(this).attr("type");
                    senderarray.push(senderobject);
                }
            }
            if ($(this).attr("type") == "checkbox") {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = $(this).parent().hasClass("checked") == true ? "1" : "0";
                senderobject.twooptiontype = $(this).attr("type");
                senderarray.push(senderobject);

            }
        } else if (type == "guid") {

            senderobject.logicalname = $(this).attr("name");
            senderobject.type = type;
            senderobject.value = $(this).val();
            senderarray.push(senderobject);

        }
    });

    // Clean unneeded array obj
    senderarray = CheckOldControl(senderarray);

    var datawidgetId = $(ButtonElement).closest('.portlet.light').attr("data-widgetid");

    var DataID = ParamDataId;

    $.ajax({
        url: "/Page/UpdateFormDataToCrm",
        type: "POST",
        data: JSON.stringify({ 'FormData': JSON.stringify(senderarray), 'EntityName': entityname, 'Id': DataID, 'Ownership': $(ButtonElement).closest('.portlet.light').attr("data-ownership").trim() }),
        dataType: "json",
        async: true,
        contentType: "application/json; charset=utf-8",
        beforeSend: function (XMLHttpRequest) {
            XMLHttpRequest.setRequestHeader("Accept", "application/json");
        },
        success: function (data) {
            if (data != "") {
                toastr.options = {
                    "closeButton": true,
                    "debug": false,
                    "positionClass": "toast-top-right",
                    "onclick": null
                }
                toastr["error"](data, ErrorMessageHeader)
                Metronic.unblockUI();
                Metronic.unblockUI($("#responsive_" + smodalid));
                Metronic.unblockUI($(lookuplink));
            }
            else {
                if (IsSubGrid == true) {
                    //update the table
                    if (TempSubGridTableId != "") {
                        var t = $("body").find("table[data-subgridid='" + TempSubGridTableId + "']")

                        var subgridcolumnsarr = $(t).find("th")

                        var WillUpdateArr = [];

                        subgridcolumnsarr.each(function () {
                            if ($(this).attr("data-logicalname") != "id") {
                                var thLogicalName = $(this).attr("data-logicalname")
                                var updatedrecordarr = $.grep(senderarray, function (element, index) {
                                    return element.logicalname == thLogicalName;
                                });
                                if (updatedrecordarr.length > 0) {
                                    if (updatedrecordarr[0].type == "lookup" || updatedrecordarr[0].type == "customer") {
                                        var v = $("input[name='" + updatedrecordarr[0].logicalname + "']").val();
                                        WillUpdateArr.push(v);
                                    }
                                    else if (updatedrecordarr[0].type == "picklist") {
                                        var v = $("select[name='" + updatedrecordarr[0].logicalname + "']").find(":selected").text();
                                        WillUpdateArr.push(v);
                                    }
                                    else if (updatedrecordarr[0].type == "boolean") {
                                        if (updatedrecordarr[0].twooptiontype == "checkbox") {
                                            var v = $("input[name='" + updatedrecordarr[0].logicalname + "']").parent().hasClass("checked") == true ? "True" : "False";
                                            WillUpdateArr.push(v);
                                        }
                                        else if (updatedrecordarr[0].twooptiontype == "radio") {
                                            var v = $("input[name='" + updatedrecordarr[0].logicalname + "']").parent().hasClass("checked") == true ? "True" : "False";
                                            WillUpdateArr.push(v);
                                        }
                                    }
                                    else {
                                        WillUpdateArr.push(updatedrecordarr[0].value);
                                    }
                                }
                                else {
                                    WillUpdateArr.push("");
                                }
                            }
                        });
                        WillUpdateArr.push(SubGridDetailId);

                        t.dataTable().fnUpdate(WillUpdateArr, $(t).attr("data-clickindex")); // Row
                    }
                }
                Metronic.unblockUI();
                Metronic.unblockUI($("#responsive_" + smodalid));
                Metronic.unblockUI($(lookuplink));

                toastr.options = {
                    "closeButton": true,
                    "debug": false,
                    "positionClass": "toast-top-right",
                    "onclick": null
                }
                toastr["success"](UpdateMessage, SuccessMessageHeader);
                GetInitializeFormValuesForArray();
            }
        },

        error: function (XMLHttpRequest, textStatus, errorThrown) {
            Metronic.unblockUI();
            Metronic.unblockUI($("#responsive_" + smodalid));
            Metronic.unblockUI($(lookuplink));
            toastr.options = {
                "closeButton": true,
                "debug": false,
                "positionClass": "toast-top-right",
                "onclick": null
            }
            if (XMLHttpRequest.responseText.indexOf("<title>") != -1) {
                toastr["error"](GetMeaningfulErrorMessage(XMLHttpRequest.responseText), ErrorMessageHeader);
            }
            else {
                toastr["error"](UpdateMessageError, ErrorMessageHeader);
            }
            return false;
        }
    });
}

function BindDataToEditForm(id, type) {
    if (dataarray.length > 0) {

        var temparr = $.grep(dataarray, function (element, index) {
            return element.EntityId == id;
        });
        var arr1 = $.map(temparr, function (o) { return o.DateTime; });
        var maxDate = Math.max.apply(null, arr1)
        temparr = $.grep(temparr, function (element, index) {
            return element.DateTime == maxDate;
        });

        for (var i = 0; i < temparr[0].data.length; i++) {
            var d = temparr[0].data[i];

            $('[data-attribute=yes]').each(function (element, index) {
                if ($(this).attr("name") == d.LogicalName) {
                    if ($(this).attr("data-type") == "string") {

                        if ($(this).attr("data-isurl") == "true") {
                            var url = "";
                            if (d.Value.indexOf("http") == -1) {
                                url = "http://" + d.Value;
                            }
                            else {
                                url = d.Value;
                            }
                            $(this).val(url);
                            $(this).parent().attr("href", url);
                            $(this).parent().attr("target", "_blank");
                        }
                        else {
                            $(this).val(d.Value);
                        }
                    }
                    else if ($(this).attr("data-type") == "lookup") {
                        $(this).val(d.LookUpValueName);
                        $(this).attr("data-id", d.Value);
                    }
                    else if ($(this).attr("data-type") == "customer") {
                        $(this).val(d.LookUpValueName);
                        $(this).attr("data-id", d.Value);
                        $(this).attr("data-target", d.LookupLogicalName);
                    }
                    else if ($(this).attr("data-type") == "ınteger" || $(this).attr("data-type") == "integer") {
                        $(this).val(d.Value)
                    }
                    else if ($(this).attr("data-type") == "money") {
                        $(this).val(d.Value.replace(",", "."));
                    }
                    else if ($(this).attr("data-type") == "decimal") {
                        $(this).val(d.Value.replace(",", "."));
                    }
                    else if ($(this).attr("data-type") == "double") {
                        $(this).val(parseFloat(d.Value.replace(",", ".")))
                    }
                    else if ($(this).attr("data-type") == "metronicdatetime") {
                        if (d.Value != "")
                            $(this).val(d.Value)

                    }
                    else if ($(this).attr("data-type") == "metronicdate") {
                        if (d.Value != "") {
                            var x = new Date(d.Value);
                            if ($(this).hasClass('datetimefield')) {
                                $(this).datepicker('update', new Date(x.getFullYear(), x.getMonth(), x.getDate()));
                            } else {
                                $(this).parent().datepicker('update', new Date(x.getFullYear(), x.getMonth(), x.getDate()));
                            }

                            //$(this).val(d.Value)
                        }
                    }
                    else if ($(this).attr("data-type") == "datetime") {

                        var timepart = $(this).attr("data-timepicker");

                        var dateformat = $(this).attr("data-dateformat")
                        var timeformat = $(this).attr("data-timeformat")
                        $(this).datetimepicker({
                            timepicker: timepart == "dateandtime" ? true : false,
                            format: timepart == "dateandtime" ? dateformat + " " + timeformat : dateformat,
                            step: 60,
                            value: d.Value
                        });

                    }
                    else if ($(this).attr("data-type") == "picklist") {
                        $(this).val(d.Value)
                    }
                    else if ($(this).attr("data-type") == "status") {
                        $(this).val(d.Value)
                    }
                    else if ($(this).attr("data-type") == "memo") {
                        $(this).val(d.Value)
                    }
                    else if ($(this).attr("data-type") == "boolean") {

                        var v = d.Value == "False" ? 0 : d.Value == "True" ? 1 : null;
                        if ($(this).val() == v)
                            $(this).parent().addClass("checked")
                        else
                            $(this).parent().removeClass("checked")

                        // For BPF Select option
                        if ($(this).prop('type') == 'select-one') {
                            $(this).val(v);
                        }

                    }
                }
            })

        }
        PrepareEditFormStage();
        GetInitializeFormValuesForArray();

    }
}

function AddSubGridButton(subgridtable, counter) {

    var html = "<div class='btn-group' style='float:right'>";
    html += "<button id=newsubgridbutton_" + counter + " class='btn btn-icon-only blue subgridadder'>"
    html += "<i class='fa fa-plus'></i>"
    html += " </button>"
    html += "</div>";
    $(html).appendTo($("#" + $(subgridtable).attr("id")).parent().parent().parent().prev());
}

function BindSubgridCreateButton(table, entityid) {

    Metronic.blockUI()
    $.ajax({
        url: "/Page/GetCreateFormForSubGrid",
        type: "GET",
        data: {
            FormId: $(table).attr("data-newformid"),
            WidgetGuid: $(table).attr("data-widgetguid"),
            WidgetId: $(table).attr("data-widgetid"),
            PageWidgetId: $(table).attr("data-pagewidgetid"),
            RelationShipName: $(table).attr("data-relationshipname"),
            ParentId: entityid,
            SubGridId: $(table).attr("data-subgridid")
        },
        dataType: "html",
        async: true,
        success: function (result) {
            $("#" + $(table).attr("id")).closest(".scroller").css("height", "auto");
            $("#" + $(table).attr("id")).closest(".slimScrollDiv").css("height", "auto");
            $("#responsive_" + $(table).attr("data-pagewidgetid")).html(result);
            $("#responsive_main_" + $(table).attr("data-pagewidgetid")).modal('toggle');
            Metronic.unblockUI();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            Metronic.unblockUI();
        }
    });
}

function RenderSubGrids(IscomingFromGrid) {
    var subgrid = $('table[data-type=subgrid]');

    var subgridcounter = 1;
    subgrid.each(function () {
        var subgridtable = $(this);
        var block = $(subgridtable).closest('.form-group');

        Metronic.blockUI({
            target: $(block)
        });

        $.ajax({
            url: "/Lookup/GetSubGridData",
            type: "POST",
            data: JSON.stringify({ 'ViewId': $(subgridtable).attr("data-viewid") }),
            dataType: "json",
            async: true,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                var vid = JSON.parse(this.data).ViewId.toString().replace('{', '').replace('}', '');
                var t = $('table[data-viewid=' + vid + ']');

                var colArr = []
                for (var i = 0; i < data.length; i++) {
                    var colObj = {};
                    var c = data[i].DisplayName;
                    colObj.title = c;
                    colArr.push(colObj);

                }
                var colObj = {}
                colObj.title = "Id";
                colArr.push(colObj);

                var DataTable = MakeDataTableForSubGrid(t, 1, colArr);


                var tharr = $(t).find("th")
                for (var i = 0; i < tharr.length; i++) {
                    $(tharr[i]).attr("data-logicalname", data[i].ColumnName);
                }

                //if it is update form
                if ($(subgridtable).attr("data-update") == "1") {
                    //get the parent id
                    var dArr = $.grep(dataarray, function (element, index) {
                        return element.Id == $(subgridtable).attr("data-pagewidgetid");
                    });
                    var DataID = querystringarray["DataId"] == undefined ? dArr[0].EntityId : querystringarray["DataId"];

                    if (IscomingFromGrid == undefined) {
                        if ($(subgridtable).attr("data-editable") == "true") {

                            AddSubGridButton($(subgridtable), subgridcounter);

                            $("#newsubgridbutton_" + subgridcounter).on("click", function () {
                                if ($(subgridtable).attr("data-newformid") == "" || $(subgridtable).attr("data-newformid") == undefined) {
                                    toastr["error"](SubGridCreateForm, ErrorMessageHeader);
                                    return;
                                }
                                BindSubgridCreateButton(subgridtable, DataID);

                            });
                        }
                    }
                    subgridcounter = subgridcounter + 1;

                    //change empty message
                    $(subgridtable).find(".dataTables_empty").text(EmptyTable);

                    //Add SubGridData part
                    Metronic.blockUI({
                        target: $(subgridtable)
                    });

                    $.ajax({
                        url: "/Page/GetAllRelatedSubGridRecords",
                        type: "POST",
                        data: JSON.stringify({ 'SubGridViewId': $(subgridtable).attr("data-viewid"), 'RelationShipName': $(subgridtable).attr("data-relationshipname"), 'ParentId': DataID }),
                        dataType: "json",
                        async: true,
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {

                            var generalinsertdata = [];
                            var x = $(subgridtable).dataTable();
                            if (JSON.parse(data.Content).length) {
                                ChangeWcfDataToDataTableGridData(JSON.parse(data.Content), generalinsertdata);
                                AddDataToExistingDataTable(generalinsertdata, x);
                                x.dataTable().fnDraw();
                            }
                            Metronic.unblockUI($(subgridtable));
                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {

                            Metronic.unblockUI($(subgridtable).closest(".form-group"));
                            Metronic.unblockUI($(subgridtable));

                        }
                    });

                }

                Metronic.unblockUI($(subgridtable).closest(".form-group"));

                DataTable.on('click', 'tr', function () {

                    if (IscomingFromGrid != undefined) {
                        return;
                    }

                    if ($(subgridtable).attr("data-updateformid") == "" || $(subgridtable).attr("data-updateformid") == undefined) {
                        toastr["error"](NoSubGridUpdateForm, ErrorMessageHeader);
                        return;
                    }
                    Metronic.blockUI();
                    var table = $("#" + $(this).closest('table').attr("id"));
                    var data = $("#" + $(this).closest('table').attr("id")).dataTable().fnGetData(this);
                    var clickrowindex = $("#" + $(this).closest('table').attr("id")).DataTable().row(this).index();
                    $(this).closest('table').attr("data-clickindex", clickrowindex);

                    //this paramter is defined in General Functions!
                    SubGridDetailId = data[data.length - 1];
                    TempSubGridTableId = $(this).closest('table').attr("data-subgridid");
                    navigationid = navigationid == "" ? querystringarray["NavigationId"] : navigationid;
                    $.ajax({
                        url: "/Page/EditForm",
                        type: "GET",
                        data: {
                            FormId: $(t).attr("data-updateformid"),
                            WidgetId: $(t).attr("data-widgetid"),
                            PageWidgetId: $(table).attr("data-pagewidgetid"),
                            DataId: data[data.length - 1],
                            NavigationId: navigationid,
                            Editable: "true",
                            Ownership: "0",
                            FormOpenType: "modal",
                            IsComingFromSubGrid: "true"
                        },
                        dataType: "html",
                        async: true,
                        success: function (result) {
                            $("#" + $(table).attr("id")).closest(".scroller").css("height", "auto")
                            $("#" + $(table).attr("id")).closest(".slimScrollDiv").css("height", "auto")
                            $("#responsive_" + $(table).attr("data-pagewidgetid")).html(result)
                            $("#responsive_main_" + $(table).attr("data-pagewidgetid")).modal('toggle');


                            Metronic.unblockUI();
                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            Metronic.unblockUI();
                        }
                    });
                });

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                Metronic.unblockUI($(subgridtable).closest(".form-group"));

            }
        });

    });
}

function SetInitialValue(array, flag) {

    $('div[data-widgettype=form]').each(function () {
        if ($(this).attr("data-personel").trim() != "1") {
            var formid = $(this).attr("data-widgetid");
            var initialvals = $.grep(array, function (element, index) {
                return element.Id == formid;
            });

            if (initialvals.length > 0) {
                if (initialvals[0].data.initialvalues != null) {
                    if (initialvals[0].data.initialvalues.length > 0) {
                        var invalues = initialvals[0].data.initialvalues;

                        for (var i = 0; i < invalues.length; i++) {

                            var fieldelement = $.grep($('[data-attribute=yes]'), function (element, index) {
                                return $(element).attr("name") == invalues[i].attributelogicalname;
                            });

                            if (fieldelement.length > 0) {
                                var val = invalues[i].initialvalue;
                                $(fieldelement).attr("data-initial", 1)

                                if ($(fieldelement).attr("data-type") == "string") {
                                    if ($(fieldelement).attr("data-isurl") == "true") {
                                        var url = "";
                                        if (val.indexOf("http") == -1) {
                                            url = "http://" + val;
                                        }
                                        else {
                                            url = val;
                                        }
                                        $(fieldelement).val(url);
                                        $(fieldelement).parent().attr("href", url);
                                        $(fieldelement).parent().attr("target", "_blank");
                                    }
                                    else {
                                        $(fieldelement).val(val);
                                    }

                                }
                                else if ($(fieldelement).attr("data-type") == "picklist") {
                                    $(fieldelement).val(val)
                                }
                                else if ($(fieldelement).attr("data-type") == "lookup") {
                                    $(fieldelement).val(invalues[i].lookupnamevalue);
                                    $(fieldelement).attr("data-id", val);
                                }
                                else if ($(fieldelement).attr("data-type") == "customer") {
                                    $(fieldelement).val(invalues[i].lookupnamevalue);
                                    $(fieldelement).attr("data-id", val);
                                    $(fieldelement).attr("data-target", invalues[i].lookuplogicalname);
                                    $(fieldelement).next().find("button").attr("data-target", invalues[i].lookuplogicalname)
                                    $(fieldelement).next().find("button").attr("data-initiallogicalname", invalues[i].lookuplogicalname)
                                    $(fieldelement).parent().next().attr("id", "responsive_" + invalues[i].lookuplogicalname + "_" + $(fieldelement).attr("data-count"))
                                    $(fieldelement).parent().next().find("table").attr("id", invalues[i].lookuplogicalname + "_" + $(fieldelement).attr("data-count"))
                                }
                                else if ($(fieldelement).attr("data-type") == "money") {

                                    $(fieldelement).val(val.replace(",", "."));
                                }
                                else if ($(fieldelement).attr("data-type") == "ınteger" || $(fieldelement).attr("data-type") == "integer") {
                                    $(fieldelement).val(val)
                                }
                                else if ($(fieldelement).attr("data-type") == "decimal") {
                                    $(fieldelement).val(val.replace(",", "."));
                                }
                                else if ($(fieldelement).attr("data-type") == "double") {
                                    $(fieldelement).val(parseFloat(val.replace(",", ".")))
                                }
                                else if ($(fieldelement).attr("data-type") == "memo") {
                                    $(fieldelement).val(val)
                                }
                                else if ($(fieldelement).attr("data-type") == "boolean") {
                                    if ($(fieldelement).attr("type") == "radio") {
                                        for (var k = 0; k < fieldelement.length; k++) {
                                            if ($(fieldelement[k]).val() == val) {
                                                $(fieldelement[k]).attr("checked", "")
                                            }
                                            else
                                                $(fieldelement[k]).removeAttr("checked");
                                        }
                                    }
                                    if ($(fieldelement).attr("type") == "checkbox") {
                                        if (val == 1) {
                                            $(fieldelement).parent().addClass("checked")
                                        }
                                    }
                                }
                                else if ($(fieldelement).attr("data-type") == "datetime") {
                                    $(fieldelement).val(val)
                                }
                            }
                        }
                    }
                }

            }
        }

    });

}

function GetMeaningfulErrorMessage(ErrorMessage) {
    return ErrorMessage.substring(ErrorMessage.indexOf("<title>") + 7, ErrorMessage.indexOf("</title>"));
}

function GridOperations(RowObject, ComeFromAction) {

    if ($(RowObject).closest('table tr').find('td').hasClass('dataTables_empty') || $(RowObject).closest('table tr').find('th').length > 0)
        return false;

    if ($(RowObject).closest('table').attr("data-openwidgettype") == "form" || (ComeFromAction != null && ComeFromAction.OpeningWidgetType == "formwidget")) {
        //Set LangId into Ineternal Parameter
        var Language = LangId;
        //Open form in new window
        var openStyle = "", editable = "", ownership = "", formopenstyle = "", openformid = "";
        if ($(RowObject).closest('table').attr("data-formopenstyle") != "" && $(RowObject).closest('table').attr("data-formopenstyle") != undefined) {
            openStyle = $(RowObject).closest('table').attr("data-formopenstyle");
        }
        else if (ComeFromAction != undefined) {
            openStyle = ComeFromAction.OpenStyle;
        }
        //decide editable
        if ($(RowObject).closest('table').attr("data-editable") != "" && $(RowObject).closest('table').attr("data-editable") != undefined) {
            editable = $(RowObject).closest('table').attr("data-editable");
        }
        else if (ComeFromAction != undefined) {
            editable = ComeFromAction.Editable;
        }
        //decide ownership
        if ($(RowObject).closest('table').attr("data-ownership") != "" && $(RowObject).closest('table').attr("data-ownership") != undefined) {
            ownership = $(RowObject).closest('table').attr("data-ownership");
        }
        else if (ComeFromAction != undefined) {
            ownership = ComeFromAction.OwnerShip;
        }
        //decide openstyle
        if ($(RowObject).closest('table').attr("data-formopenstyle") != "" && $(RowObject).closest('table').attr("data-formopenstyle") != undefined) {
            formopenstyle = $(RowObject).closest('table').attr("data-formopenstyle");
        }
        else if (ComeFromAction != undefined) {
            formopenstyle = ComeFromAction.OpenStyle;
        }
        //decide openformid
        if ($(RowObject).closest('table').attr("data-openformid") != "" && $(RowObject).closest('table').attr("data-openformid") != undefined) {
            openformid = $(RowObject).closest('table').attr("data-openformid");
        }
        else if (ComeFromAction != undefined) {
            openformid = ComeFromAction.OpenFormId;
        }
        if (openStyle != "modal") {
            var data = $("#" + $(RowObject).closest('table').attr("id")).dataTable().fnGetData(RowObject);
            var parameters = "width=" + (screen.availWidth - 10) + ",height=" + (screen.availHeight - 122) + ',scrollbars=yes,toolbar=0,resizable=yes,titlebar=no';
            wOpen = window.open('/Page/EditForm?FormId=' + openformid +
                                '&WidgetId=' + $(RowObject).closest('table').attr("id") +
                                '&DataId=' + data[data.length - 1] +
                                '&NavigationId=' + navigationid +
                                '&Editable=' + editable +
                                '&Ownership=' + ownership.trim() +
                                '&PageWidgetId=' + $(RowObject).closest('table').attr("id") +
                                '&Language=' + Language,
                                '_blank',
                                parameters);
            wOpen.moveTo(0, 0);
            wOpen.resizeTo(screen.availWidth, screen.availHeight);
        }
        else {
            //open form in Modal
            Metronic.blockUI();
            //get related data of the datable
            var data = $("#" + $(RowObject).closest('table').attr("id")).dataTable().fnGetData($(RowObject));
            var table = $(RowObject).closest('table');
            $.ajax({
                url: "/Page/EditForm",
                type: "GET",
                data: {
                    FormId: openformid,
                    WidgetId: $(table).attr("data-widgetid"),
                    PageWidgetId: $(table).attr("id"),
                    DataId: data[data.length - 1],
                    NavigationId: navigationid,
                    Editable: editable,
                    Ownership: ownership.trim(),
                    FormOpenType: formopenstyle,
                    Language: Language
                },
                cache: false,
                dataType: "html",
                async: true,
                success: function (result) {
                    $("#" + $(table).attr("id")).closest(".scroller").css("height", "auto");
                    $("#" + $(table).attr("id")).closest(".slimScrollDiv").css("height", "auto");
                    $("#responsive_" + $(table).attr("id")).html(result);
                    $("#responsive_main_" + $(table).attr("id")).modal('toggle');
                    Metronic.unblockUI();

                    RenderSubGrids("true");
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    Metronic.unblockUI();
                }
            });
        }
    }
    else if ($(RowObject).closest('table').attr("data-openwidgettype") == "html" || (ComeFromAction != null && ComeFromAction.OpeningWidgetType == "htmlwidget")) {

        var data = $("#" + $(RowObject).closest('table').attr("id")).dataTable().fnGetData(RowObject);
        var table = $(RowObject).closest('table');

        var openhtmlid = "", openstyle = "";
        if ($(RowObject).closest('table').attr("data-openformid") != "" && $(RowObject).closest('table').attr("data-openformid") != undefined) {
            openhtmlid = $(table).attr("data-openhtmlid");
        }
        else {
            openhtmlid = ComeFromAction.OpenFormId;
        }
        if ($(RowObject).closest('table').attr("data-formopenstyle") != "" && $(RowObject).closest('table').attr("data-formopenstyle") != undefined) {
            openstyle = $(table).attr("data-formopenstyle");
        }
        else {
            openstyle = ComeFromAction.OpenStyle;
        }

        if (openstyle != "modal") {
            var parameters = "width=" + screen.availWidth - 10 + ",height=" + screen.availHeight - 122 + ',scrollbars=yes, toolbar=0,resizable=yes';
            wOpen = window.open('/Page/OpenHTMLWidget?WidgetId=' + $(table).attr("data-widgetid") +
                                '&WidgetGuid=' + openhtmlid +
                                '&EntityId=' + data[data.length - 1] +
                                '&OpenType=' + openstyle +
                                '&NavigationId=' + navigationid +
                                '&Language=' + Language
                                , '_blank', parameters);
            wOpen.moveTo(0, 0);
            wOpen.resizeTo(screen.availWidth, screen.availHeight);
        }
        else {
            Metronic.blockUI();
            $.ajax({
                url: "/Page/OpenHTMLWidget",
                type: "POST",
                data: JSON.stringify({ 'WidgetId': $(table).attr("data-widgetid"), 'WidgetGuid': openhtmlid, 'EntityId': data[data.length - 1], 'OpenType': openstyle, 'Language': Language }),
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                async: true,
                success: function (result) {
                    var html = JSON.parse(result.Content)

                    $("#" + $(table).attr("id")).closest(".scroller").css("height", "auto")
                    $("#" + $(table).attr("id")).closest(".slimScrollDiv").css("height", "auto")
                    $("#responsive_" + $(table).attr("id")).html(html)
                    $("#responsive_main_" + $(table).attr("id")).modal('toggle');

                    Metronic.unblockUI();
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    Metronic.unblockUI();
                }
            });
        }

    }
}

function CustomActions(ButtonObject, EntityId) {

    Metronic.blockUI();
    //for modals loading message
    Metronic.blockUI({
        target: $(ButtonObject).closest(".formwidgetclass"),
        overlayColor: 'none',
        animate: true
    });
    if ($(ButtonObject).attr("data-workflowid") != "" && $(ButtonObject).attr("data-workflowid") != undefined) {

        $.ajax({
            url: "/Page/RenderCustomActions",
            type: "POST",
            data: JSON.stringify({ 'EntityId': EntityId, 'WorkFlowId': $(ButtonObject).attr("data-workflowid") }),
            dataType: "json",
            async: true,
            contentType: "application/json; charset=utf-8",
            beforeSend: function (XMLHttpRequest) {
                XMLHttpRequest.setRequestHeader("Accept", "application/json");
            },
            success: function (data) {
                if (JSON.parse(data.Content) != "") {
                    if ($(ButtonObject).attr("data-errormessage") != "" && $(ButtonObject).attr("data-errormessage") != undefined) {
                        toastr["error"]($(ButtonObject).attr("data-errormessage"), ErrorMessageHeader);
                    }
                    else {
                        toastr["error"](JSON.parse(data.Content), ErrorMessageHeader);
                    }

                }
                else {
                    if ($(ButtonObject).attr("data-returnmessage") != "" && $(ButtonObject).attr("data-returnmessage") != undefined) {
                        toastr["success"]($(ButtonObject).attr("data-returnmessage"), SuccessMessageHeader);
                    }
                    else {
                        toastr["success"](GeneralSuccessMessage, SuccessMessageHeader);
                    }
                }
                Metronic.unblockUI();
                Metronic.unblockUI($(ButtonObject).closest(".formwidgetclass"));
            },

            error: function (XMLHttpRequest, textStatus, errorThrown) {
                Metronic.unblockUI();
                Metronic.unblockUI($(ButtonObject).closest(".formwidgetclass"));
                toastr["error"]("Error during execution", "Error");

            }
        });
    }
}

function GetInitializeFormValuesForArray() {

    initFormValueArray = [];

    $('div[data-widgettype="form"]').find("[data-attribute=yes]").each(function () {
        var senderobject = {};
        entityname = $(this).closest('.panel-body').attr("data-entityname");
        $(this).attr("name", $(this).attr("name").replace("subgrid_", ""));
        $(this).attr("name", $(this).attr("name").replace("bpf_", ""));

        var type = $(this).attr("data-type");

        if (type == "string") {

            if ($(this).attr("data-formatted") == 1) {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = $(this).val().replace("(", "").replace(")", "").replace(/ /g, '');
                initFormValueArray.push(senderobject);
            }
            else {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = $(this).val();
                initFormValueArray.push(senderobject);
            }

        }
        else if (type == "lookup") {
            if ($(this).attr("data-id") != "" && $(this).attr("data-id") != undefined) {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = $(this).attr("data-id");
                senderobject.entityname = $(this).attr("data-target");

                initFormValueArray.push(senderobject);
            }
        }
        else if (type == "customer") {
            if ($(this).attr("data-id") != "" && $(this).attr("data-id") != undefined) {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = $(this).attr("data-id");
                senderobject.entityname = $(this).attr("data-target");
                initFormValueArray.push(senderobject);
            }
        }
        else if (type == "ınteger" || type == "integer") {

            senderobject.logicalname = $(this).attr("name");
            senderobject.type = type;
            senderobject.value = $(this).val();
            initFormValueArray.push(senderobject);

        }
        else if (type == "money") {

            senderobject.logicalname = $(this).attr("name");
            senderobject.type = type;
            senderobject.value = $(this).val();
            initFormValueArray.push(senderobject);

        }
        else if (type == "decimal") {

            senderobject.logicalname = $(this).attr("name");
            senderobject.type = type;
            senderobject.value = $(this).val();
            initFormValueArray.push(senderobject);

        }
        else if (type == "double") {

            senderobject.logicalname = $(this).attr("name");
            senderobject.type = type;
            senderobject.value = $(this).val();
            initFormValueArray.push(senderobject);

        }
        else if ($(this).attr("data-type") == "metronicdatetime") {
            if ($(this).val() != "") {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = "datetime";
                senderobject.value = $(this).val();
                senderobject.datetimepicker = $(this).attr("data-timepicker");
                senderobject.dateformat = $(this).attr("data-beforedateformat");
                senderobject.timeformat = $(this).attr("data-beforetimeformat");
                initFormValueArray.push(senderobject);
            }

        }
        else if ($(this).attr("data-type") == "metronicdate") {
            if ($(this).val() != "") {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = "datetime";
                senderobject.value = $(this).val();
                senderobject.datetimepicker = $(this).attr("data-timepicker");
                senderobject.dateformat = $(this).attr("data-beforedateformat");
                senderobject.timeformat = $(this).attr("data-beforetimeformat");
                initFormValueArray.push(senderobject);
            }

        }
        else if (type == "datetime") {
            if ($(this).val() != "") {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = $(this).val();
                senderobject.datetimepicker = $(this).attr("data-timepicker");
                senderobject.dateformat = $(this).attr("data-beforedateformat");
                senderobject.timeformat = $(this).attr("data-beforetimeformat");
                initFormValueArray.push(senderobject);
            }
        }
        else if (type == "picklist") {
            if ($(this).val() != null) {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = $(this).find(":selected").val();
                initFormValueArray.push(senderobject);
            }
            else {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = null;
                initFormValueArray.push(senderobject);

            }
        }
        else if (type == "memo") {

            senderobject.logicalname = $(this).attr("name");
            senderobject.type = type;
            senderobject.value = $(this).val();
            initFormValueArray.push(senderobject);

        }
        else if (type == "boolean") {
            if ($(this).attr("type") == "radio") {
                if ($(this).parent().hasClass("checked") == true) {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderobject.twooptiontype = $(this).attr("type");
                    initFormValueArray.push(senderobject);
                }
            }
            if ($(this).attr("type") == "checkbox") {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = $(this).parent().hasClass("checked") == true ? "1" : "0";
                senderobject.twooptiontype = $(this).attr("type");
                initFormValueArray.push(senderobject);

            }
        } else if (type == "guid") {

            senderobject.logicalname = $(this).attr("name");
            senderobject.type = type;
            senderobject.value = $(this).val();
            initFormValueArray.push(senderobject);

        }
    });
}

function CheckOldControl(oldArray) {

    if (oldArray != null > oldArray.length > 0) {

        var postArray = [];

        for (var i = 0; i < oldArray.length; i++) {

            if (initFormValueArray[i] != undefined && oldArray[i].value != initFormValueArray[i].value) {
                postArray.push(oldArray[i]);
            } else if (initFormValueArray[i]) {
                postArray.push(oldArray[i]);
            }
        }

        return postArray;

    } else {
        return oldArray;
    }



}

function MakeChart(data, id) {
    //column charts
    if (data[0].ChartType == 2) {
        var chartArry = [];
        for (var i in data[0].data) {
            var chartobj = {};
            var obj = data[0].data[i];
            if (obj.Series1 != "999999") {
                chartobj.Series1 = parseFloat(obj.Series1 == "" ? 0 : obj.Series1);
            }
            if (obj.Series2 != "999999") {
                chartobj.Series2 = parseFloat(obj.Series2 == "" ? 0 : obj.Series2);
            }
            if (obj.Series3 != "999999") {
                chartobj.Series3 = parseFloat(obj.Series3 == "" ? 0 : obj.Series3);
            }
            if (obj.Series4 != "999999") {
                chartobj.Series4 = parseFloat(obj.Series4 == "" ? 0 : obj.Series4);
            }
            if (obj.Series5 != "999999") {
                chartobj.Series5 = parseFloat(obj.Series5 == "" ? 0 : obj.Series5);
            }
            chartobj.Color1 = obj.SeriesColor1;
            chartobj.Color2 = obj.SeriesColor2;
            chartobj.Color3 = obj.SeriesColor3;
            chartobj.Color4 = obj.SeriesColor4;
            chartobj.Color5 = obj.SeriesColor5;

            chartobj.Name1 = obj.Series1Name;
            chartobj.Name2 = obj.Series2Name;
            chartobj.Name3 = obj.Series3Name;
            chartobj.Name4 = obj.Series4Name;
            chartobj.Name5 = obj.Series5Name;

            chartobj.Horizontal = obj.Horizontal;
            chartArry.push(chartobj);
        }
        var graphs = [];
        if (chartArry.length > 0) {
            for (var i in chartArry[0]) {

                if (i.indexOf("Series") != -1) {
                    // first graph
                    var graph1 = {};
                    graph1.alphaField = "alpha",
                    graph1.fillAlphas =  1,
                    graph1.title = chartArry[0]["Color" + i.replace("Series", "")] != null ? chartArry[0]["Name" + i.replace("Series", "")] : "nullattribute";
                    graph1.valueField = i;
                    graph1.type = "column";
                    graph1.lineAlpha = 5;
                    graph1.lineColor = chartArry[0]["Color" + i.replace("Series", "")] != null ? "#" + chartArry[0]["Color" + i.replace("Series", "")] : '#' + Math.floor(Math.random() * 16777215).toString(16); 
                    graph1.balloonText = "[[title]] : <b>[[value]]</b>";
                    graph1.labelText = data[0].LabelText;
                    fontSize = data[0].ChartFontSize;
                    graphs.push(graph1);
                }

            }
        }
        var chart = AmCharts.makeChart("chartdiv_" + id, {
            //Is3D
            marginLeft: 30,
            depth3D: data[0].Is3D == "1" ? 15 : 0,
            angle: data[0].Is3D == "1" ? 30 : 0,
            handDrawn: data[0].IsHandWrite == "1" ? true : false,
            handDrawScatter: data[0].IsHandWrite == "1" ? 3 : 0,
            fontFamily: data[0].ChartFontFamily,                      
            color: '#' + data[0].ChartColor,           
            type: "serial",
            theme: "light",    
            pathToImages: "/amcharts/amcharts/images/",
            autoMargins: false,          
            startDuration: 1,
            dataProvider: chartArry,
            graphs: graphs,
            categoryField: "Horizontal",        
            categoryAxis: {
                gridPosition: "start",
                axisAlpha: 0,
                tickLength: 0,
                gridAlpha: 0.1,
            },
            legend: {
                useGraphSettings: true,
                markerSize: 12,
                valueWidth: 0,
                verticalGap: 0,
                "align": "right",
                position: data[0].ChartLegendPosition,
            },
            valueAxes: [{
                axisAlpha: 0,
                gridAlpha: 0.1,
            }],
        });

       
    }
        //piecharts && Funnel charts
    else if (data[0].ChartType == 1 || data[0].ChartType == 3) {
        var chartArry = [];

        for (var i in data[0].data) {
            var chartobj = {};
            var obj = data[0].data[i];
            if (obj.Series != "999999") {
                chartobj.Series = parseFloat(obj.Series == "" ? 0 : obj.Series);
            }
            chartobj.Horizontal = obj.Horizontal;
            chartArry.push(chartobj);
        }
        if (data[0].ChartType == 1) {
            var chart = AmCharts.makeChart("chartdiv_" + id, {
                type: "pie",
                theme: "light",
                rotate : true,
                depth3D: data[0].Is3D == "1" ? 15 : 0,
                angle: data[0].Is3D == "1" ? 30 : 0,
                handDrawn: data[0].IsHandWrite == "1" ? true : false,
                handDrawScatter: data[0].IsHandWrite == "1" ? 3 : 0,
                fontFamily: data[0].ChartFontFamily,
                fontSize: data[0].ChartFontSize,
                color: '#' + data[0].ChartColor,
                outlineAlpha: 1,
                outlineColor: "#FFFFFF",
                outlineThickness: 2,
                dataProvider: chartArry,
                valueField: "Series",
                titleField: "Horizontal",
                labelText: data[0].LabelText,
                balloonText: "[[title]]<br><span style='font-size:14px'><b>[[value]]</b> ([[percents]]%)</span>",
                //"innerRadius": "30%",
            });
         
            function handleRollOver(e) {
                var wedge = e.dataItem.wedge.node;
                wedge.parentNode.appendChild(wedge);
            }
        }
        else if(data[0].ChartType == 3) {
            var chart = AmCharts.makeChart("chartdiv_" + id, {
                type: "funnel",
                theme: "light",
                depth3D: data[0].Is3D == "1" ? 80 : 0,
                angle: data[0].Is3D == "1" ? 30 : 0,
                handDrawn: data[0].IsHandWrite == "1" ? true : false,
                handDrawScatter: data[0].IsHandWrite == "1" ? 3 : 0,
                fontFamily: data[0].ChartFontFamily,
                color: '#' + data[0].ChartColor,
                fontSize : data[0].ChartFontSize,
                dataProvider: chartArry,
                valueField: "Series",
                titleField: "Horizontal",
                cornerRadius : 0,
                marginRight : 220,
                marginLeft: 15,
                marginTop : 50,
                labelPosition : "right",
                funnelAlpha : 0.9,
                startX : -500,
                neckWidth : "40%",
                startAlpha : 0,
                neckHeight: "30%",
                outlineAlpha: 1,
                outlineColor: "#FFFFFF",
                outlineThickness: 2,
                labelText: data[0].LabelText,
            });
        }
 
    }

}

function BRValidation() {
    var input = $('input[has-data-error="true"]').first();

    if (input != null && input.length > 0 ) {
        var errorText = $(input).attr('data-error-text');
        $(input).focus();       

        toastr["error"](errorText, ErrorMessageHeader);

        return "error";
    }

    return "";
}