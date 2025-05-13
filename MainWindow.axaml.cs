using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MCLauncher;

public partial class MainWindow : Window
{
    //private readonly MinecraftLauncher launcher;

    public MainWindow()
    {
        InitializeComponent();
        //launcher = new MinecraftLauncher();
    }


    // private void OpenAzurhosts(object? sender, RoutedEventArgs e)
    // {
    //     var url = "https://azurhosts.com";
    //     var psi = new ProcessStartInfo(url) { UseShellExecute = true };
    //     Process.Start(psi);
    // }

    // TODO : Faire la demande de partenariat en ticket à Azurhosts, puis remettre le code ci-dessus

    private void OpenGitHub(object? sender, RoutedEventArgs e)
    {
        var url = "https://github.com/YoannDev90/MCLauncher";
        var psi = new ProcessStartInfo(url) { UseShellExecute = true };
        Process.Start(psi);
    }

    private void FoldersButton_Click(object? sender, RoutedEventArgs routedEventArgs)
    {
        StatusText.Text = "Ouverture du dossier Minecraft...";
    }

    private void NewInstanceButton_Click(object? sender, RoutedEventArgs e)
    {
        StatusText.Text = "Création d'une nouvelle instance...";
        var newInstanceWindow = new NewInstanceWindow();
        newInstanceWindow.Show();
    }

    private void AccountsButton_Click(object? sender, RoutedEventArgs e)
    {
        StatusText.Text = "Gestion des comptes...";
        var accountsWindow = new AccountsWindow();
        accountsWindow.Show();
    }

    private void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        StatusText.Text = "Ouverture des paramètres...";
        var settingsWindow = new SettingsWindow();
        settingsWindow.Show();
    }

    private void Launch(object? sender, RoutedEventArgs e)
    {
        StatusText.Text = "Lancement de l'instance";
    }

    private void Delete(object? sender, RoutedEventArgs e)
    {
        StatusText.Text = "Suppression de l'instance";
    }
}