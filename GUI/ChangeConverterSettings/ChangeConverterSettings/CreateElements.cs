using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using ChangeConverterSettings;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using Avalonia.Media;
using Avalonia.VisualTree;


public class CreateElements
{
    public CreateElements()
    {
    }
    public CreateElements(int index, StackPanel mainPanel)
    {
        StackPanel? stackPanel = null;
        foreach (StackPanel stackpanel in ComponentLists.stackPanels)
        {
            var classnameBlock = (TextBlock)stackpanel.Children[0];
            string classname = classnameBlock.Text;
            if (classname == GlobalVariables.FileSettings[index].ClassName)
            {
                stackPanel = stackpanel;
            }
        }
        
        if (stackPanel == null)
        {
            stackPanel = CreateAndAddHorizontalStackPanel(mainPanel);
            ComponentLists.stackPanels.Add(stackPanel);
        }
        CreateAndAddElements(index, stackPanel);
    }


    public static void CreateAndAddElements(int index, Panel panel)
    {
        if (panel == null)
        {
            Logger.Instance.SetUpRunTimeLogMessage("Panel is null", true);
            return;
        }

        var fileSettings = GlobalVariables.FileSettings[index];

        var className = fileSettings.ClassName;
        var existingClassName = ComponentLists.formatNames.FirstOrDefault(tb => ((TextBlock)tb).Text == className);
        ComboBox formatDropDown;

        if (existingClassName == null)
        {
            var formatTextBlock = CreateTextBlock(className);
            panel.Children.Add(formatTextBlock);
            ComponentLists.formatNames.Add(formatTextBlock);

            formatDropDown = CreateComboBox(index);
            panel.Children.Add(formatDropDown);
            ComponentLists.formatDropDowns.Add(formatDropDown);
            formatDropDown.Items.Add(GlobalVariables.defaultText);
            ComponentLists.outputTracker.Add(fileSettings.ClassName, fileSettings.ClassDefault);
            formatDropDown.SelectionChanged += (sender, e) => FormatDropDown_SelectionChanged(sender);

            var defaultTypeTextBox = CreateTextBox(fileSettings.DefaultType, index);
            panel.Children.Add(defaultTypeTextBox);
            ComponentLists.outputPronomCodeTextBoxes.Add(defaultTypeTextBox);

            var outputTypeTextBox = CreateReadOnlyTextBox(PronomHelper.PronomToFullName(fileSettings.DefaultType), index);
            panel.Children.Add(outputTypeTextBox);
            ComponentLists.outputNameTextBoxes.Add(outputTypeTextBox);

            var updateButton = CreateButton("Update", index);
            updateButton.Click += (sender, e) => UpdateButton_Click(sender);
            panel.Children.Add(updateButton);
            ComponentLists.updateButtons.Add(updateButton);
        }
        else
        {
            formatDropDown = (ComboBox)ComponentLists.formatDropDowns[ComponentLists.formatNames.IndexOf(existingClassName)];
        }

        formatDropDown.Items.Add(fileSettings.FormatName);
        ComponentLists.outputTracker.Add(fileSettings.FormatName, fileSettings.DefaultType);
        formatDropDown.SelectedIndex = 0;

        FindWidestElements();

    }

    private static void FormatDropDown_SelectionChanged(object sender)
    {
        var comboBox = (ComboBox)sender;
        var parent = (StackPanel)comboBox.Parent;
        var className = ((TextBlock)parent.Children[0]).Text;
        var textBox = (TextBox)parent.Children[2];
        var readOnlyTextBox = (TextBox)parent.Children[3];
        var item = comboBox.SelectedItem.ToString();

        string type;
        if (item == GlobalVariables.defaultText)
        {
            type = ComponentLists.outputTracker[className];
        }
        else
        {
            type = ComponentLists.outputTracker[item];
        }

        textBox.Text = type;
        readOnlyTextBox.Text = PronomHelper.PronomToFullName(type);
    }

