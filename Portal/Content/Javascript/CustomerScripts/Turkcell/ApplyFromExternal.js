$(document).ready(function () {
    $(".container").css("width", "1280px");
    $(".modal-dialog.modal-full").css("width", "50%");
     
    $('.modal.fade').on('shown.bs.modal', function () {
        $("#trailer").addClass("disabled");
    });

    $(".container").find(".bodycontent").css("padding-top", "15px");
    $(".container").find(".bodycontent").css("background-color", "white");
    $(".page-content").css("height", "auto");
    $(".page-content").css("margin-bottom", "35px");

    $("a[href='/E/kullanici-olustur-PAYq7/tr-TR']").css("background", "#c23f44");
});