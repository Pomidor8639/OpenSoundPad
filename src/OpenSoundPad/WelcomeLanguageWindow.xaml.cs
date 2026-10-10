using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace OpenSoundPad;

public partial class WelcomeLanguageWindow : Window
{
    public string SelectedLanguage { get; private set; } = "en";

    public WelcomeLanguageWindow()
    {
        InitializeComponent();
        SelectLang("en");
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        try
        {
            var helper = new WindowInteropHelper(this);
            int darkMode = 1;
            DwmSetWindowAttribute(helper.Handle, 20, ref darkMode, sizeof(int));
            DwmSetWindowAttribute(helper.Handle, 19, ref darkMode, sizeof(int));
        }
        catch
        {
        }
    }

    private void SelectLang(string lang)
    {
        SelectedLanguage = lang;

        var greenBrush = (Brush)FindResource("AccentGreen");
        var darkBrush = (Brush)FindResource("BorderDark");

        CardEn.BorderBrush = lang == "en" ? greenBrush : darkBrush;
        CardEn.BorderThickness = lang == "en" ? new Thickness(1.5) : new Thickness(1);
        CheckEn.Visibility = lang == "en" ? Visibility.Visible : Visibility.Collapsed;

        CardRu.BorderBrush = lang == "ru" ? greenBrush : darkBrush;
        CardRu.BorderThickness = lang == "ru" ? new Thickness(1.5) : new Thickness(1);
        CheckRu.Visibility = lang == "ru" ? Visibility.Visible : Visibility.Collapsed;

        CardDe.BorderBrush = lang == "de" ? greenBrush : darkBrush;
        CardDe.BorderThickness = lang == "de" ? new Thickness(1.5) : new Thickness(1);
        CheckDe.Visibility = lang == "de" ? Visibility.Visible : Visibility.Collapsed;

        CardEs.BorderBrush = lang == "es" ? greenBrush : darkBrush;
        CardEs.BorderThickness = lang == "es" ? new Thickness(1.5) : new Thickness(1);
        CheckEs.Visibility = lang == "es" ? Visibility.Visible : Visibility.Collapsed;

        CardFr.BorderBrush = lang == "fr" ? greenBrush : darkBrush;
        CardFr.BorderThickness = lang == "fr" ? new Thickness(1.5) : new Thickness(1);
        CheckFr.Visibility = lang == "fr" ? Visibility.Visible : Visibility.Collapsed;

        ContinueBtn.Content = lang switch
        {
            "ru" => "Продолжить",
            "de" => "Weiter",
            "es" => "Continuar",
            "fr" => "Continuer",
            _ => "Continue"
        };
    }

    private void CardEn_Click(object sender, MouseButtonEventArgs e) => SelectLang("en");
    private void CardRu_Click(object sender, MouseButtonEventArgs e) => SelectLang("ru");
    private void CardDe_Click(object sender, MouseButtonEventArgs e) => SelectLang("de");
    private void CardEs_Click(object sender, MouseButtonEventArgs e) => SelectLang("es");
    private void CardFr_Click(object sender, MouseButtonEventArgs e) => SelectLang("fr");

    private void ContinueBtn_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
