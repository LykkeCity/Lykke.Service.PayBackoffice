using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.Messages.Sms;
using Core.Settings;

namespace LkeServices.Messages.Sms
{	
	public class TwilioSmsSender : IAlternativeSmsSender
	{
		private readonly ILog _log;
		private readonly TwilioSettings _twilioSettings;
		private readonly TwilioRestClient _twilioRestClient;

		public TwilioSmsSender(TwilioSettings twilioSettings, ILog log)
		{
			_log = log;
			_twilioSettings = twilioSettings;
			_twilioRestClient = new TwilioRestClient(twilioSettings.AccountSid, twilioSettings.AuthToken);
		}

		public string GetSenderNumber(string recipientNumber)
		{
			return recipientNumber.IsUSCanadaNumber() ? _twilioSettings.UsNumber : _twilioSettings.SwissNumber;
		}

		public async Task ProcessSmsAsync(string phoneNumber, SmsMessage message)
		{
			var msg = await _twilioRestClient.SendMessage(message.From, phoneNumber, message.Text);

			if (!msg.Success)
				await _log.WriteWarningAsync("TwilioSmsSender", "ProcessSmsAsync", phoneNumber, msg.ErrorMesssage);
		}
	}

	internal class TwilioSmsCredentials
	{
		public string AccountSid { get; set; } = string.Empty;

		public string AuthToken { get; set; } = string.Empty;

		public string FromNumber { get; set; }


	}

	internal class TwilioRestClient
	{
		private readonly string _accountSid;
		private readonly string _authToken;

		public TwilioRestClient(string accountSid, string authToken)
		{
			_accountSid = accountSid;
			_authToken = authToken;
		}


		private const string TwilioSmsEndpointFormat = "https://api.twilio.com/2010-04-01/Accounts/{0}/Messages.json";

		/// <summary>
		/// Send an sms message using Twilio REST API
		/// </summary>
		/// <param name="from">from</param>
		/// <param name="toPhoneNumber">E.164 formatted phone number, e.g. +16175551212</param>
		/// <param name="message"></param>
		/// <returns></returns>
		public async Task<TwilioResponse> SendMessage(
			string from,
			string toPhoneNumber,
			string message)
		{
			if (string.IsNullOrWhiteSpace(toPhoneNumber))
			{
				throw new ArgumentException("toPhoneNumber was not provided");
			}

			if (string.IsNullOrWhiteSpace(message))
			{
				throw new ArgumentException("message was not provided");
			}

			var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = CreateBasicAuthenticationHeader(
				_accountSid,
				_authToken);

			var keyValues = new List<KeyValuePair<string, string>>();
			keyValues.Add(new KeyValuePair<string, string>("To", toPhoneNumber));
			keyValues.Add(new KeyValuePair<string, string>("From", from));
			keyValues.Add(new KeyValuePair<string, string>("Body", message));

			var content = new FormUrlEncodedContent(keyValues);

			var postUrl = string.Format(
					CultureInfo.InvariantCulture,
					TwilioSmsEndpointFormat,
					_accountSid);

			var response = await client.PostAsync(
				postUrl,
				content).ConfigureAwait(false);
			if (response.IsSuccessStatusCode)
			{
				return new TwilioResponse { Success = true };
			}
			else
			{
				string responseBody = await response.Content.ReadAsStringAsync();
				return new TwilioResponse
				{
					Success = false,
					ErrorMesssage = response.ReasonPhrase + " " + responseBody
				};
			}
		}



		private AuthenticationHeaderValue CreateBasicAuthenticationHeader(string username, string password)
		{
			return new AuthenticationHeaderValue(
				"Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password)))
			);
		}
	}

	internal class TwilioResponse
	{
		public bool Success { get; set; }

		public string ErrorMesssage { get; set; }
	}
}
