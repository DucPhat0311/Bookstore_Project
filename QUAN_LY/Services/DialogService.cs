// QUAN_LY/Services/DialogService.cs
using System.Threading.Tasks;
using System.Windows;

namespace QUAN_LY.Services
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmationAsync(string title, string message);
        Task ShowErrorAsync(string title, string message);
        Task ShowSuccessAsync(string title, string message);
        Task ShowInfoAsync(string title, string message);
    }

    public class DialogService : IDialogService
    {
        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var result = MessageBox.Show(message, title,
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            });
        }

        public async Task ShowErrorAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(message, title,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        public async Task ShowSuccessAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(message, title,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public async Task ShowInfoAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(message, title,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
    }
}