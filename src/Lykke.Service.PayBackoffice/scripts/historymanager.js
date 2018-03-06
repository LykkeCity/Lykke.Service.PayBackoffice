var HistoryManager = (function () {
    function HistoryManager() {
    }
    HistoryManager.prototype.put = function (state) {
        history.pushState(state, state.title, "#" + state.url);
    };
    HistoryManager.prototype.back = function () {
        history.back();
    };
    return HistoryManager;
}());
var historyManager = new HistoryManager();
window.addEventListener('popstate', function (e) {
    console.log(e);
    var state = e.state;
    requests.doRequest({ url: state.url, divResult: state.div });
    if (state.type === 'menu') {
        mainMenu.selectByUrl(state.url);
    }
});
//# sourceMappingURL=historymanager.js.map