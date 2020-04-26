namespace VerticalLog {
    export function observeAdditions(container: HTMLElement) {
        let config = { childList: true, subtree: true };
        let observer = new MutationObserver((muts, o) => {
            container.scrollIntoView(false);
        });
        observer.observe(container, config);
    }
}