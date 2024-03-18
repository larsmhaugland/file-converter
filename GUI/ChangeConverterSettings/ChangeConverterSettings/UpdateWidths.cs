using Avalonia.Controls;
using Avalonia.Media;
using ChangeConverterSettings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Markup.Xaml;
using System.IO;
using System.Reflection;
using Avalonia.Layout;
using Avalonia.Input;
using Tmds.DBus.Protocol;
using System.Diagnostics;

public class UpdateWidths
{
    private readonly MainWindow mainWindow;
    public UpdateWidths(MainWindow _mainWindow)
    {
        mainWindow = _mainWindow;
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
    

    public void UpdateColumnHeaderWidths()
    {
        TextBlock? formatColumn = mainWindow.FindControl<TextBlock>("FormatColumn");
        if (formatColumn != null)
        {
            double width = GetControlWidth(formatColumn);
            if (width > WidthInfo.longestName)
                WidthInfo.longestName = (int)width;
            else
                formatColumn.Width = WidthInfo.longestName;
        }

        TextBlock? pronomColumn = mainWindow.FindControl<TextBlock>("pronomColumn");
        if (pronomColumn != null)
        {
            double width = GetControlWidth(pronomColumn);
            if (width > WidthInfo.longestFormat)
                WidthInfo.longestFormat = (int)width;
            else
                pronomColumn.Width = WidthInfo.longestFormat;
        }


        TextBlock? outputColumn = mainWindow.FindControl<TextBlock>("outputColumn");
        if (outputColumn != null)
        {
            double width = GetControlWidth(outputColumn);
            if (width > WidthInfo.longestOutput)
                WidthInfo.longestOutput = (int)width;
            else
                outputColumn.Width = WidthInfo.longestOutput;
        }


        TextBlock? outputNameColumn = mainWindow.FindControl<TextBlock>("outputNameColumn");
        if (outputNameColumn != null)
        {
            double width = GetControlWidth(outputNameColumn);
            if (width > WidthInfo.longestOutputType)
                WidthInfo.longestOutputType = (int)width;
            else
                outputNameColumn.Width = WidthInfo.longestOutputType;
        }
    }
    public void UpdateControlWidths()
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

