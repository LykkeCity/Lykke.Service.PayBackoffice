using System.Collections.Generic;

namespace BackOffice.Models
{
    public static class WebSiteConstants
    {
        public const string PersonalAreaDiv = "#pamain";

        public const string LykkePurple = "#c91ec9";
    }

    public interface IPersonalAreaDialog
    {
        string Caption { get; set; }
        string Width { get; set; }
    }

    public class OkDialogViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
    }

    public interface IFindClientViewModel
    {
        /// <summary>
        /// Where should I Do Search request
        /// </summary>
        string RequestUrl { get; }
        /// <summary>
        /// Where should I put result Html
        /// </summary>
        string Div { get; }

        string Value { get; }
    }

    public class AreaMenuItem
    {
        public string Caption { get; set; }
        public string Url { get; set; }

        public static AreaMenuItem Create(string caption, string url)
        {
            return new AreaMenuItem
            {
                Caption = caption,
                Url = url
            };
        }
    }

    public class AreaMenuViewModel
    {
        public List<AreaMenuItem> MenuItems = new List<AreaMenuItem>();


        public static AreaMenuViewModel Create(params AreaMenuItem[] menuItems)
        {
            var result = new AreaMenuViewModel();
            result.MenuItems.AddRange(menuItems);
            return result;
        }

    }

    public class ClientPartnerRelationIndexViewModel : IFindClientViewModel
    {
        public ClientPartnerRelationViewModel[] Relations { get; set; }
        public string Div { get; set; }
        public string RequestUrl { get; set; }
        public string Value { get; set; }
    }

    public class ClientPartnerRelationViewModel
    {
        public string Email { get; set; }
        public string ClientId { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
    }

}
