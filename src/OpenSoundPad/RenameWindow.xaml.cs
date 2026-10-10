using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace OpenSoundPad;

public partial class RenameWindow : Window
{
    public string ResultName { get; private set; } = "";

    public RenameWindow(string currentName)
    {
        InitializeComponent();
        NameInput.Text = currentName;
        NameInput.SelectAll();
        Loaded += (_, _) => NameInput.Focus();

        Title = Loc.RenameSound;
        TitleText.Text = Loc.RenameSound;
        PromptText.Text = Loc.EnterNewName;
        CancelBtn.Content = Loc.Cancel;
        OkBtn.Content = Loc.T("Сохранить", "Save", "Speichern", "Guardar", "Enregistrer");
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

    private void NameInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Ok_Click(sender, e);
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        string val = NameInput.Text.Trim();
        if (string.IsNullOrEmpty(val))
        {
            MessageBox.Show(Loc.T(
                "Название не может быть пустым.",
                "Name cannot be empty.",
                "Name darf nicht leer sein.",
                "El nombre no puede estar vacío.",
                "Le nom ne peut pas être vide."), "OSP", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        ResultName = val;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
