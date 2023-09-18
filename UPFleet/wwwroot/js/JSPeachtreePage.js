$(document).ready(function () {
    $("#prntbtn").click(function () {
        $(".function-btn").hide();
        window.print();
        $(".function-btn").show();
    });    
});