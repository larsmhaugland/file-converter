using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Markup.Xaml;
using System.IO;
using Avalonia.Media;
using System.Globalization;
using System.Reflection;
using Avalonia.Layout;
using Avalonia.Input;
using Tmds.DBus.Protocol;
using System.Diagnostics;

namespace ChangeConverterSettings
{
    public static class GlobalVariables
    {
        public static string? Input = null;
        public static string? Output = null;
        //List of all settingsdata
        public static List <SettingsData> FileSettings = new List<SettingsData>();
        // Map with info about what folders have overrides for specific formats
        public static Dictionary<string, SettingsData> FolderOverride = new Dictionary<string, SettingsData>(); // the key is a foldername
        public static int? maxThreads = null;
        public static string? checksumHash = null;
        public static string? requester = null;
        public static string? converter = null;
        public static string? timeout = null;
        public static string defaultText = "Default";
        public static List<string> supportedHashes = new List<string> { "MD5", "SHA256" };
        public static string defaultSettingsPath = "../../../settings.xml";
    }
    public static class WidthInfo
    {
        public static int longestName = 0;
        public static int longestFormat = 0;
        public static int longestOutput = 0;
        public static int longestOutputType = 0;
    }
    public static class ComponentLists
    {
        public static List<TextBlock> formatNames = new List<TextBlock>();
        public static List<ComboBox> formatDropDowns = new List<ComboBox>();
        public static List<TextBox> outputPronomCodeTextBoxes = new List<TextBox>();
        public static List<TextBox> outputNameTextBoxes = new List<TextBox>();
        public static List<Button> updateButtons = new List<Button>();
        public static List<StackPanel> stackPanels = new List<StackPanel>();
        public static Dictionary<string, string> outputTracker = new Dictionary<string, string>();
    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Directory.SetCurrentDirectory("../../../");
            InitializeComponent();
            Settings settings = Settings.Instance;
            settings.ReadSettings(GlobalVariables.defaultSettingsPath);
            WriteSettingsToScreen();
            Console.WriteLine(GlobalVariables.FileSettings);
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public void SaveButton(object sender, RoutedEventArgs args)
        {
            CreateElements createElements = new CreateElements(this);
            createElements.UpdateState();

            TextBox? requesterTextBox = this.FindControl<TextBox>("Requester");
            if (requesterTextBox != null)
                GlobalVariables.requester = requesterTextBox.Text;
            TextBox? converterTextBox = this.FindControl<TextBox>("Converter");
            if (converterTextBox != null)
                GlobalVariables.converter = converterTextBox.Text;
            TextBox? inputTextBox = this.FindControl<TextBox>("Input");
            if (inputTextBox != null)
                GlobalVariables.Input = inputTextBox.Text;
            TextBox? outputTextBox = this.FindControl<TextBox>("Output");
            if (outputTextBox != null)
                GlobalVariables.Output = outputTextBox.Text;
            TextBox? threadsTextBox = this.FindControl<TextBox>("MaxThreads");
            if (threadsTextBox != null && int.TryParse(threadsTextBox.Text, out _))
                GlobalVariables.maxThreads = int.Parse(threadsTextBox.Text);
            ComboBox? checksumComboBox = this.FindControl<ComboBox>("Checksum");
            if (checksumComboBox != null)
                GlobalVariables.checksumHash = checksumComboBox.SelectedItem.ToString();
            TextBox? timeoutTextBox = this.FindControl<TextBox>("Timeout");
            if (timeoutTextBox != null)
                GlobalVariables.timeout = timeoutTextBox.Text;


            foreach (var settingsData in GlobalVariables.FileSettings)
            {
                for (int i = 0; i < ComponentLists.formatDropDowns.Count; i++)
                {
                    ComboBox formatDropDown = ComponentLists.formatDropDowns[i];
                    int prevSelecIndex = formatDropDown.SelectedIndex;
                    for (int j = 0; j < formatDropDown.Items.Count; j++)
                    {
                        formatDropDown.SelectedIndex = j;
                        string? name = formatDropDown.SelectedItem.ToString();
                        string? text;
                        if (name != GlobalVariables.defaultText)
                        {
                            text = ComponentLists.outputTracker[name];
                        }
                        else
                        {
                            text = ComponentLists.outputTracker[settingsData.ClassName];
                        }
                        if (name != null && text != null)
                        {
                            if (settingsData.FormatName == name)
                                settingsData.DefaultType = text;
                            else if (name == GlobalVariables.defaultText && settingsData.ClassName == ComponentLists.formatNames[i].Text)
                                settingsData.ClassDefault = text;
                        }

                    }

                    ComponentLists.formatDropDowns[i].SelectedIndex = prevSelecIndex;
                }
            }

            Debug.WriteLine("Requester: " + GlobalVariables.requester);
            Debug.WriteLine("Converter: " + GlobalVariables.converter);
            Debug.WriteLine("Input: " + GlobalVariables.Input);
            Debug.WriteLine("Output: " + GlobalVariables.Output);
            Debug.WriteLine("MaxThreads: " + GlobalVariables.maxThreads);
            Debug.WriteLine("Checksum: " + GlobalVariables.checksumHash);
            Debug.WriteLine("Timeout: " + GlobalVariables.timeout);

            WriteSettingsToFile();
        }

        void WriteSettingsToFile()
        {
            Settings settings = Settings.Instance;
            settings.WriteSettings(GlobalVariables.defaultSettingsPath);
        }
        private void WriteSettingsToScreen()
        {
            TextBox? requesterTextBox = this.FindControl<TextBox>("Requester");
            if (requesterTextBox != null)
                requesterTextBox.Text = GlobalVariables.requester;
            TextBox? converterTextBox = this.FindControl<TextBox>("Converter");
            if (converterTextBox != null)
                converterTextBox.Text = GlobalVariables.converter;
            TextBox? inputTextBox = this.FindControl<TextBox>("Input");
            if (inputTextBox != null)
                inputTextBox.Text = GlobalVariables.Input;
            TextBox? outputTextBox = this.FindControl<TextBox>("Output");
            if (outputTextBox != null)
                outputTextBox.Text = GlobalVariables.Output;
            TextBox? threadsTextBox = this.FindControl<TextBox>("MaxThreads");
            if (threadsTextBox != null)
                threadsTextBox.Text = GlobalVariables.maxThreads.ToString();
            ComboBox? checksumComboBox = this.FindControl<ComboBox>("Checksum");
            if (checksumComboBox != null)
            {
                foreach (string hash in GlobalVariables.supportedHashes)
                {
                    checksumComboBox.Items.Add(hash);
                }
                checksumComboBox.SelectedItem = GlobalVariables.checksumHash;
            }

            TextBox? timeoutTextBox = this.FindControl<TextBox>("Timeout");
            if (timeoutTextBox != null)
                timeoutTextBox.Text = GlobalVariables.timeout;


            GlobalVariables.FileSettings.Sort((x, y) => x.FormatName.CompareTo(y.FormatName));

            for (int i = 0; i < GlobalVariables.FileSettings.Count; i++)
            {
                ComboBox? formatDropDown = this.FindControl<ComboBox>("formatDropDown" + (i + 1).ToString());
                if (formatDropDown == null)
                {
                    StackPanel? stackPanelMain = this.FindControl<StackPanel>("MainStackPanel");
                    if (stackPanelMain != null)
                    {
                        CreateElements creator = new CreateElements(i, stackPanelMain, this);
                    }
                }
            }

            UpdateWidths updateWidths = new UpdateWidths(this);
            updateWidths.UpdateColumnHeaderWidths();
            updateWidths.UpdateControlWidths();
        }
    }
}