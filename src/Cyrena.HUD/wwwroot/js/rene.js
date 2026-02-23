
window.scrollToBottom = (el, force = false, threshold = 80) => {
    if (!el) return;

    const distanceFromBottom =
        el.scrollHeight - el.scrollTop - el.clientHeight;

    const isNearBottom = distanceFromBottom <= threshold;

    if (force || isNearBottom) {
        el.scrollTo({
            top: el.scrollHeight,
            behavior: "smooth"
        });
    }
};

window.autoGrow = (el, maxRows) => {
    if (!el) return;

    el.style.height = "auto";

    const lineHeight = parseInt(getComputedStyle(el).lineHeight);
    const maxHeight = lineHeight * maxRows;

    el.style.height = Math.min(el.scrollHeight, maxHeight) + "px";
};

window.triggerClick = (element) => {
    if (element) {
        element.click();
    }
};