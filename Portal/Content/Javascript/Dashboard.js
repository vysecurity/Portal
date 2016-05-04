$(document).ready(function () {
    Metronic.init(); // init metronic core componets
    Layout.init(); // init layout
    //to change the error page direction
    $(".menuclick > a").each(function(){
        $(this).attr("href", $(this).attr("href").replace("Error", "Page"));
    });

});