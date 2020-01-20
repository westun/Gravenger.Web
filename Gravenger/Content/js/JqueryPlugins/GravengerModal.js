(function ($) {
    var methods = {
        init: function (options) {

        },
        show: function () {
            var $element = this;

            var modal = $element[0];
            modal.style.display = "block";
            var closebutton = $element.find('.modal-close-button')[0];
            window.addEventListener("click", function (event) {
                if (event.target == modal || event.target == closebutton) {
                    modal.style.display = "none";
                };
            });
        },
        close: function () {
            var $element = this;
            var modal = $element[0];
            modal.style.display = "none";
        }
    };

    $.fn.gravengerModal = function (methodOrOptions) {
        if (methods[methodOrOptions]) {
            return methods[methodOrOptions].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof methodOrOptions === 'object' || !methodOrOptions) {
            // Default to "init"
            return methods.init.apply(this, arguments);
        } else {
            $.error('Method ' + methodOrOptions + ' does not exist on jQuery.gravengerModal');
        }
    };
})(jQuery);