namespace WithTooltip {
    export function register(content: HTMLElement, tooltip: HTMLElement) {
        content.onmouseenter = (ev) => reposition(tooltip, ev.clientX, ev.clientY);
        content.onmousemove = (ev) => reposition(tooltip, ev.clientX, ev.clientY);
        content.onmouseleave = (ev) => deposition(tooltip);
    }

    function reposition(element: HTMLElement, x: number, y: number) {
        element.classList.add("with-tooltip__tooltip--visible");

        element.style.left = x+2 + "px";

        if (window.innerHeight - y < 300) {
            let zoomedChild = element.querySelector(".magnify");
            if (zoomedChild != null) {
                zoomedChild.classList.add("magnify--bottom-left");
            }

            element.style.top = null;
            element.style.bottom = (window.innerHeight - y) + 2 + "px";
        } else {
            let zoomedChild = element.querySelector(".magnify");
            if (zoomedChild != null) {
                zoomedChild.classList.remove("magnify--bottom-left");
            }

            element.style.top = y + 2 + "px";
            element.style.bottom = null;
        }
    }

    function deposition(element: HTMLElement) {
        element.classList.remove("with-tooltip__tooltip--visible");
    }
}