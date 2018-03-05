using Core.Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;

namespace Core.Ethereum.Models
{
    public class EthereumTransactionModel
    {
        public string ToAddress { get; set; }
        public string FromAddress { get; set; }
        public BigInteger GasAmount { get; set; }
        public BigInteger GasPrice { get; set; }
        public double Value { get; set; } //in ether
        public Asset Asset { get; set; }
    }

    public class GetEthereumTransactionModel
    {
        public string FromAddress { get; set; }
        public string TransactionHexRaw { get; set; }
    }

    public class BroadcastEthereumTransactionModel
    {
        public string FromAddress { get; set; }
        public string TransactionHexRawSigned { get; set; }
    }

    public class GetTransactionHashModel
    {
        public string TransactionHash { get; set; }
    }

    public class GetEthereumTokenTransactionHistory : GetEthereumTransactionHistory
    {
        public string TokenAddress { get; set; }
    }

    public class GetEthereumTransactionHistory
    {
        public string Address { get; set; }
        public int? Start { get; set; }
        public int? Count { get; set; }
    }

    public class EthereumTransactionHistorical
    {
        public int TransactionIndex { get; set; }

        public long BlockNumber { get; set; }

        public string Gas { get; set; }

        public string GasPrice { get; set; }

        public string Value { get; set; }

        public string Nonce { get; set; }

        public string TransactionHash { get; set; }

        public string BlockHash { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        //Comes with erc 20
        //public string Input { get; set; }

        public int BlockTimestamp { get; set; }

        public string ContractAddress { get; set; }

        public string GasUsed { get; set; }

        public DateTime BlockTimeUtc { get; set; }
        public bool HasError { get; set; }

        public IEnumerable<EthereumAddressHistory> ErcTransfers { get;set; }
    }

    public class EthereumInternalMessageHistorical
    {
        public string TransactionHash { get; set; }
        public ulong BlockNumber { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public int Depth { get; set; }
        public string Value { get; set; }
        public int MessageIndex { get; set; }
        public string Type { get; set; }
        public uint BlockTimestamp { get; set; }
        public DateTime BlockTimeUtc { get; set; }
    }

    public class TransactionDetails
    {
        public EthereumTransactionHistorical Transaction { get; set; }
        public IEnumerable<EthereumInternalMessageHistorical> Messages { get; set; }
    }

    public class EthereumAddressHistory
    {
        public ulong BlockNumber { get; set; }

        public BigInteger Value { get; set; }

        public BigInteger GasUsed { get; set; }

        public BigInteger GasPrice { get; set; }

        public string TransactionHash { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public uint BlockTimestamp { get; set; }

        public bool HasError { get; set; }

        public int TransactionIndex { get; set; }

        public int MessageIndex { get; set; }

        public DateTime BlockTimeUtc { get; set; }

        public string ContractAddress { get; set; }
    }
}
