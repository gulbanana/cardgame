"use strict";
var ActiveCard;
(function (ActiveCard) {
    function scrollIntoView(content) {
        content.scrollIntoView();
    }
    ActiveCard.scrollIntoView = scrollIntoView;
})(ActiveCard || (ActiveCard = {}));
var FadeIn;
(function (FadeIn) {
    function removeClass(element) {
        element.classList.remove("fade-in--new");
    }
    FadeIn.removeClass = removeClass;
})(FadeIn || (FadeIn = {}));
var FlashBorder;
(function (FlashBorder) {
    function removeClass(element) {
        element.classList.remove("flash-border--new");
    }
    FlashBorder.removeClass = removeClass;
})(FlashBorder || (FlashBorder = {}));
var VerticalLog;
(function (VerticalLog) {
    function scrollToBottom(content) {
        content.scrollIntoView(false);
    }
    VerticalLog.scrollToBottom = scrollToBottom;
})(VerticalLog || (VerticalLog = {}));
var WithTooltip;
(function (WithTooltip) {
    function register(content, tooltip) {
        content.onmouseenter = (ev) => reposition(tooltip, ev.clientX, ev.clientY);
        content.onmousemove = (ev) => reposition(tooltip, ev.clientX, ev.clientY);
        content.onmouseleave = (ev) => deposition(tooltip);
        content.onclick = (ev) => deposition(tooltip);
    }
    WithTooltip.register = register;
    function reposition(element, x, y) {
        element.style.display = "inherit";
        if (window.innerWidth - x < 120) {
            element.style.left = null;
            element.style.right = (window.innerWidth - x) + 2 + "px";
        }
        else {
            element.style.left = x + 2 + "px";
            element.style.right = null;
        }
        if (window.innerHeight - y < 300) {
            element.style.top = null;
            element.style.bottom = (window.innerHeight - y) + 2 + "px";
        }
        else {
            element.style.top = y + 2 + "px";
            element.style.bottom = null;
        }
        if (element.parentElement.id != "with-tooltip__holder") {
            element.oldParent = element.parentElement;
            let holder = document.querySelector("#with-tooltip__holder");
            let existingChild = holder.firstChild;
            if (existingChild != null) {
                existingChild.oldParent.appendChild(element);
            }
            holder.appendChild(element);
        }
    }
    function deposition(element) {
        element.style.display = "none";
        element.oldParent.appendChild(element);
    }
})(WithTooltip || (WithTooltip = {}));
