﻿using System;
using Common;
using Core.CashOperations;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.CashTransfers
{
    public abstract class CashOutBaseEntity : TableEntity, ICashOutRequest
    {
        public string Id => RowKey;
        public string ClientId { get; set; }
        public string AssetId { get; set; }
        public string PaymentSystem { get; set; }
        public string PaymentFields { get; set; }
        public string BlockchainHash { get; set; }
        public string TradeSystem { get; set; }
        CashOutRequestTradeSystem ICashOutRequest.TradeSystem => TradeSystem.ParseEnum(CashOutRequestTradeSystem.Spot);
        public string AccountId { get; set; }

        public CashOutRequestStatus Status
        {
            get { return (CashOutRequestStatus)StatusVal; }
            set { StatusVal = (int) value; }
        }
        
        public TransactionStates State
        {
            get { return (TransactionStates)StateVal; }
            set { StateVal = (int)value; }
        }
        
        public double Amount { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsHidden { get; set; }

        public int StatusVal { get; set; }
        public int StateVal { get; set; }
        public CashOutVolumeSize VolumeSize { get; set; }
        public string VolumeText
        {
            get { return VolumeSize.ToString(); }
            set
            {
                CashOutVolumeSize volumeSize;
                if (Enum.TryParse(value, out volumeSize))
                    VolumeSize = volumeSize;
                else
                    VolumeSize = CashOutVolumeSize.Unknown;
            }
        }
        public string PreviousId { get; set; }
    }
}
