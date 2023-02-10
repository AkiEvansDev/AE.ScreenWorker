using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using AE.Core;
using AE.CoreWPF;

using Microsoft.Win32;

using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;

using Ookii.Dialogs.Wpf;

using ScreenBase.Data.Base;

using ScreenWorkerWPF.ViewModel;
using ScreenWorkerWPF.Windows;

namespace ScreenWorkerWPF.Dialogs;

public partial class EditPropertyDialog : ContentDialog
{
    private readonly Dictionary<string, UIElement> controls;

    public EditPropertyDialog(IEditProperties source, string title)
    {
        controls = new Dictionary<string, UIElement>();
        InitializeComponent();

        var clone = source.Clone();
        Title = title;

        var properties = clone
            .GetType()
            .GetProperties()
            .Where(p => p.GetCustomAttribute<EditPropertyAttribute>() != null)
            .GroupBy(p => p.GetCustomAttribute<EditPropertyAttribute>().Order, p => p)
            .OrderBy(g => g.Key)
            .ToList();

        var isLastCB = false;
        foreach (var group in properties)
        {
            if (group.First().GetCustomAttribute<SeparatorAttribute>() != null)
            {
                Container.Children.Add(new Separator());
            }

            if (group.Count() == 1)
            {
                var control = GetControl(group.First(), clone);
                if (control != null)
                {
                    if (isLastCB && control is CheckBox checkBox)
                        checkBox.Margin = new Thickness(0, -Container.Spacing, 0, 0);

                    Container.Children.Add(control);
                    isLastCB = control is CheckBox;
                }
            }
            else
            {
                var panel = new Grid();
                panel.ColumnDefinitions.Add(new ColumnDefinition());

                var index = 0;
                foreach (var item in group)
                {
                    index++;

                    var control = GetControl(item, clone);
                    if (control != null)
                    {
                        Grid.SetColumn(control, panel.ColumnDefinitions.Count - 1);
                        panel.Children.Add(control);

                        if (index != group.Count())
                        {
                            panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
                            panel.ColumnDefinitions.Add(new ColumnDefinition());
                        }
                    }
                }

                if (isLastCB && panel.Children[0] is CheckBox)
                    panel.Margin = new Thickness(0, -Container.Spacing, 0, 0);

                Container.Children.Add(panel);
                isLastCB = panel.Children[0] is CheckBox;
            }
        }

        clone.NeedUpdateInvoke();

        Closing += (s, e) =>
        {
            if (e.Result == ContentDialogResult.Primary)
                foreach (var property in properties.SelectMany(g => g))
                {
                    property.SetValue(source, property.GetValue(clone));
                }
        };
    }

    private UIElement GetControl(PropertyInfo property, IEditProperties clone)
    {
        var attr = property.GetCustomAttribute<EditPropertyAttribute>();

        UIElement result;
        if ((property.PropertyType == typeof(int) || property.PropertyType == typeof(double)) && attr is NumberEditPropertyAttribute nAttr)
        {
            result = GetNumberEditControl(property, clone, nAttr);
        }
        else if (property.PropertyType == typeof(ScreenPoint) && attr is ScreenPointEditPropertyAttribute pAttr)
        {
            result = GetScreenPointEditControl(property, clone, pAttr);
        }
        else if (property.PropertyType == typeof(string) && attr is TextEditPropertyAttribute tAttr)
        {
            result = GetTextEditControl(property, clone, tAttr);
        }
        else if (property.PropertyType == typeof(string) && attr is LoadEditPropertyAttribute pathAttr)
        {
            result = GetPathEditControl(property, clone, pathAttr);
        }
        else if (property.PropertyType == typeof(string) && attr is FilePathEditPropertyAttribute fPathAttr)
        {
            result = GetFilePathEditControl(property, clone, fPathAttr);
        }
        else if (property.PropertyType == typeof(string) && attr is VariableEditPropertyAttribute vAttr)
        {
            result = GetVariableEditControl(property, clone, vAttr);
        }
        else if (property.PropertyType == typeof(bool) && attr is CheckBoxEditPropertyAttribute checkAttr)
        {
            result = GetCheckBoxEditControl(property, clone, checkAttr);
        }
        else if ((property.PropertyType == typeof(bool) || property.PropertyType == typeof(string) || property.PropertyType.IsEnum) && attr is ComboBoxEditPropertyAttribute cAttr)
        {
            result = GetComboBoxEditControl(property, clone, cAttr);
        }
        else
        {
            result = null;
        }

        if (result != null)
            controls.Add(property.Name, result);

        return result;
    }

