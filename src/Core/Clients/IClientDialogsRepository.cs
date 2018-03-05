using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Core.Clients
{
    public enum DialogType
    {
        Info,
        Warning,
        Question
    }

    public class ClientDialogDto : IClientDialog
    {
        public Guid Id { get; set; }
        public DialogType Type { get; set; }
        public string Caption { get; set; }
        public string Text { get; set; }
        public ClientDialogButton[] Buttons { get; set; } = Array.Empty<ClientDialogButton>();
        public string ClientId { get; set; }
    }

    public class ClientDialogButton
    {
        public Guid Id { get; set; }
        public bool PinRequired { get; set; }
        public string Text { get; set; }
    }

    public interface IClientDialog
    {
        Guid Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        DialogType Type { get; set; }

        string Caption { get; set; }

        string Text { get; set; }

        string ClientId { get; set; }

        ClientDialogButton[] Buttons { get; set; }
    }

    public interface IClientDialogsRepository
    {
        Task AddDialogAsync(IClientDialog clientDialog);
        Task SubmitDialogAsync(string clientId, Guid dialogId, Guid buttonId);
        Task DeleteDialogAsync(string clientId, Guid dialogId);
        Task<IClientDialog> GetDialogAsync(string clientId, Guid dialogId);
        Task<IEnumerable<IClientDialog>> GetDialogsAsync(string clientId);
        Task<bool> IsSubmitted(string clientId, Guid dialogId, Guid buttonId);
    }
}
