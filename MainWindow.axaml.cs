using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using Avalonia.Media;

namespace MCLauncher;

public partial class MainWindow : Window
{
    //private readonly MinecraftLauncher launcher;
    private bool editModeEnabled = false;

    public MainWindow()
    {
        InitializeComponent();
        //launcher = new MinecraftLauncher();
        
        // Initialiser quelques données d'exemple pour le design
        InitializeDemoData();
    }

    // private void OpenAzurhosts(object? sender, RoutedEventArgs e)
    // {
    //     var url = "https://azurhosts.com";
    //     var psi = new ProcessStartInfo(url) { UseShellExecute = true };
    //     Process.Start(psi);
    // }

    // TODO : Faire la demande de partenariat en ticket à Azurhosts, puis remettre le code ci-dessus
    private void InitializeDemoData()
    {
        // Exemple de données pour la démonstration
        // Dans une version finale, ces données proviendraient d'une source réelle
        var instances = new[]
        {
            "Survival 1.20.4",
            "Creative Mods",
            "SkyBlock Challenge"
        };
        
        // Déjà défini dans le XAML avec ItemsSource, donc on peut commenter cette ligne
        // InstancesRepeater.ItemsSource = instances;
    }

    private void OpenGitHub(object? sender, RoutedEventArgs e)
    {
        var url = "https://github.com/YoannDev90/MCLauncher";
        var psi = new ProcessStartInfo(url) { UseShellExecute = true };
        Process.Start(psi);
        StatusText.Text = "Ouverture de GitHub...";
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
        try
        {
            StatusText.Text = "Ouverture des paramètres...";
            
            // Créer une nouvelle instance de la fenêtre des paramètres
            var settingsWindow = new SettingsWindow();
            
            // Afficher la fenêtre
            settingsWindow.Show();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur d'ouverture des paramètres: {ex}");
            StatusText.Text = "Erreur: impossible d'ouvrir les paramètres";
        }
    }

    private void Launch(object? sender, RoutedEventArgs e)
    {
        StatusText.Text = "Lancement de l'instance";
    }

    private void Edit(object? sender, RoutedEventArgs e)
    {
        StatusText.Text = "Modification de l'instance";
    }

    private void Delete(object? sender, RoutedEventArgs e)
    {
        StatusText.Text = "Suppression de l'instance";
    }
    
    private void ToggleEditMode(object? sender, RoutedEventArgs e)
    {
        editModeEnabled = !editModeEnabled;
        EditModeEnabled.IsChecked = editModeEnabled;
        
        // Mettre à jour l'apparence du bouton de mode édition
        if (editModeEnabled)
        {
            EditModeButton.Background = new SolidColorBrush(Color.Parse("#CC5068B0"));
            StatusText.Text = "Mode édition activé";
        }
        else
        {
            EditModeButton.Background = new SolidColorBrush(Color.Parse("#7F17181C"));
            StatusText.Text = "Mode édition désactivé";
        }
    }
}
