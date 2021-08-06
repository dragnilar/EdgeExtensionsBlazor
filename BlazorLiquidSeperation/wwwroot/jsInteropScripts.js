window.setBackgroundImage1 = (url) => document.getElementsByTagName("body")[0].style =
    `background-image: url(${url})`;

window.runSearchQuery = (url) => chrome.search.query({text: url}, () => {
    console.log(url);
});