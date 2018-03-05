﻿using System.Threading.Tasks;

namespace Core.BitCoin
{
    public interface ILastProcessedBlockRepository
    {
        Task InsertOrUpdateForClientAsync(string clientId, int blockHeight);
        Task<int> GetLastProcessedBlockHeightAsync(string clientId);
    }
}