    public static void UpdateState()
    {
        foreach (StackPanel stackpanel in ComponentLists.stackPanels)
        {
            
            var parent = stackpanel;
            var className = ((TextBlock)parent.Children[0]).Text;
            var comboBox = (ComboBox)parent.Children[1];
            var textBox = (TextBox)parent.Children[2];
            var readOnlyTextBox = (TextBox)parent.Children[3];
            var item = comboBox.SelectedItem.ToString();

            var newType = textBox.Text;

            string oldType;
            if (item == GlobalVariables.defaultText)
            {
                item = className;
            }

            oldType = ComponentLists.outputTracker[item];

            if (newType != oldType)
            {
                ComponentLists.outputTracker[item] = newType;
            }

            readOnlyTextBox.Text = PronomHelper.PronomToFullName(newType);
        }
    }
    private static void UpdateButton_Click(object sender)
    {
        var button = (Button)sender;
        var parent = (StackPanel)button.Parent;
        var className = ((TextBlock)parent.Children[0]).Text;
        var comboBox = (ComboBox)parent.Children[1];
        var textBox = (TextBox)parent.Children[2];
        var readOnlyTextBox = (TextBox)parent.Children[3];
        var item = comboBox.SelectedItem.ToString();

        var newType = textBox.Text;
        
        string oldType;
        if (item == GlobalVariables.defaultText)
        {
            item = className;
        }
        
        oldType = ComponentLists.outputTracker[item];

        if (newType != oldType)
        {
            ComponentLists.outputTracker[item] = newType;
        }
        
        readOnlyTextBox.Text = PronomHelper.PronomToFullName(newType);

    }
    public static StackPanel CreateAndAddHorizontalStackPanel(StackPanel mainStackPanel)
    {
        if (mainStackPanel == null)
        {
            Logger.Instance.SetUpRunTimeLogMessage("MainStackPanel is null", true);
            return null;
        }

        var horizontalStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 10, 0, 0)
        };

        mainStackPanel.Children.Add(horizontalStackPanel);
        return horizontalStackPanel;
    }

    private static TextBlock CreateTextBlock(string text)
    {
        return new TextBlock
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(10, 10, 0, 0)
        };
    }

    private static ComboBox CreateComboBox(int index)
    {
        var comboBox = new ComboBox
        {
            Name = "formatDropDown" + (index + 1),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(10, 10, 0, 0)
        };
        return comboBox;
    }

    private static TextBox CreateTextBox(string text, int index)
    {
        return new TextBox
        {
            Text = text,
            Name = "OutputTypeTextBox" + (index + 1),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(10, 10, 0, 0)
        };
    }

    private static TextBox CreateReadOnlyTextBox(string text, int index)
    {
        return new TextBox
        {
            Text = text,
            IsReadOnly = true,
            Name = "OutputTypeTextBox" + (index + 1),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(10, 10, 0, 0)
        };
    }

    private static Button CreateButton(string text, int index)
    {
        return new Button
        {
            Content = text,
            Name = "Button" + (index + 1),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(10, 10, 0, 0)
        };
    }
    private static double CalculateTextWidth(Control element)
    {
        string text = element switch
        {
            TextBox { Text: var textBoxText } when textBoxText != null => textBoxText,
            TextBlock { Text: var textBlockText } when textBlockText != null => textBlockText,
            _ => throw new ArgumentException("Unsupported element type or null text", nameof(element))
        };

        var fontFamily = GetFontFamily(element);
        var fontSize = GetFontSize(element);
        var fontStyle = GetFontStyle(element);
        var fontWeight = GetFontWeight(element);
        var fontStretch = GetFontStretch(element);

        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
            fontSize,
            Brushes.Black // You can specify the brush for the text color here
        );
        formattedText.MaxTextWidth = double.PositiveInfinity;

        // Calculate the width required for the content
        double contentWidth = formattedText.Width;

        // Add padding or margin as needed
        double padding = 20; // Adjust this value as needed
        return contentWidth + padding;
    }

    private static FontFamily GetFontFamily(Control element) =>
        element switch
        {
            TextBox textBox => textBox.FontFamily,
            TextBlock textBlock => textBlock.FontFamily,
            _ => throw new ArgumentException("Unsupported element type", nameof(element))
        };

    private static double GetFontSize(Control element) =>
        element switch
        {
            TextBox textBox => textBox.FontSize,
            TextBlock textBlock => textBlock.FontSize,
            _ => 12 // Default font size
        };
    private static FontStyle GetFontStyle(Control element) =>
    element switch
    {
        TextBox textBox => textBox.FontStyle,
        TextBlock textBlock => textBlock.FontStyle,
        _ => FontStyle.Normal // Default font style
    };

    private static FontWeight GetFontWeight(Control element) =>
        element switch
        {
            TextBox textBox => textBox.FontWeight,
            TextBlock textBlock => textBlock.FontWeight,
            _ => FontWeight.Normal // Default font weight
        };

    private static FontStretch GetFontStretch(Control element) =>
        element switch
        {
            TextBox textBox => textBox.FontStretch,
            TextBlock textBlock => textBlock.FontStretch,
            _ => FontStretch.Normal // Default font stretch
        };

    public static double GetMaxComboBoxWidth(List<ComboBox> comboBoxes)
    {
        double maxWidth = 0;

        foreach (var comboBox in comboBoxes)
        {
            comboBox.Measure(Size.Infinity);
            double width = comboBox.DesiredSize.Width;

            maxWidth = Math.Max(maxWidth, width);
        }

        return maxWidth;
    }

    private static void FindWidestElements()
    {
        foreach (var textBlock in ComponentLists.formatNames)
        {
            double width = CalculateTextWidth(textBlock);
            if (width > WidthInfo.longestName)
            {
                WidthInfo.longestName = (int)width;
            }
        }


        double maxComboBoxWidth = GetMaxComboBoxWidth(ComponentLists.formatDropDowns);
        WidthInfo.longestFormat = (int)maxComboBoxWidth;

        foreach (var textBox in ComponentLists.outputPronomCodeTextBoxes)
        {
            double width = CalculateTextWidth(textBox);
            if (width > WidthInfo.longestOutput)
            {
                WidthInfo.longestOutput = (int)width;
            }
        }

        foreach (var textBox in ComponentLists.outputNameTextBoxes)
        {
            double width = CalculateTextWidth(textBox);
            if ( width > WidthInfo.longestOutputType)
            {
                WidthInfo.longestOutputType = (int)width;
            }
        }
    }
}