    private UIElement GetNumberEditControl(PropertyInfo property, IEditProperties clone, NumberEditPropertyAttribute nAttr)
    {
        var control = new NumberBox
        {
            Minimum = nAttr.MinValue,
            Maximum = nAttr.MaxValue,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden,
            Header = nAttr.Title == "-" ? null : nAttr.Title ?? property.Name,
            FocusVisualStyle = null
        };

        clone.NeedUpdate += () => control.Value = (double)Convert.ChangeType(property.GetValue(clone), typeof(double));

        if (nAttr.SmallChange > 0)
            control.SmallChange = nAttr.SmallChange;
        if (nAttr.LargeChange > 0)
            control.LargeChange = nAttr.LargeChange;

        control.ValueChanged += (s, e) =>
        {
            if (!double.IsNaN(e.NewValue))
            {
                if (property.PropertyType == typeof(int))
                    property.SetValue(clone, (int)e.NewValue);
                else
                    property.SetValue(clone, e.NewValue);
            }
            else
                control.Value = 0;
        };

        if (nAttr.UseXFromScreen || nAttr.UseYFromScreen)
        {
            var panel1 = new SimpleStackPanel
            {
                Spacing = Container.Spacing,
            };
            var panel2 = new Grid();

            panel2.ColumnDefinitions.Add(new ColumnDefinition());
            panel2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Container.Spacing) });
            panel2.ColumnDefinitions.Add(new ColumnDefinition());

            if (nAttr.UseXFromScreen)
            {
                var control1 = new Button
                {
                    Content = nAttr.XFromScreenTitle,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    FocusVisualStyle = null
                };

                control1.Click += (s, e) =>
                {
                    var oldX = (int)property.GetValue(clone);
                    var point = ScreenWindow.GetPoint(new ScreenPoint(oldX, 0));

                    if (point != null)
                    {
                        if (property.PropertyType == typeof(int))
                            property.SetValue(clone, point.X);
                        else
                            property.SetValue(clone, (double)point.X);

                        clone.NeedUpdateInvoke();
                    }
                };

                Grid.SetColumn(control1, 0);
                panel2.Children.Add(control1);
            }

            if (nAttr.UseYFromScreen)
            {
                var control1 = new Button
                {
                    Content = nAttr.YFromScreenTitle,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    FocusVisualStyle = null
                };

                control1.Click += (s, e) =>
                {
                    var oldY = (int)property.GetValue(clone);
                    var point = ScreenWindow.GetPoint(new ScreenPoint(0, oldY));

                    if (point != null)
                    {
                        if (property.PropertyType == typeof(int))
                            property.SetValue(clone, point.Y);
                        else
                            property.SetValue(clone, (double)point.Y);
                    }

                    clone.NeedUpdateInvoke();
                };

                Grid.SetColumn(control1, 2);
                panel2.Children.Add(control1);
            }

            panel1.Children.Add(control);
            panel1.Children.Add(panel2);
            
            return panel1;
        }
        else
            return control;
    }

    private UIElement GetScreenPointEditControl(PropertyInfo property, IEditProperties clone, ScreenPointEditPropertyAttribute pAttr)
    {
        var viewColor = new ColorPicker();
        var control = new Button
        {
            Content = pAttr.Title,
            Margin = pAttr.ShowColorBox ? new Thickness(0) : new Thickness(0, Container.Spacing, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            FocusVisualStyle = null,
        };

        control.Click += (s, e) =>
        {
            var oldPoint = (ScreenPoint)property.GetValue(clone);
            var point = ScreenWindow.GetPoint(oldPoint);

            if (point != null)
            {
                viewColor.SelectColor = Color.FromArgb(point.A, point.R, point.G, point.B);
                property.SetValue(clone, point);
            }
        };

        if (pAttr.ShowColorBox)
        {
            var viewBox = new Border
            {
                CornerRadius = new CornerRadius(4),
                Width = 20,
                Height = 20,
            };

            clone.NeedUpdate += () =>
            {
                var point = (ScreenPoint)property.GetValue(clone);
                viewColor.SelectColor = Color.FromArgb(point.A, point.R, point.G, point.B);
                viewBox.Background = new SolidColorBrush(viewColor.SelectColor);
            };

            var panel = new Grid
            {
                Margin = new Thickness(0, 0, 0, -1)
            };

            panel.ColumnDefinitions.Add(new ColumnDefinition());
            panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
            panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            var openViewColorBtn = new DropDownButton()
            {
                Content = viewBox,
                Flyout = new Flyout
                {
                    Placement = FlyoutPlacementMode.RightEdgeAlignedTop,
                    Content = viewColor,
                },
                FocusVisualStyle = null
            };

            viewColor.SelectColorChanged += (s, e) =>
            {
                var point = (ScreenPoint)property.GetValue(clone);

                point.A = viewColor.SelectColor.A;
                point.R = viewColor.SelectColor.R;
                point.G = viewColor.SelectColor.G;
                point.B = viewColor.SelectColor.B;

                property.SetValue(clone, point);
                viewBox.Background = new SolidColorBrush(viewColor.SelectColor);
            };

            Grid.SetColumn(control, 0);
            Grid.SetColumn(openViewColorBtn, 2);

            panel.Children.Add(control);
            panel.Children.Add(openViewColorBtn);

            return panel;
        }
        else
            return control;
    }

    private UIElement GetTextEditControl(PropertyInfo property, IEditProperties clone, TextEditPropertyAttribute tAttr)
    {
        var control = new TextBox()
        {
            FocusVisualStyle = null
        };

        if (tAttr.Title != "-")
            ControlHelper.SetHeader(control, tAttr.Title ?? property.Name);

        clone.NeedUpdate += () => control.Text = (string)property.GetValue(clone);
        control.TextChanged += (s, e) => property.SetValue(clone, control.Text);

        return control;
    }

    private UIElement GetPathEditControl(PropertyInfo property, IEditProperties clone, LoadEditPropertyAttribute lAttr)
    {
        PropertyInfo nameProperty = null;
        if (!lAttr.NameProperty.IsNull())
            nameProperty = clone.GetType().GetProperty(lAttr.NameProperty);

        var control = new TextBox()
        {
            IsReadOnly = true,
            FocusVisualStyle = null
        };

        clone.NeedUpdate += () =>
        {
            var path = (string)property.GetValue(clone);

            if (!Directory.Exists(path))
            {
                path = ScriptInfo.GetDefaultPath();
                property.SetValue(clone, path);
            }

            control.Text = path;
        };

        control.PreviewMouseLeftButtonUp += (s, e) =>
        {
            VistaFileDialog dialog;

            if (lAttr is SaveEditPropertyAttribute sAttr)
                dialog = new VistaSaveFileDialog
                {
                    CheckPathExists = true,
                    Filter = sAttr.Filter,
                    DefaultExt = sAttr.DefaultExt,
                    FileName = Path.Combine(control.Text, nameProperty == null ? sAttr.DefaultName : (string)nameProperty.GetValue(clone)),
                };
            else
                dialog = new VistaOpenFileDialog
                {
                    Filter = lAttr.Filter,
                    FileName = Path.Combine(control.Text, nameProperty == null ? lAttr.DefaultName : (string)nameProperty.GetValue(clone)),
                };

            if (dialog.ShowDialog(Application.Current.MainWindow).GetValueOrDefault())
            {
                property.SetValue(clone, Path.GetDirectoryName(dialog.FileName));
                nameProperty?.SetValue(clone, Path.GetFileNameWithoutExtension(dialog.FileName));

                clone.NeedUpdateInvoke();
            }
        };

        if (lAttr.Title != "-")
            ControlHelper.SetHeader(control, lAttr.Title ?? property.Name);

        return control;
    }
    
    private UIElement GetFilePathEditControl(PropertyInfo property, IEditProperties clone, FilePathEditPropertyAttribute fPathAttr)
    {
        var control = new TextBox()
        {
            IsReadOnly = true,
            FocusVisualStyle = null
        };

        clone.NeedUpdate += () => control.Text = (string)property.GetValue(clone);

        control.PreviewMouseLeftButtonUp += (s, e) =>
        {
            var dialog = new VistaOpenFileDialog
            {
                Filter = fPathAttr.Filter,
                FileName = control.Text,
            };

            if (dialog.ShowDialog(Application.Current.MainWindow).GetValueOrDefault())
            {
                property.SetValue(clone, dialog.FileName);
                clone.NeedUpdateInvoke();
            }
        };

        if (fPathAttr.Title != "-")
            ControlHelper.SetHeader(control, fPathAttr.Title ?? property.Name);

        return control;
    }

    private UIElement GetVariableEditControl(PropertyInfo property, IEditProperties clone, VariableEditPropertyAttribute vAttr)
    {
        var control = new Grid();
        ComboBox cb1 = null;
        ComboBox cb2 = null;

        control.RowDefinitions.Add(new RowDefinition());
        control.RowDefinitions.Add(new RowDefinition());

        control.ColumnDefinitions.Add(new ColumnDefinition());

        var cb = new CheckBox
        {
            Content = vAttr.Title ?? $"Use variable for {vAttr.PropertyName}",
            FocusVisualStyle = null
        };

        void Checked()
        {
            foreach (var prop in vAttr.PropertyNames)
                controls[prop].Visibility = Visibility.Collapsed;

            cb1.Visibility = Visibility.Visible;
        }

        void Unchecked()
        {
            foreach (var prop in vAttr.PropertyNames)
                controls[prop].Visibility = Visibility.Visible;

            cb1.SelectedItem = "-";
            cb1.Visibility = Visibility.Collapsed;
        }

        clone.NeedUpdate += () =>
        {
            cb.IsChecked = !((string)property.GetValue(clone)).IsNull();

            if (cb.IsChecked == true)
                Checked();
            else
                Unchecked();
        };

        cb.Checked += (s, e) => Checked();
        cb.Unchecked += (s, e) => Unchecked();

        Grid.SetColumn(cb, 0);
        Grid.SetRow(cb, 0);
        Grid.SetColumnSpan(cb, 3);
        control.Children.Add(cb);

        static string GetPart(string data, int index, IEnumerable<string> def = null)
        {
            var split = data.Split('.');

            if (split.Length == 2)
            {
                if (index == 0 || def == null || !def.Any())
                    return split[index];
                else if (def.Contains(split[index]))
                    return split[index];
                else
                    return def.First();
            }

            if (def != null && def.Any() && index != 0)
                return def.First();

            return data;
        }

        cb1 = new ComboBox
        {
            Margin = new Thickness(0, Container.Spacing, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemsSource = new string[] { "-" }.Concat(MainViewModel.Current.Variables.Select(v => v.Name)),
            FocusVisualStyle = null
        };

        clone.NeedUpdate += () => cb1.SelectedValue = ((string)property.GetValue(clone)).IsNull() ? "-" : GetPart((string)property.GetValue(clone), 0);

        cb1.SelectionChanged += (s, e) =>
        {
            cb2.Visibility = Visibility.Collapsed;
            cb2.ItemsSource = null;

            if ((string)cb1.SelectedItem == "-")
                property.SetValue(clone, "");
            else
            {
                var variable = MainViewModel.Current.Variables.First(v => v.Name == (string)cb1.SelectedItem);

                if ((variable.VariableType == VariableType.Point || variable.VariableType == VariableType.Color) && vAttr.Target == VariableType.Number)
                {
                    cb2.Visibility = Visibility.Visible;
                    cb2.ItemsSource = variable.GetSubValues();
                    cb2.SelectedItem = GetPart((string)property.GetValue(clone), 1, cb2.ItemsSource.OfType<string>());

                    property.SetValue(clone, $"{cb1.SelectedItem}.{cb2.SelectedItem}");
                }
                else if (variable.VariableType == vAttr.Target || vAttr.Target == VariableType.Text)
                {
                    property.SetValue(clone, $"{cb1.SelectedItem}");
                }
                else
                {
                    cb2.Visibility = Visibility.Visible;
                    cb2.ItemsSource = new string[] { "ConvertError!" };
                    cb2.SelectedItem = cb2.ItemsSource.OfType<string>().First();

                    property.SetValue(clone, "");
                }
            }
        };

        Grid.SetColumn(cb1, 0);
        Grid.SetRow(cb1, 1);
        control.Children.Add(cb1);

        control.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Container.Spacing) });
        control.ColumnDefinitions.Add(new ColumnDefinition());

        cb2 = new ComboBox
        {
            Margin = new Thickness(0, Container.Spacing, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            FocusVisualStyle = null
        };

        cb2.SelectionChanged += (s, e) => property.SetValue(clone, (string)cb1.SelectedItem == "-" ? "" : $"{cb1.SelectedItem}{(cb2 != null ? $".{cb2.SelectedItem}" : "")}");

        Grid.SetColumn(cb2, 2);
        Grid.SetRow(cb2, 1);
        control.Children.Add(cb2);

        return control;
    }

    private UIElement GetCheckBoxEditControl(PropertyInfo property, IEditProperties clone, CheckBoxEditPropertyAttribute checkAttr)
    {
        var control = new CheckBox
        {
            Content = checkAttr.Title != "-" ? checkAttr.Title ?? property.Name : "",
            FocusVisualStyle = null
        };

        clone.NeedUpdate += () => control.IsChecked = (bool)property.GetValue(clone);

        control.Checked += (s, e) => property.SetValue(clone, true);
        control.Unchecked += (s, e) => property.SetValue(clone, false);

        return control;
    }

    private UIElement GetComboBoxEditControl(PropertyInfo property, IEditProperties clone, ComboBoxEditPropertyAttribute cAttr)
    {
        var control = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            FocusVisualStyle = null
        };

        if (cAttr.Title != "-")
            ControlHelper.SetHeader(control, cAttr.Title ?? property.Name);

        if (cAttr.Source == ComboBoxEditPropertySource.Functions || cAttr.Source == ComboBoxEditPropertySource.Variables)
        {
            control.ItemsSource = new string[] { "-" }.Concat(
                cAttr.Source == ComboBoxEditPropertySource.Functions
                    ? MainViewModel.Current.CustomFunctions.Select(f => f.Title)
                    : cAttr.VariablesFilter == VariablesFilter.None
                        ? MainViewModel.Current.Variables.Select(f => f.Name)
                        : MainViewModel.Current.Variables.Where(v => (int)v.VariableType == (int)cAttr.VariablesFilter).Select(f => f.Name));

            clone.NeedUpdate += () => control.SelectedValue = ((string)property.GetValue(clone)).IsNull() ? "-" : (string)property.GetValue(clone);
            control.SelectionChanged += (s, e) => property.SetValue(clone, (string)control.SelectedItem == "-" ? "" : (string)control.SelectedItem);
        }
        else if (cAttr.Source == ComboBoxEditPropertySource.Boolean)
        {
            control.ItemsSource = new bool[] { true, false };

            clone.NeedUpdate += () => control.SelectedValue = (bool)property.GetValue(clone);
            control.SelectionChanged += (s, e) => property.SetValue(clone, (bool)control.SelectedValue);
        }
        else if (cAttr.Source == ComboBoxEditPropertySource.Enum)
        {
            control.ItemsSource = property.PropertyType
                .GetEnumNames()
                .Select(e => cAttr.TrimStart.IsNull()
                    ? e
                    : e[cAttr.TrimStart.Length..]
                );

            clone.NeedUpdate += () => control.SelectedValue = cAttr.TrimStart.IsNull()
                ? ((Enum)property.GetValue(clone)).Name()
                : ((Enum)property.GetValue(clone)).Name()?[cAttr.TrimStart.Length..];

            control.SelectionChanged += (s, e) => property.SetValue(clone, Enum.Parse(property.PropertyType, cAttr.TrimStart + (string)control.SelectedItem));
        }

        return control;
    }
}
