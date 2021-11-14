window.setBackgroundImage1 = (url) => document.getElementsByTagName("body")[0].style =
    `background-image: url(${url})`;

window.runSearchQuery = (url) => chrome.search.query({text: url}, () => {
    console.log(url);
});

function enableToasts() {
    var toastElList = [].slice.call(document.querySelectorAll('.toast'));
    var toastList = toastElList.map(function (toastEl) {
        return new window.bootstrap.Toast(toastEl, option);
    });
}

function showSaveChangesToast() {
    var toastTrigger = document.getElementById('saveChangesBtn');
    var toastLiveExample = document.getElementById('saveChangesToast');
    if (toastTrigger) {
        toastTrigger.addEventListener('click', function () {
            var toast = new bootstrap.Toast(toastLiveExample);

            toast.show();
        });
    }
}

window.SetFocusToElement = (element) => {
    element.focus();
}