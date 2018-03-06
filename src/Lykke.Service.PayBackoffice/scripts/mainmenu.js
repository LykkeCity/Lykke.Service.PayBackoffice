var MainManu = (function () {
    function MainManu() {
    }
    MainManu.prototype.selectMenu = function (newElement) {
        this.deselectMenu();
        this.currentItem = newElement;
        newElement.addClass('smiSel');
    };
    MainManu.prototype.selectByUrl = function (url) {
        var element = $('li[data-url="' + url + '"]');
        this.selectMenu(element);
    };
    MainManu.prototype.deselectMenu = function () {
        if (this.currentItem) {
            this.currentItem.removeClass('smiSel');
            this.currentItem = undefined;
        }
    };
    return MainManu;
}());
var mainMenu = new MainManu();
//# sourceMappingURL=mainmenu.js.map