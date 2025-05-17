using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace MCLauncher
{
    public partial class ConfirmationDialog : Window
    {
        public string WindowTitle { get; }
        public string MessageTitle { get; }
        public string MessageText { get; }
        public string AdditionalText { get; }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public ConfirmationDialog(string title, string message, string additionalText = "")
        {
            InitializeComponent();
            
            // Définir les propriétés pour le binding
            WindowTitle = title;
            MessageTitle = title;
            MessageText = message;
            AdditionalText = additionalText;
            
            DataContext = this;
            
            // Configurer les événements de bouton
            this.FindControl<Button>("ConfirmButton").Click += ConfirmButton_Click;
            this.FindControl<Button>("CancelButton").Click += CancelButton_Click;
        }

        private void ConfirmButton_Click(object? sender, RoutedEventArgs e)
        {
            // Fermer la fenêtre avec un résultat "true"
            Close(true);
        }
        
        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            // Fermer la fenêtre avec un résultat "false"
            Close(false);
        }
    }
}
