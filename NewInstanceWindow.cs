using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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

        OfficialCheckBox.IsCheckedChanged += (s, e) => FilterVersions();
        SnapshotCheckBox.IsCheckedChanged += (s, e) => FilterVersions();
        OldCheckBox.IsCheckedChanged += (s, e) => FilterVersions();
        SearchBox.KeyUp += (s, e) => FilterVersions();

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
        VersionsListBox.SelectionChanged += VersionsListBox_SelectionChanged;

        _ = LoadVersionsAsync();
    }

    public ObservableCollection<MinecraftVersion> AllVersions { get; } = new();
    public ObservableCollection<MinecraftVersion> FilteredVersions { get; } = new();

    public event EventHandler<MinecraftInstance> InstanceCreated;

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
            var versions = await _launcher.GetAllVersionsAsync().ConfigureAwait(true);
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

    private void VersionsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (VersionsListBox.SelectedItem is MinecraftVersion version)
        {
            _selectedVersion = version;
            Debug.WriteLine($"Version sélectionnée : {version.Name}");
        }
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        var instanceName = InstanceNameBox.Text;

        if (string.IsNullOrWhiteSpace(instanceName))
        {
            Debug.WriteLine("Le nom de l'instance ne peut pas être vide");
            return;
        }

        if (_selectedVersion == null)
        {
            Debug.WriteLine("Aucune version sélectionnée");
            return;
        }

        var instance = new MinecraftInstance
        {
            Name = instanceName,
            Version = _selectedVersion.Name,
            Loader = _selectedLoader
        };

        instance.Path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MCLauncher",
            "instances",
            instance.Id);

        InstanceCreated?.Invoke(this, instance);

        Close();
    }
}