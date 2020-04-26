namespace MainLayout {
    export function maintainFixedHeight(element: HTMLElement) {
        element.style.height = window.innerHeight + "px";
        window.onresize = () => element.style.height = window.innerHeight + "px";
    }
}