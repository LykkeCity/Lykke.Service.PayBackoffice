﻿namespace LkeServices.Messages.EmailTemplates.ViewModels
{
    public class BtcDepositDoneTempate
    {
        public string AssetName { get; set; }
        public double? Amount { get; set; }
        public string ExplorerUrl { get; set; }
        public int Year { get; set; }
        public int ValidDays { get; set; }
    }
}
