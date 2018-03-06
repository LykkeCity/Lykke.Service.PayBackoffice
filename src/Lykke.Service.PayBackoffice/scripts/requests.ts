/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/bootstrap/bootstrap.d.ts" />
/// <reference path="typings/lib.d.ts" />
/// <reference path="ui.ts" />

function getCookie(cName: string): string {
    if (document.cookie.length > 0) {
        let cStart = document.cookie.indexOf(cName + "=");
        if (cStart !== -1) {
            cStart = cStart + cName.length + 1;
            let cEnd = document.cookie.indexOf(";", cStart);
            if (cEnd === -1) {
                cEnd = document.cookie.length;
            }
            return decodeURIComponent(document.cookie.substring(cStart, cEnd));
        }
    }
    return "";
}

function getAntiForgeryCookie(): string {
    return getCookie("XSRF-TOKEN");
}

interface IRequestData {
    url: string;
    requestUrl?: string;
    formId?: string;
    params?: any;
    requestParams?: any;
    onStart?: () => void;
    onFinish?: () => void;
    divResult?: string;
    divError?: string;
    placement?: string;
    callBack?: (result?) => void;
    replaceDiv?: boolean;

    hideSocial?: boolean;

    putToHistory?: boolean;

    onShowLoading?: () => void;
    onHideLoading?: () => void;
    showLoading?: boolean;
    notToHistory?: boolean;

    putToLast?: boolean;
    partialUpdate?: boolean;
}




class Requests {
    public static pageManager: IPageManager;
    public static ui: IUi;

    public static last: () => void;

    private static reloadAction = {
        reloadPage: true,
        url: "",
        data: undefined,
        div: "",
        showLoading: true
    }

    public static doRequest(o: IRequestData) {

        if (!o.url) {
            throw "url is empty";
        }

        o.params = o.formId ? $(o.formId).serialize() : o.params;


        if (o.putToLast)
            this.setRefreshUrl(o);

        if (o.partialUpdate)
            this.setUpdateInfo(o);

        if (o.onStart)
            o.onStart();

        this.blockOnRequest();

        if (o.divResult) {
            $(o.divResult).fadeOut(200, () => this.peformRequest(o));
        } else
            this.peformRequest(o);

    }

    static setRefreshAsReloadPage() {
        this.reloadAction.reloadPage = true;
    }

    static setRefreshUrl(r: IRequestData) {
        this.reloadAction.reloadPage = false;
        this.reloadAction.div = r.divResult;
        this.reloadAction.url = r.url;
        this.reloadAction.data = r.params;
        this.reloadAction.showLoading = r.showLoading;
    }

    static setUpdateInfo(r: IRequestData) {
        this.reloadAction.reloadPage = false;
        this.reloadAction.div = r.divResult;
        this.reloadAction.url = r.requestUrl;
        this.reloadAction.data = r.requestParams;
        this.reloadAction.showLoading = r.showLoading;
    }

    static showLoadingInDiv(id: string) {
        $(id).html('<div style="text-align:center; margin-top:20px;"><img src="/images/loading-pa.gif"/></div>');
        $(id).show();
    }

    private static peformRequest(o: IRequestData) {

        if (o.onShowLoading)
            o.onShowLoading();
        else {
            if (o.showLoading !== false && o.divResult) {
                this.showLoadingInDiv(o.divResult);
            }
        }

        var options = { url: o.url, type: 'POST', data: o.params, headers: { 'RequestVerificationToken': getAntiForgeryCookie() } };
        $.ajax(options)
            .then(result => {
                if (o.onHideLoading)
                    o.onHideLoading();
                if (o.onFinish)
                    o.onFinish();

                if (o.divResult)
                    $(o.divResult).hide();

                this.unBlockOnRequest();
                this.requestOkHandler(o, result);
            })
            .fail(jqXhr => {
                if (o.onHideLoading)
                    o.onHideLoading();
                if (o.onFinish)
                    o.onFinish();
                this.unBlockOnRequest();
                this.requestFailHandle(o, jqXhr.responseText, jqXhr);
            });

    }

    public static requestOkHandler(o: IRequestData, result) {


        if (result.status === 'Fail') {

            if (result.divError)
                o.divError = result.divError;

            o.placement = result.placement;

            this.requestFailHandle(o, result.msg);
            return;
        }

        if (result.status === "Reload") {
            this.ui.hideDialog();
            if (this.reloadAction.reloadPage)
                location.reload();
            else
                this.doRequest({ url: this.reloadAction.url, params: this.reloadAction.data, divResult: this.reloadAction.div, showLoading: this.reloadAction.showLoading });

            return;
        }


        if (result.refreshUrl) {
            this.ui.hideDialog();
            this.doRequest({ url: result.refreshUrl, params: result.prms, divResult: result.div, putToHistory: result.putToHistory });
            return;
        }

        if (result.inputId) {
            $(result.inputId).val(result.text);
            return;
        }

        if (result.status === "Request") {
            this.doRequest({ url: result.url, params: result.prms, putToHistory: result.putToHistory });
            return;
        }

        if (result.status === "refreshLast") {
            this.last();
            return;
        }


        if (result.status === "OkAndNothing") {
            if (o.callBack)
                o.callBack(result);
            return;
        }

        if (result.status === "ShowDialog") {
            this.ui.hideDialog(() => {
                this.ui.showDialog(result.url, result.prms);
            });
        }

        if (result.status === "Redirect") {
            if (result.Params) {
                window.location = <any>(result.Url + "?" + result.Params);
            } else
                window.location = result.Url;
            return;
        }





        if (o.divResult) {

            if (o.replaceDiv) {
                $(o.divResult).replaceWith(result);
            } else {
                $(o.divResult).html(result);

                $(o.divResult).fadeIn(200, () => {
                    this.pageManager.ressize();
                });
            }
            this.pageManager.ressize();

            this.ui.initFocus();

        }

    }


    public static handleErrorReq(o, result) {

        if (result.status === "Fail")
            this.ui.showError(result.divError, result.Result, undefined, result.Placement);

        else if (result.responseText && o.divError)
            this.ui.showError(o.divError, result.responseText);
    }

    public static requestFailHandle(o: IRequestData, text, jqXhr?) {
        if (jqXhr && jqXhr.status === 403) {
            if (o.divResult) {
                $(o.divResult).html('<div class="alert alert-danger" style="text-align:center; margin-top:20px;"><h1>Access denied</h1>You don\'t have an access to visit the ' + o.url + ' page.</div>');
            }
        } else {
            this.ui.showError(o.divError, text, undefined, o.placement);
        }
    }

    public static blockOnRequest() {

        $('.disableOnRequest').each(function () {
            $(this).attr('disabled', 'true');
        });

        $('.hideOnRequest').each(function () {
            $(this).css('display', 'none');
        });

        $('.showOnRequest').each(function () {
            $(this).css('display', 'inline');
        });
    }

    public static unBlockOnRequest() {
        $('.disableOnRequest').each(function () {
            $(this).removeAttr('disabled');
        });

        $('.hideOnRequest').each(function () {
            $(this).css('display', 'inline');
        });

        $('.showOnRequest').each(function () {
            $(this).css('display', 'none');
        });
    }

}

var requests = Requests;


