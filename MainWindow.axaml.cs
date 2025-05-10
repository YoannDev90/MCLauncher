using System;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.ProcessBuilder;

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
        _launcher = new MinecraftLauncher(new MinecraftPath("./minecraft"));
        LoadVersionsAsync();

        RamSlider.PropertyChanged += (s, e) =>
        {
            if (e.Property == Slider.ValueProperty)
            {
                var value = (int)RamSlider.Value;
                RamSlider.Value = value;
            }
        };

        // Gestion du changement des CheckBox
        ReleaseCheckBox.IsCheckedChanged += (s, e) => UpdateVersionList();
        SnapshotCheckBox.IsCheckedChanged += (s, e) => UpdateVersionList();
        AlphaCheckBox.IsCheckedChanged += (s, e) => UpdateVersionList();
        BetaCheckBox.IsCheckedChanged += (s, e) => UpdateVersionList();

        LaunchButton.Click += LaunchButton_Click;
        RamSlider.PropertyChanged += (s, e) => RamValue.Text = RamSlider.Value.ToString();
    }

    private async void LoadVersionsAsync()
    {
        var versions = await _launcher.GetAllVersionsAsync();
        _allVersionNames = versions.Select(v => v.Name).ToArray();
        _allVersionIsRelease = versions.Select(v => v.Type == "release").ToArray();
        _allVersionIsSnapshot = versions.Select(v => v.Type == "snapshot").ToArray();
        _allVersionIsAlpha = versions.Select(v => v.Type == "old_alpha").ToArray();
        _allVersionIsBeta = versions.Select(v => v.Type == "old_beta").ToArray();
        _allVersionDates = versions.Select(v => v.ReleaseTime.DateTime).ToArray();
        UpdateVersionList();
    }

    private void UpdateVersionList()
    {
        var showRelease = ReleaseCheckBox.IsChecked ?? false;
        var showSnapshot = SnapshotCheckBox.IsChecked ?? false;
        var showAlpha = AlphaCheckBox.IsChecked ?? false;
        var showBeta = BetaCheckBox.IsChecked ?? false;

        var filteredVersions = _allVersionNames
            .Select((name, index) => new
            {
                Name = name,
                Type = _allVersionIsRelease[index] ? "release" :
                    _allVersionIsSnapshot[index] ? "snapshot" :
                    _allVersionIsBeta[index] ? "beta" :
                    _allVersionIsAlpha[index] ? "alpha" : "unknown",
                Date = _allVersionDates[index]
            })
            .Where(v => (showRelease && v.Type == "release") ||
                        (showSnapshot && v.Type == "snapshot") ||
                        (showBeta && v.Type == "beta") ||
                        (showAlpha && v.Type == "alpha"))
            .OrderBy(v => v.Type == "release" ? 0 : v.Type == "snapshot" ? 1 : v.Type == "beta" ? 2 : 3)
            .ThenByDescending(v => v.Date)
            .ToList();

        VersionComboBox.Items.Clear();
        foreach (var v in filteredVersions) VersionComboBox.Items.Add(v.Name);
        if (VersionComboBox.Items.Count > 0)
            VersionComboBox.SelectedIndex = 0;
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

    private void NewInstanceButton_Click(object? sender, RoutedEventArgs e)
    {
        StatusText.Text = "Création d'une nouvelle instance...";
        // Ouvre une nouvelle fenêtre pour créer une instance
    }


    private void FoldersButton_Click(object? sender, RoutedEventArgs routedEventArgs)
    {
        // Ici, ajoute la logique pour ouvrir le dossier des instances
        StatusText.Text = "Ouverture du dossier Minecraft...";
        // Ouvre le dossier .Minecraft
    }

    private void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        // Ici, ajoute la logique pour ouvrir les paramètres
        StatusText.Text = "Ouverture des paramètres...";
        // Ouvre une nouvelle fenêtre pour les paramètres
    }

    private void AccountsButton_Click(object? sender, RoutedEventArgs e)
    {
        // Ici, ajoute la logique pour gérer les comptes
        StatusText.Text = "Gestion des comptes...";
        // Ouvre une nouvelle fenêtre pour gérer les comptes
    }

    private async void LaunchButton_Click(object? sender, RoutedEventArgs e)
    {
        if (VersionComboBox.SelectedItem == null) return;
        var version = VersionComboBox.SelectedItem.ToString();
        var username = UsernameBox.Text ?? "Player";
        var ram = (int)RamSlider.Value;

        StatusText.Text = "Installation en cours...";

        try
        {
            await _launcher.InstallAsync(version!);
            var process = await _launcher.BuildProcessAsync(version!, new MLaunchOption
            {
                Session = MSession.CreateOfflineSession(username),
                MaximumRamMb = ram
            });
            process.Start();
            StatusText.Text = "Minecraft lancé !";
        }
        catch (Exception ex)
        {
            StatusText.Text = "Erreur : " + ex.Message;
        }
    }
}