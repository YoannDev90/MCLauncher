using Avalonia.Controls;
using System.Threading.Tasks;

namespace MCLauncher;

public static class MessageBoxManager
{
    /// <summary>
    /// Affiche une boîte de dialogue d'information
    /// </summary>
    public static async Task ShowInfoAsync(Window parent, string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var mainPanel = new StackPanel { Spacing = 20, Margin = new Avalonia.Thickness(20) };

        mainPanel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });

        var buttonOk = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Width = 80
        };
        buttonOk.Click += (s, e) => dialog.Close();
        mainPanel.Children.Add(buttonOk);

        dialog.Content = mainPanel;
        await dialog.ShowDialog(parent);
    }

    /// <summary>
    /// Affiche une boîte de dialogue d'erreur
    /// </summary>
    public static Task ShowErrorAsync(Window parent, string title, string message)
    {
        return ShowInfoAsync(parent, title, message); // Version simplifiée pour éviter les erreurs
    }
}
