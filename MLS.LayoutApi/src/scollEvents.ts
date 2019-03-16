
export interface ITopmostSEssionInContainerChangedHandler {
    (sessionId: string): void
}

function getTop(e: HTMLElement): number {
    return e.getBoundingClientRect().top;
}

function getTopMostVisible<T extends HTMLElement>(elements: T[]): T {
    const height_threshold = 20;
    for (let element of elements) {
        if (element.getBoundingClientRect().bottom > height_threshold)
            return element;
    }
}

export function trackTopmostSession(container: HTMLElement, handler: ITopmostSEssionInContainerChangedHandler): void {
    if (handler) {
        let sessionEditors: HTMLIFrameElement[] = [];
        let topMost: HTMLIFrameElement = null;
        container.addEventListener("scroll", scrollEvent => {
            if (sessionEditors.length <= 0) {
                container.querySelectorAll<HTMLIFrameElement>("iframe[data-trydotnet-session-id]").forEach(e => {
                    sessionEditors.push(e);
                    sessionEditors.sort((a, b) => {
                        return getTop(a) - getTop(b);
                    });
                });
            }
            else {
                let currentTopMost = getTopMostVisible(sessionEditors);
                if (currentTopMost !== topMost) {
                    let prevSession = topMost ? topMost.dataset.trydotnetSessionId : "";
                    topMost = currentTopMost;
                    if (prevSession !== topMost.dataset.trydotnetSessionId) {
                        handler(topMost.dataset.trydotnetSessionId);
                    }
                }
            }
        });
    }
}