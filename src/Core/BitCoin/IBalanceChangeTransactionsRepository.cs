﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Bitcoin;

namespace Core.BitCoin
{
    public interface IBalanceChangeTransaction : IBlockchainTransaction
    {
        string ClientId { get; set; }
        string Multisig { get; set; }
        DateTime DetectDt { get; }
    }

    public class BalanceChangeTransaction : IBalanceChangeTransaction
    {
        public static IBalanceChangeTransaction Create(IBlockchainTransaction blockchainTx, string clientId, string multisig)
        {
            return new BalanceChangeTransaction
            {
                ClientId = clientId,
                Confirmations = blockchainTx.Confirmations,
                Hash = blockchainTx.Hash,
                ReceivedCoins = blockchainTx.ReceivedCoins,
                SpentCoins = blockchainTx.SpentCoins,
                Multisig = multisig,
                BlockId = blockchainTx.BlockId,
                Height = blockchainTx.Height,
                DetectDt = DateTime.UtcNow
            };
        }

        public string Hash { get; set; }
        public int Confirmations { get; set; }
        public InputOutput[] ReceivedCoins { get; set; }
        public InputOutput[] SpentCoins { get; set; }
        public string BlockId { get; set; }
        public int Height { get; set; }
        public string ClientId { get; set; }
        public string Multisig { get; set; }
        public DateTime DetectDt { get; set; }
    }

    public interface IBalanceChangeTransactionsRepository
    {
        Task<bool> InsertIfNotExistsAsync(IBalanceChangeTransaction balanceChangeTransaction);
        Task<IEnumerable<IBalanceChangeTransaction>> GetAsync(string hash);
        Task<IBalanceChangeTransaction> GetAsync(string hash, string clientId);
    }
}
