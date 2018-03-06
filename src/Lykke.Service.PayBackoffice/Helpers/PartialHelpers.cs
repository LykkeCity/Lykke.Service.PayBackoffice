using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackOffice.Models;
using Core.Clients;
using Core.PaymentSystems;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BackOffice.Helpers
{
    public static class PartialHelpers
    {
        public static IHtmlContent RenderRating(this IHtmlHelper helper, int value, int maxValue)
        {
            return helper.Partial("~/Views/Helpers/RenderRating.cshtml", Tuple.Create(value, maxValue));
        }

        public static IHtmlContent RenderSaveCancelButtonPair(this IHtmlHelper helper, string url, string formId, string savePhrase = null, string onFinishJs = null, string onValidateJs = null)
        {
            return helper.Partial("~/Views/Helpers/RenderSaveCancelButtonPair.cshtml", new SaveCancelButtonPairModel { Url = url, FormId = formId, SavePhrase = savePhrase, OnFinish = onFinishJs, OnValidate = onValidateJs });
        }

        public static IHtmlContent RenderDeleteCancelButtonPair(this IHtmlHelper helper, string url, string formId, string deletePhrase = null)
        {
            return helper.Partial("~/Views/Helpers/RenderDeleteCancelButtonPair.cshtml", new SaveCancelButtonPairModel { Url = url, FormId = formId, SavePhrase = deletePhrase });
        }

        public static IHtmlContent RenderSaveCancelButtonMobilePair(this IHtmlHelper helper, string url, string formId, string savePhrase = null)
        {
            return helper.Partial("~/Views/Helpers/RenderSaveCancelButtonMobilePair.cshtml", new SaveCancelButtonPairModel { Url = url, FormId = formId, SavePhrase = savePhrase });
        }

        public static IHtmlContent RenderOkDialogButton(this IHtmlHelper helper)
        {
            return helper.Partial("~/Views/Helpers/RenderOkDialogButton.cshtml");
        }

        public static IHtmlContent RenderInputWithLabelOnTop(this IHtmlHelper helper, string name, string caption, string value = null, string placeHolder = null, string type = null, bool lg = true, bool focused = false)
        {
            return helper.Partial("~/Views/Helpers/RenderInputWithLabelOnTop.cshtml", new InputWithLabelModel { Name = name, Caption = caption, Value = value, Placeholder = placeHolder, Type = type, Lg = lg, Focused = focused });
        }

        public static IHtmlContent RenderDateSelectorWithLabelOnTop(this IHtmlHelper helper, string name, string caption, string value = null, string placeHolder = null, string type = null, bool lg = true, bool focused = false)
        {
            return helper.Partial("~/Views/Helpers/RenderDateSelectorWithLabelOnTop.cshtml", new InputWithLabelModel { Name = name, Caption = caption, Value = value, Placeholder = placeHolder, Type = type, Lg = lg, Focused = focused });
        }

        public static IHtmlContent RenderDatePickerWithLabelOnTop(this IHtmlHelper helper, string name, string caption, string value = null, string placeHolder = null, string type = null, bool lg = true, bool focused = false)
        {
            return helper.Partial("~/Views/Helpers/RenderDatePickerWithLabelOnTop.cshtml", new InputWithLabelModel { Name = name, Caption = caption, Value = value, Placeholder = placeHolder, Type = type, Lg = lg, Focused = focused });
        }

        public static IHtmlContent RenderTextAreaWithLabelOnTop(this IHtmlHelper helper, string name, string caption, string value = null, string placeHolder = null, string type = null, bool lg = true, bool focused = false)
        {
            return helper.Partial("~/Views/Helpers/RenderTextAreaWithLabelOnTop.cshtml", new InputWithLabelModel { Name = name, Caption = caption, Value = value, Placeholder = placeHolder, Type = type, Lg = lg, Focused = focused });
        }

        public static IHtmlContent RenderInputWithLabelOnTopReadOnly(this IHtmlHelper helper, string caption, string value, string type = null, string name = null)
        {
            return helper.Partial("~/Views/Helpers/RenderInputWithLabelOnTopReadOnly.cshtml", new InputWithLabelModel { Name = name, Caption = caption, Value = value });
        }

        public static IHtmlContent RenderSelect(this IHtmlHelper helper, string name, string caption, IEnumerable<string> ids, Func<string, string> getValue = null, string currentId = null, bool hasOptional = false, string optionalText = null, bool sortedByValue = false)
        {
            if (sortedByValue)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach(string id in ids)
                {
                    dict.Add(id, getValue(id));
                }
                ids = dict.OrderBy(_ => _.Value).Select(_ => _.Key).ToList();
            }
            return helper.Partial("~/Views/Helpers/RenderSelect.cshtml", new SelectModel { Name = name, Caption = caption, Ids = ids, GetValue = getValue, CurrentId = currentId, HasOptional = hasOptional, OptionalText = optionalText});
        }

        public static IHtmlContent RenderSelectFromEnum(this IHtmlHelper helper, string name, string caption, Type enumType, Enum currentValue, bool hasOptional = false)
        {
            return helper.Partial("~/Views/Helpers/RenderSelectForEnum.cshtml", new SelectFromEnumModel { Name = name, Caption = caption, EnumType = enumType, CurrentValue = currentValue, HasOptional = hasOptional });
        }

        public static IHtmlContent RenderSelectFromBool(this IHtmlHelper helper, string name, string caption, bool? currentValue, bool hasOptional = false)
        {
            return helper.Partial("~/Views/Helpers/RenderSelectForBool.cshtml", new SelectFromBoolModel { Name = name, Caption = caption, CurrentValue = currentValue, HasOptional = hasOptional });
        }

        public static IHtmlContent RenderCountryFlag(this IHtmlHelper helper, string countryCode, string textColor = null)
        {
            CountryFlagModel model = new CountryFlagModel();
            model.CountryCode = countryCode;
            model.TextColor = textColor;
            return helper.Partial("~/Views/Helpers/RenderCountryFlag.cshtml", model);
        }

        public static IHtmlContent RenderCaptionValue(this IHtmlHelper helper, string caption, string value)
        {
            return helper.Partial("~/Views/Helpers/RenderCaptionValue.cshtml", Tuple.Create(caption, value));
        }

        public static IHtmlContent RenderHiddenId(this IHtmlHelper helper, string id, string fieldName = "Id")
        {
            return helper.Partial("~/Views/Helpers/RenderHiddenId.cshtml", Tuple.Create(id, fieldName));
        }

        public static IHtmlContent RenderMultiselect(this IHtmlHelper helper, string caption, string name, IEnumerable<KeyValuePair<string, string>> items, string[] selected, string height = "200px")
        {
            return helper.Partial("~/Views/Helpers/RenderMultiselect.cshtml", new MultiSelectModel { Name = name, Caption = caption, Items = items, Selected = selected, Height = height });
        }

        public static IHtmlContent RenderSelect2(this IHtmlHelper helper, string caption, string name, IEnumerable<ItemViewModel> items, bool multi, string placeholder, bool allowClear = false)
        {
            return helper.Partial("~/Views/Helpers/RenderSelect2.cshtml", new Select2Model { Name = name, Caption = caption, Items = items, Multi = multi, Placeholder = placeholder, AllowClear = allowClear });
        }

        public static IHtmlContent RenderCheckbox(this IHtmlHelper helper, string name, string caption, bool isSelected, string value = null)
        {
            return helper.Partial("~/Views/Helpers/RenderCheckbox.cshtml", new CheckboxModel { Name = name, Caption = caption, IsSelected = isSelected, Value = value });
        }

        public static IHtmlContent BtnCreate(this IHtmlHelper helper, string url)
        {
            return helper.Partial("~/Views/Helpers/BtnCreate.cshtml", url);
        }

        public static IHtmlContent BtnEditById(this IHtmlHelper helper, string url, string id = null)
        {
            return helper.Partial("~/Views/Helpers/BtnEditById.cshtml", Tuple.Create(url, id));
        }
        public static IHtmlContent BtnEditByValue(this IHtmlHelper helper, string url, string value, string valueFieldName)
        {
            return helper.Partial("~/Views/Helpers/BtnEditByValue.cshtml", new EditByValueModel { Url = url, Value = value, ValueFieldName = valueFieldName } );
        }

        public static IHtmlContent RenderPaymentStatusIcon(this IHtmlHelper helper, PaymentStatus ps)
        {
            return helper.Partial("~/Views/Helpers/RenderPaymentStatusIcon.cshtml", ps);
        }

        public static IHtmlContent RenderCashInPaymentSystemIcon(this IHtmlHelper helper, CashInPaymentSystem ps)
        {
            return helper.Partial("~/Views/Helpers/RenderCashInPaymentSystemIcon.cshtml", ps);
        }

        public static IHtmlContent RenderFoundOtherClients(this IHtmlHelper helper, IPersonalData pd)
        {
            var searchData = pd as SearchPersonalDataModel;

            if (searchData?.OtherClients == null || !searchData.OtherClients.Any())
                return HtmlString.Empty;

            return helper.Partial("PartialFoundOtherClients", new FoundOtherClientViewModel
            {
                PersonalData = searchData
            });
        }
    }

    public class EditByValueModel
    {
        public string Url { get; set; }
        public string Value { get; set; }
        public string ValueFieldName { get; set; }
    }

    public class SaveCancelButtonPairModel
    {
        public string Url { get; set; }
        public string FormId { get; set; }
        public string SavePhrase { get; set; }
        public string OnFinish { get; set; }
        public string OnValidate { get; set; }
    }

    public class InputWithLabelModel
    {
        public string Name { get; set; }
        public string Caption { get; set; }
        public string Value { get; set; }
        public string Placeholder { get; set; }
        public string Type { get; set; }
        public bool Lg { get; set; }
        public bool Focused { get; set; }
    }

    public class SelectModel
    {
        public string Name { get; set; }
        public string Caption { get; set; }
        public IEnumerable<string> Ids { get; set; }
        public Func<string, string> GetValue { get; set; }
        public string CurrentId { get; set; }
        public bool HasOptional { get; set; }
        public string OptionalText { get; set; }
    }

    public class SelectFromEnumModel
    {
        public string Name { get; set; }
        public string Caption { get; set; }
        public Type EnumType { get; set; }
        public Enum CurrentValue { get; set; }
        public bool HasOptional { get; set; }
    }

    public class SelectFromBoolModel
    {
        public string Name { get; set; }
        public string Caption { get; set; }
        public bool? CurrentValue { get; set; }
        public bool HasOptional { get; set; }
    }

    public class MultiSelectModel
    {
        public string Caption { get; set; }
        public string Name { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Items { get; set; }
        public string[] Selected { get; set; }
        public string Height { get; set; }
    }
    
    public class Select2Model
    {
        public string Caption { get; set; }
        public string Name { get; set; }
        public IEnumerable<ItemViewModel> Items { get; set; }
        public bool Multi { get; set; }
        public string Placeholder { get; set; }
        public bool AllowClear { get; set; }
    }
    
    public class CheckboxModel
    {
        public string Caption { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public string Value { get; set; }
    }

    public class CountryFlagModel
    {
        public string CountryCode { get; set; }
        public string TextColor { get; set; }
    }

}
