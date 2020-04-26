namespace WithTooltip {
    export function register(content: HTMLElement, tooltip: HTMLElement) {
        content.onmouseenter = (ev) => reposition(tooltip, ev.clientX, ev.clientY);
        content.onmousemove = (ev) => reposition(tooltip, ev.clientX, ev.clientY);
        content.onmouseleave = (ev) => deposition(tooltip);
        content.onclick = (ev) => deposition(tooltip);
    }

    function reposition(element: HTMLElement, x: number, y: number) {
        element.style.display = "inherit";

        if (window.innerWidth - x < 120) {
            element.style.left = null;
            element.style.right = (window.innerWidth - x) + 2 + "px";
        } else {
            element.style.left = x + 2 + "px";
            element.style.right = null;
        }        

        if (window.innerHeight - y < 300) {
            element.style.top = null;
            element.style.bottom = (window.innerHeight - y) + 2 + "px";
        } else {
            element.style.top = y + 2 + "px";
            element.style.bottom = null;
        }
        
        if (element.parentElement.id != "with-tooltip__holder") {
            (element as any).oldParent = element.parentElement;

            let holder = document.querySelector("#with-tooltip__holder");
            let existingChild = holder.firstElementChild;
            if (existingChild != null) {
                deposition(existingChild as HTMLElement);
            }

            holder.appendChild(element);
        }
    }

    function deposition(element: HTMLElement) {
        element.style.display = "none";
        (element as any).oldParent.appendChild(element);
    }
}