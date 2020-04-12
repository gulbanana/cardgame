namespace WithTooltip {
    export function reposition(element: HTMLElement, x: number, y: number) {
        element.classList.add("with-tooltip__tooltip--visible");

        element.style.left = x+2 + "px";

        if (window.innerHeight - y < 200) {
            element.querySelector(".magnify").classList.add("magnify--bottom-left");
            element.style.top = null;
            element.style.bottom = (window.innerHeight - y) + 2 + "px";
        } else {
            element.querySelector(".magnify").parentElement.classList.remove("magnify--bottom-left");
            element.style.top = y + 2 + "px";
            element.style.bottom = null;
        }
    }

    export function deposition(element: HTMLElement) {
        element.classList.remove("with-tooltip__tooltip--visible");
    }
}