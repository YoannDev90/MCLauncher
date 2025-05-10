using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CmlLib.Core;

namespace MCLauncher;

public partial class MainWindow : Window
{
    private readonly MinecraftLauncher _launcher;
    private DateTime[] _allVersionDates = Array.Empty<DateTime>();
    private bool[] _allVersionIsAlpha = Array.Empty<bool>();
    private bool[] _allVersionIsBeta = Array.Empty<bool>();
    private bool[] _allVersionIsRelease = Array.Empty<bool>();
    private bool[] _allVersionIsSnapshot = Array.Empty<bool>();
    private string[] _allVersionNames = Array.Empty<string>();

    public MainWindow()
    {
        InitializeComponent();
    }


    private void OpenAzurhosts(object? sender, RoutedEventArgs e)
    {
        var url = "https://azurhosts.com";
        var psi = new ProcessStartInfo(url) { UseShellExecute = true };
        Process.Start(psi);
    }

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
}