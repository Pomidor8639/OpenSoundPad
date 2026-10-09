using System.Collections.Generic;
using System.Windows;
using OpenSoundPad.Audio;

namespace OpenSoundPad;

public partial class DevicesWindow : Window
{
    public string? SelectedInputId { get; private set; }
    public string? SelectedOutputId { get; private set; }
    public string? SelectedMonitorId { get; private set; }

    public DevicesWindow(string? inputId, string? outputId, string? monitorId)
    {
        InitializeComponent();
        RefreshLists();
        SelectById(InputBox, SelectedInputId = inputId);
        SelectById(CableBox, SelectedOutputId = outputId);
        SelectById(MonitorBox, SelectedMonitorId = monitorId);
        CableBox.SelectionChanged += (_, _) => UpdateVirtMic();
        UpdateVirtMic();
    }

    private void RefreshLists()
    {
        InputBox.ItemsSource = OspEngine.GetInputMicrophones();
        CableBox.ItemsSource = OspEngine.GetVirtualCables();
        MonitorBox.ItemsSource = OspEngine.GetOutputDevices();
        if (InputBox.SelectedIndex < 0 && InputBox.Items.Count > 0) InputBox.SelectedIndex = 0;
        if (CableBox.SelectedIndex < 0 && CableBox.Items.Count > 0) CableBox.SelectedIndex = 0;
        if (MonitorBox.SelectedIndex < 0 && MonitorBox.Items.Count > 0) MonitorBox.SelectedIndex = 0;
    }

    private static void SelectById(System.Windows.Controls.ComboBox box, string? id)
    {
        if (id == null)
        {
            if (box.SelectedIndex < 0 && box.Items.Count > 0) box.SelectedIndex = 0;
            return;
        }
        foreach (var item in box.Items)
            if (item is OspDevice d && d.Id == id) { box.SelectedItem = item; return; }
        if (box.SelectedIndex < 0 && box.Items.Count > 0) box.SelectedIndex = 0;
    }

    private void UpdateVirtMic()
    {
        VirtMicText.Text = CableBox.SelectedItem is OspDevice dev
            ? OspEngine.FindVirtualMicName(dev.Name)
            : "Кабель не выбран";
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        string? i = (InputBox.SelectedItem as OspDevice)?.Id;
        string? c = (CableBox.SelectedItem as OspDevice)?.Id;
        string? m = (MonitorBox.SelectedItem as OspDevice)?.Id;
        RefreshLists();
        SelectById(InputBox, i);
        SelectById(CableBox, c);
        SelectById(MonitorBox, m);
        UpdateVirtMic();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        SelectedInputId = (InputBox.SelectedItem as OspDevice)?.Id;
        SelectedOutputId = (CableBox.SelectedItem as OspDevice)?.Id;
        SelectedMonitorId = (MonitorBox.SelectedItem as OspDevice)?.Id;
        DialogResult = true;
    }
}
