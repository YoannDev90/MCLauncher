using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MCLauncher;

public partial class EditInstanceDialog : Window
{
    public EditInstanceDialog(string currentName)
    {
        //InitializeComponent();

        // Initialiser avec le nom actuel
        var nameTextBox = this.FindControl<TextBox>("InstanceNameTextBox");
        nameTextBox.Text = currentName;

        // Configurer les événements de bouton
        this.FindControl<Button>("SaveButton").Click += SaveButton_Click;
        this.FindControl<Button>("CancelButton").Click += CancelButton_Click;
    }

    private void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        var nameTextBox = this.FindControl<TextBox>("InstanceNameTextBox");
        var newName = nameTextBox.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(newName))
            // Afficher un message d'erreur si le nom est vide
            // Pour simplifier, nous permettons simplement aux utilisateurs de ne pas fermer la fenêtre
            return;

        // Fermer la fenêtre et retourner le nouveau nom
        Close(newName);
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        // Fermer la fenêtre sans retourner de valeur
        Close();
    }
}