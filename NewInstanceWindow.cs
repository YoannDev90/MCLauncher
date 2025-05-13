using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CmlLib.Core;

namespace MCLauncher;

public class MinecraftVersion
{
    public string Name { get; set; } = "";
    public DateTime ReleaseDate { get; set; }
    public string Type { get; set; } = "";
}

public partial class NewInstanceWindow : Window
{
    private readonly MinecraftLauncher _launcher;
    private string _selectedLoader = "Vanilla";
    private MinecraftVersion? _selectedVersion;

    public NewInstanceWindow()
    {
        InitializeComponent();
        DataContext = this;
        _launcher = new MinecraftLauncher();

        OfficialCheckBox.Checked += (s, e) => FilterVersions();
        OfficialCheckBox.Unchecked += (s, e) => FilterVersions();
        SnapshotCheckBox.Checked += (s, e) => FilterVersions();
        SnapshotCheckBox.Unchecked += (s, e) => FilterVersions();
        OldCheckBox.Checked += (s, e) => FilterVersions();
        OldCheckBox.Unchecked += (s, e) => FilterVersions();
        SearchBox.KeyUp += (s, e) => FilterVersions();

        NoLoaderRadio.Checked += (s, e) => SetLoader("Vanilla");
        NeoForgeRadio.Checked += (s, e) => SetLoader("NeoForge");
        ForgeRadio.Checked += (s, e) => SetLoader("Forge");
        FabricRadio.Checked += (s, e) => SetLoader("Fabric");
        QuiltRadio.Checked += (s, e) => SetLoader("Quilt");
        LiteLoaderRadio.Checked += (s, e) => SetLoader("LiteLoader");

        OkButton.Click += OkButton_Click;
        CancelButton.Click += (s, e) => Close();
        RefreshButton.Click += RefreshButton_Click;

        _ = LoadVersionsAsync();
    }

    public ObservableCollection<MinecraftVersion> AllVersions { get; } = new();
    public ObservableCollection<MinecraftVersion> FilteredVersions { get; } = new();

    private void SetLoader(string loader)
    {
        _selectedLoader = loader;
    }

    private async void RefreshButton_Click(object? sender, RoutedEventArgs e)
    {
        await LoadVersionsAsync();
    }

    private async Task LoadVersionsAsync()
    {
        AllVersions.Clear();
        FilteredVersions.Clear();
        try
        {
            var versions = await _launcher.GetAllVersionsAsync();
            Debug.WriteLine($"Nombre de versions trouvées : {versions}");
            foreach (var v in versions)
                if (v.Type == "release" || v.Type == "snapshot" || v.Type == "old_alpha" || v.Type == "old_beta")
                {
                    var version = new MinecraftVersion
                    {
                        Name = v.Name,
                        ReleaseDate = v.ReleaseTime.UtcDateTime,
                        Type = v.Type
                    };
                    AllVersions.Add(version);
                    Debug.WriteLine($"Ajouté : {version.Name} ({version.Type})");
                }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Erreur lors du chargement : " + ex);
        }

        FilterVersions();
    }

    private void FilterVersions()
    {
        var showRelease = OfficialCheckBox.IsChecked ?? false;
        var showSnapshot = SnapshotCheckBox.IsChecked ?? false;
        var showOld = OldCheckBox.IsChecked ?? false;
        var search = SearchBox.Text?.ToLowerInvariant() ?? "";

        FilteredVersions.Clear();
        foreach (var v in AllVersions)
        {
            if (!string.IsNullOrWhiteSpace(search) && !v.Name.ToLowerInvariant().Contains(search))
                continue;

            var typeMatch =
                (showRelease && v.Type == "release") ||
                (showSnapshot && v.Type == "snapshot") ||
                (showOld && (v.Type == "old_alpha" || v.Type == "old_beta"));

            if (typeMatch)
                FilteredVersions.Add(v);
        }
    }

    private void VersionButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is MinecraftVersion version)
        {
            _selectedVersion = version;
            Debug.WriteLine($"Version sélectionnée : {version.Name}");
        }
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        var instanceName = InstanceNameBox.Text;
        var loader = _selectedLoader;

        if (_selectedVersion == null)
            return;

        // Traiter la création...
        Close();
    }
}