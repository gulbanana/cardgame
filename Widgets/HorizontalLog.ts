namespace HorizontalLog {
    export function observeAdditions(container: HTMLElement) {
        let config = { childList: true, subtree: true };
        let observer = new MutationObserver((muts, o) => {
            container.scrollLeft = (container.scrollWidth - container.clientWidth);
        });
        observer.observe(container, config);
    }
}