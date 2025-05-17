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

        // Replace deprecated Checked/Unchecked events with IsCheckedChanged
        OfficialCheckBox.IsCheckedChanged += (s, e) => FilterVersions();
        SnapshotCheckBox.IsCheckedChanged += (s, e) => FilterVersions();
        OldCheckBox.IsCheckedChanged += (s, e) => FilterVersions();
        SearchBox.KeyUp += (s, e) => FilterVersions();

        // Replace deprecated Checked events with IsCheckedChanged
        NoLoaderRadio.IsCheckedChanged += (s, e) =>
        {
            if (NoLoaderRadio.IsChecked == true) SetLoader("Vanilla");
        };
        NeoForgeRadio.IsCheckedChanged += (s, e) =>
        {
            if (NeoForgeRadio.IsChecked == true) SetLoader("NeoForge");
        };
        ForgeRadio.IsCheckedChanged += (s, e) =>
        {
            if (ForgeRadio.IsChecked == true) SetLoader("Forge");
        };
        FabricRadio.IsCheckedChanged += (s, e) =>
        {
            if (FabricRadio.IsChecked == true) SetLoader("Fabric");
        };
        QuiltRadio.IsCheckedChanged += (s, e) =>
        {
            if (QuiltRadio.IsChecked == true) SetLoader("Quilt");
        };
        LiteLoaderRadio.IsCheckedChanged += (s, e) =>
        {
            if (LiteLoaderRadio.IsChecked == true) SetLoader("LiteLoader");
        };

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

        Close();
    }
}