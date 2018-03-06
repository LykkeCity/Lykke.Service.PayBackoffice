class MainManu {

    private currentItem: JQuery;

    selectMenu(newElement: JQuery) {
        this.deselectMenu();

        this.currentItem = newElement;
        newElement.addClass('smiSel');
    }

    selectByUrl(url: string) {
        var element = $('li[data-url="'+url+'"]');
        this.selectMenu(element);
    }

    deselectMenu() {
        if (this.currentItem) {
            this.currentItem.removeClass('smiSel');
            this.currentItem = undefined;
        }
    }

}

var mainMenu = new MainManu();