$(document).ready(function () {

    var attr = $("input[name='gsc_isportalcreated']");
    attr.each(function () {
        if ($(this).val() == 0) {
            $(this).removeAttr("checked");
        }
        else if ($(this).val() == 1) {
            $(this).attr("checked", "checked");
        }
    });
});