let selectionEnabled = false;

document.getElementById("enableSelectionTranslate")?.addEventListener("change", function () {
    selectionEnabled = this.checked;
});

document.addEventListener("mouseup", function () {
    if (!selectionEnabled) return;

    const text = window.getSelection().toString().trim();
    if (text.length === 0) return;

    document.getElementById("source-textarea").value = text;
});
