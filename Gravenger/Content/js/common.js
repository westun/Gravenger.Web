//Jquery block ui for ajax calls
$(document).ajaxStop(function () {
    $('body').unblock();
});
$(document).ajaxStart(function () {
    $('body').block();
});
$(document).ajaxError(function (event, request, settings, exception) {
    $("body").prepend("<div id='alert_error' class='container alert alert-error alert-block'><h4>Whoops!</h4>An error has occurred.  &nbsp;Please try to reload the page.  &nbsp;If the problem persists, try to restart your browser..</div>");
    $("body").children().not("#alert_error, #nav_container").hide();
    $('body').unblock();
});