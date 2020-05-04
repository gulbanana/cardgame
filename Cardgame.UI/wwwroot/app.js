"use strict";
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
var HorizontalLog;
(function (HorizontalLog) {
    function observeAdditions(container) {
        let config = { childList: true, subtree: true };
        let observer = new MutationObserver((muts, o) => {
            container.scrollLeft = (container.scrollWidth - container.clientWidth);
        });
        observer.observe(container, config);
    }
    HorizontalLog.observeAdditions = observeAdditions;
})(HorizontalLog || (HorizontalLog = {}));
var MainLayout;
(function (MainLayout) {
    function maintainFixedHeight(element) {
        element.style.height = window.innerHeight + "px";
        window.onresize = () => element.style.height = window.innerHeight + "px";
    }
    MainLayout.maintainFixedHeight = maintainFixedHeight;
})(MainLayout || (MainLayout = {}));
var VerticalLog;
(function (VerticalLog) {
    function observeAdditions(container) {
        let config = { childList: true, subtree: true };
        let observer = new MutationObserver((muts, o) => {
            setTimeout(() => {
                container.scrollIntoView(false);
            }, 0);
        });
        observer.observe(container, config);
    }
    VerticalLog.observeAdditions = observeAdditions;
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
            let existingChild = holder.firstElementChild;
            if (existingChild != null) {
                deposition(existingChild);
            }
            holder.appendChild(element);
        }
    }
    function deposition(element) {
        element.style.display = "none";
        element.oldParent.appendChild(element);
    }
})(WithTooltip || (WithTooltip = {}));
