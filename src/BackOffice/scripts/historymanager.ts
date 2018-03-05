interface IHistoryState {
    type:string;
    div: string;
    url: string;
    title: string; 
}

class HistoryManager {
    

    put(state: IHistoryState): void {


        history.pushState(state, state.title, "#" + state.url);
    }

    back() {
        history.back();
    }

}


var historyManager = new HistoryManager();


window.addEventListener('popstate', e => {
    console.log(e);
    var state = <IHistoryState>(<any>e).state;

    requests.doRequest({ url: state.url, divResult: state.div });

    if (state.type === 'menu') {
      mainMenu.selectByUrl(state.url);
    }

});