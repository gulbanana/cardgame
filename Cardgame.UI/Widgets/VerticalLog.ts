namespace VerticalLog {
    export function observeAdditions(container: HTMLElement) {
        let config = { childList: true, subtree: true };
        let observer = new MutationObserver((muts, o) => {
            setTimeout(() => {
                container.scrollIntoView(false);
            }, 0);   
        });
        observer.observe(container, config);
    }
}