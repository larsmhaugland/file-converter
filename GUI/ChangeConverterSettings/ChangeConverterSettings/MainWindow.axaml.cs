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
            Console.WriteLine(ComponentLists.outputTracker);
        }
        public void SaveButton(object sender, RoutedEventArgs args)
        {
            CreateElements.UpdateState();
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
                            if(settingsData.FormatName == name)
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

        public double GetControlWidth(Control control)
        {
            double padding = 10;
            if (control is TextBox textBox)
            {
                var formattedText = new FormattedText(
                    textBox.Text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(textBox.FontFamily, textBox.FontStyle, textBox.FontWeight, textBox.FontStretch),
                    textBox.FontSize,
                    Brushes.Black);

                // Add padding to accommodate the text properly
                return formattedText.Width + padding * 2; // Add padding on both sides
            }
            else if (control is TextBlock textBlock)
            {
                textBlock.Measure(Size.Infinity);
                return textBlock.DesiredSize.Width;
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.Measure(Size.Infinity);
                return comboBox.DesiredSize.Width;
            }
            else
            {
                // Handle other types of controls or throw an exception
                throw new ArgumentException("Unsupported control type");
            }
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
            
            for(int i = 0; i < GlobalVariables.FileSettings.Count; i++)
            {
                ComboBox? formatDropDown = this.FindControl<ComboBox>("formatDropDown" + (i + 1).ToString());
                if(formatDropDown == null)
                {
                    StackPanel? stackPanelMain = this.FindControl<StackPanel>("MainStackPanel");
                    if (stackPanelMain != null)
                    {
                        CreateElements creator = new CreateElements(i, stackPanelMain); 
                    }
                }
            }
            UpdateColumnHeaderWidths();
            UpdateWidths();
        }
        
        private void UpdateColumnHeaderWidths()
        {
            TextBlock? formatColumn = this.FindControl<TextBlock>("FormatColumn");
            if (formatColumn != null)
            {
                double width = GetControlWidth(formatColumn);
                if (width > WidthInfo.longestName)
                    WidthInfo.longestName = (int)width;
                else
                    formatColumn.Width = WidthInfo.longestName;
            }

            TextBlock? pronomColumn = this.FindControl<TextBlock>("pronomColumn");
            if (pronomColumn != null)
            {
                double width = GetControlWidth(pronomColumn);
                if (width > WidthInfo.longestFormat)
                    WidthInfo.longestFormat = (int)width;
                else
                    pronomColumn.Width = WidthInfo.longestFormat;
            }


            TextBlock? outputColumn = this.FindControl<TextBlock>("outputColumn");
            if (outputColumn != null)
            {
                double width = GetControlWidth(outputColumn);
                if (width > WidthInfo.longestOutput)
                    WidthInfo.longestOutput = (int)width;
                else
                    outputColumn.Width = WidthInfo.longestOutput;
            }
           

            TextBlock? outputNameColumn = this.FindControl<TextBlock>("outputNameColumn");
            if (outputNameColumn != null)
            {
                double width = GetControlWidth(outputNameColumn);
                if (width > WidthInfo.longestOutputType)
                    WidthInfo.longestOutputType = (int)width;
                else
                    outputNameColumn.Width = WidthInfo.longestOutputType;
            }
        }
        public void UpdateWidths()
        {
            if (WidthInfo.longestName > 0)
            {
                foreach (TextBlock formatName in ComponentLists.formatNames)
                {
                    formatName.Width = WidthInfo.longestName;
                }
            }
            if (WidthInfo.longestFormat > 0)
            {
                

                foreach (var comboBox in ComponentLists.formatDropDowns)
                {
                    comboBox.Width = WidthInfo.longestFormat;
                }
            }
            if (WidthInfo.longestOutput > 0)
            {
                foreach (TextBox outputTypeTextBox in ComponentLists.outputPronomCodeTextBoxes)
                {
                    outputTypeTextBox.Width = WidthInfo.longestOutput;
                }
            }
            if (WidthInfo.longestOutputType > 0)
            {
                foreach (TextBox outputTypeTextBox in ComponentLists.outputNameTextBoxes)
                {
                    outputTypeTextBox.Width = WidthInfo.longestOutputType;
                }
            }
        }
    }
}