using System;

namespace BackOffice.Models
{
    public class ErrorDialogViewModel : DialogViewModel
    {
        public ErrorDialogViewModel()
        {
        }

        public ErrorDialogViewModel(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public string Message { get; set; }
    }
}
