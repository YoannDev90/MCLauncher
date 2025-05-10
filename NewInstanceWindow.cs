using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MCLauncher;

public partial class NewInstanceWindow : Window
{
    private string _selectedLoader = "Vanilla";

    public NewInstanceWindow()
    {
        InitializeComponent();

        LoadVersions();
        // Filtres
        OfficialCheckBox.Checked += (s, e) => FilterVersions();
        OfficialCheckBox.Unchecked += (s, e) => FilterVersions();
        SnapshotCheckBox.Checked += (s, e) => FilterVersions();
        SnapshotCheckBox.Unchecked += (s, e) => FilterVersions();
        OldCheckBox.Checked += (s, e) => FilterVersions();
        OldCheckBox.Unchecked += (s, e) => FilterVersions();

        // Recherche (filtrage à chaque frappe)
        SearchBox.KeyUp += (s, e) => FilterVersions();

        // Loader radio (stocke le loader sélectionné)
        NoLoaderRadio.Checked += (s, e) => SetLoader("Aucun");
        NeoForgeRadio.Checked += (s, e) => SetLoader("NeoForge");
        ForgeRadio.Checked += (s, e) => SetLoader("Forge");
        FabricRadio.Checked += (s, e) => SetLoader("Fabric");
        QuiltRadio.Checked += (s, e) => SetLoader("Quilt");
        LiteLoaderRadio.Checked += (s, e) => SetLoader("LiteLoader");

        // Boutons
        OKButton.Click += OkButton_Click;
        CancelButton.Click += (s, e) => Close();
    }

    private ObservableCollection<MinecraftVersion> AllVersions { get; } = new();

    private void SetLoader(string loader)
    {
        _selectedLoader = loader;
        // Ici tu peux afficher dynamiquement un panneau selon le loader sélectionné
        // Par exemple, afficher un TextBlock si "Aucun" est sélectionné
        // ou afficher les options du loader si ce n'est pas "Aucun"
        // (À adapter selon ton XAML)
    }

    private void LoadVersions()
    {
        AllVersions.Clear();
        AllVersions.Add(new MinecraftVersion
            { Name = "1.21.5", ReleaseDate = new DateTime(2025, 3, 25), Type = "release" });
        AllVersions.Add(new MinecraftVersion
            { Name = "1.21.4", ReleaseDate = new DateTime(2024, 12, 3), Type = "release" });
        AllVersions.Add(new MinecraftVersion
            { Name = "1.21.3", ReleaseDate = new DateTime(2024, 10, 23), Type = "release" });
        AllVersions.Add(new MinecraftVersion
            { Name = "1.21.1", ReleaseDate = new DateTime(2024, 8, 8), Type = "release" });
        AllVersions.Add(new MinecraftVersion
            { Name = "1.20.6", ReleaseDate = new DateTime(2024, 4, 29), Type = "release" });
        AllVersions.Add(new MinecraftVersion
            { Name = "1.20.5", ReleaseDate = new DateTime(2024, 3, 15), Type = "snapshot" });
        AllVersions.Add(new MinecraftVersion
            { Name = "1.20.4", ReleaseDate = new DateTime(2024, 2, 1), Type = "beta" });
        AllVersions.Add(new MinecraftVersion
            { Name = "1.20.3", ReleaseDate = new DateTime(2023, 12, 1), Type = "alpha" });
        AllVersions.Add(new MinecraftVersion
            { Name = "1.20.2", ReleaseDate = new DateTime(2023, 11, 1), Type = "experimental" });
        // Ajoute d'autres versions si besoin
        FilterVersions();
    }

    private void FilterVersions()
    {
        var showRelease = OfficialCheckBox.IsChecked ?? false;
        var showSnapshot = SnapshotCheckBox.IsChecked ?? false;
        var showOld = OldCheckBox.IsChecked ?? false;
        var search = SearchBox.Text?.ToLowerInvariant() ?? "";

        var filtered = AllVersions.Where(v =>
            (string.IsNullOrWhiteSpace(search) || v.Name.ToLowerInvariant().Contains(search)) &&
            (
                (showRelease && v.Type == "release") ||
                (showSnapshot && v.Type == "snapshot") ||
                (showOld && (v.Type == "beta" || v.Type == "alpha" || v.Type == "experimental"))
            )
        ).ToList();
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        var instanceName = InstanceNameBox.Text;
        var group = GroupComboBox.SelectedItem?.ToString();
        var loader = _selectedLoader;

        // Ici, traite la création de l'instance selon les valeurs récupérées
        // ...

        Close();
    }

    // Pour le bouton "Actualiser"
    private void RefreshButton_Click(object? sender, RoutedEventArgs e)
    {
        LoadVersions();
    }

    // Modèle pour une version Minecraft
    public class MinecraftVersion
    {
        public string Name { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Type { get; set; }
    }
}