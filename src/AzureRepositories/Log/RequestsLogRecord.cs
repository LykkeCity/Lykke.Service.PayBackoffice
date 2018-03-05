using System;
using Core.EventLogs;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Log
{
    public class RequestsLogRecord : TableEntity, IRequestsLogRecord
    {
        public DateTime DateTime { get; set; }
        public string Url { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string UserAgent { get; set; }

        private const int MaxFieldSize = 1024 * 4;

        public static RequestsLogRecord Create(string userId, string url, string request, string response, string userAgent)
        {
            if (request?.Length > MaxFieldSize)
                request = request.Substring(0, MaxFieldSize);

            var dateTime = DateTime.UtcNow;

            return new RequestsLogRecord
            {
                PartitionKey = GeneratePartitionKey(userId),
                RowKey = GenerateRowKey(dateTime),
                Url = url,
                Request = request,
                Response = response,
                DateTime = dateTime,
                UserAgent = userAgent
            };
        }

        public static string GeneratePartitionKey(string userId)
        {
            return userId;
        }

        public static string GenerateRowKey(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        }

        internal static string GenerateRowKey(DateTime dateTime, int retryNumber, int itemNumber)
        {
            return $"{dateTime:yyyy-MM-dd HH:mm:ss.fffffff}.{retryNumber:000}.{itemNumber:00}";
        }
    }
}