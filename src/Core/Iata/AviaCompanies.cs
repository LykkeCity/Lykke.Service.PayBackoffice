using System.Collections.Generic;

namespace Core.Iata
{
    public class AviaCompany
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public static AviaCompany Create(string id, string name)
        {
            return new AviaCompany
            {
                Id = id,
                Name = name
            };
        }
    }


    public static class AviaCompanies
    {
        public static readonly Dictionary<string, AviaCompany> Data = new Dictionary<string, AviaCompany>
        {
            ["BA"] = AviaCompany.Create("BA", "British Airways"),
            ["IT"] = AviaCompany.Create("IT", "IATA"),
            ["QR"] = AviaCompany.Create("QR", "Qatar"),
            ["DL"] = AviaCompany.Create("DL", "Delta Air Lines"),
            ["EK"] = AviaCompany.Create("EK", "Emirates"),
        }; 
    }
}
