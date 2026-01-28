window.translationTask = 0;

document.addEventListener(
    'selectionchange', () => {
        const fragment = document.getSelection().toString();
        console.log(fragment);
        if (window.translationTask != 0) {
            clearTimeout(window.translationTask);
        }
        window.translationTask = setTimeout(
            () => translate(fragment),
            1000
        );
    }
);

function translate(fragment) {
    fragment = fragment.trim();
    if (fragment.length > 0) {
        const langFrom = document.querySelector('select[name="lang-from"]').value;
        const langTo = document.querySelector('select[name="lang-to"]').value;

        console.log("Translated: ", fragment);
        fetch(`/Home/FetchTranslation?lang-from=${langFrom}&lang-to=${langTo}&original-text=${fragment}&action-button=fetch`)
            .then(r => r.json())
            .then(j => {
                alert(j);
            });
    }    
    window.translationTask = 0;
}