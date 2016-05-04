var formid = "", widgetid = "", widgetguid = "", pagewidgetid = "";
$(document).ready(function () {

    var html = "<div class='btn-group'>";;
    html += "<button id=createcase class='btn purple exporter'>"
    html += "Servis Talebi Oluştur";
    html += " </button>";
    html += "</div>";

    $(html).appendTo($(".updater").parent());
    var formwidgetid = "PW-Form-SveE6";

    var modaldiv = '<div id="responsive_main_' + formwidgetid + '" data-divtype="subgrid" class="modal fade modal-scroll"  tabindex="-1" data-replace="true">';
    modaldiv += '<div class="modal-dialog modal-full">';
    modaldiv += '<div class="modal-content">';
    modaldiv += '<div class="modal-body">';
    modaldiv += '<div id="responsive_' + formwidgetid + '"></div>';
    modaldiv += '</div>'
    modaldiv += '<div class="modal-footer">';
    modaldiv += '<button type="button" data-dismiss="modal" class="btn">Kapat</button>';
    modaldiv += '</div>';
    modaldiv += '</div>';
    modaldiv += '</div>';
    modaldiv += '</div>';
    $(modaldiv).appendTo($("div[data-widgetid='" + formwidgetid + "']").parent().parent());

    widgetguid = $("div[data-widgetid='" + formwidgetid + "']").attr("data-widgetguid");
    widgetid = $("div[data-widgetid='" + formwidgetid + "']").attr("data-widget");
    formid = $("div[data-widgetid='" + formwidgetid + "']").attr("data-formid");
    pagewidgetid = $("div[data-widgetid='" + formwidgetid + "']").attr("data-widgetid");

    $("div[data-widgetid='" + formwidgetid + "']").parent().empty();

    $("#createcase").on("click", function () {

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
                $("#responsive_" + formwidgetid).empty();
                $("#responsive_main_" + formwidgetid).closest(".scroller").css("height", "auto");
                $("#responsive_main_" + formwidgetid).closest(".slimScrollDiv").css("height", "auto");
                $("#responsive_" + formwidgetid).html(result);
                $("#responsive_main_" + formwidgetid).modal('toggle');

                $("#subgrid_ps_pensioncontractid").attr("data-id", $("#ps_pensioncontractid").attr("data-id"));
                $("#subgrid_ps_pensioncontractid").val( $("#ps_pensioncontractid").val());
                Metronic.unblockUI();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                Metronic.unblockUI();
            }
        });

    });
});