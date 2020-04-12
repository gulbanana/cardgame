"use strict";
var WithTooltip;
(function (WithTooltip) {
    function reposition(element, x, y) {
        element.classList.add("with-tooltip__tooltip--visible");
        element.style.left = x + 2 + "px";
        if (window.innerHeight - y < 200) {
            element.querySelector(".magnify").classList.add("magnify--bottom-left");
            element.style.top = null;
            element.style.bottom = (window.innerHeight - y) + 2 + "px";
        }
        else {
            element.querySelector(".magnify").parentElement.classList.remove("magnify--bottom-left");
            element.style.top = y + 2 + "px";
            element.style.bottom = null;
        }
    }
    WithTooltip.reposition = reposition;
    function deposition(element) {
        element.classList.remove("with-tooltip__tooltip--visible");
    }
    WithTooltip.deposition = deposition;
})(WithTooltip || (WithTooltip = {}));
