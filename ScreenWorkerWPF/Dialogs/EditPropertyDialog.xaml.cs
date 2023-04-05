using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using AE.Core;
using AE.CoreWPF;

using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;

using Ookii.Dialogs.Wpf;

using ScreenBase.Data.Base;
using ScreenBase.Data.Game;

using ScreenWorkerWPF.Common;
using ScreenWorkerWPF.ViewModel;
using ScreenWorkerWPF.Windows;

namespace ScreenWorkerWPF.Dialogs;

public partial class EditPropertyDialog : ContentDialog
{
    private readonly Dictionary<string, UIElement> controls;

    public static Task<ContentDialogResult> ShowAsync(IEditProperties source, string title)
    {
        var dialog = new EditPropertyDialog(source, title);
        return dialog.ShowAsync(ContentDialogPlacement.Popup);
    }

    public EditPropertyDialog(IEditProperties source, string title)
    {
        controls = new Dictionary<string, UIElement>();
        InitializeComponent();

        var clone = source.Clone();
        if (clone is IAction action)
        {
            Title = new TextBlock { Text = title };
            Task.Run(async () =>
            {
                var info = await CommonHelper.GetHelpInfo(action.Type);
                if (info != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var textBlock = new TextBlock();
                        FormattedTextBlockBehavior.SetFormattedData(textBlock, info.Data);

                        (Title as TextBlock).ToolTip = new ToolTip
                        {
                            Content = textBlock,
                            MaxWidth = Application.Current.MainWindow.Width / 2,
                        };
                    });
                }
            });
        }
        else
            Title = title;

        var properties = clone
            .GetType()
            .GetProperties()
            .Where(p => p.GetCustomAttribute<EditPropertyAttribute>() != null)
            .OrderBy(p => p.GetCustomAttribute<EditPropertyAttribute>().Order)
            .ToList();

        if (properties.Any())
        {
            var groups = new List<KeyValuePair<int, List<PropertyInfo>>>();

            var current = new KeyValuePair<int, List<PropertyInfo>>(
                properties.Min(p => p.GetCustomAttribute<GroupAttribute>()?.Group ?? 0) - 1,
                new List<PropertyInfo>()
            );
            groups.Add(current);

            foreach (var property in properties)
            {
                var groupAttr = property.GetCustomAttribute<GroupAttribute>();
                if (groupAttr != null)
                {
                    if (groups.Any(g => g.Key == groupAttr.Group))
                    {
                        var item = groups.First(g => g.Key == groupAttr.Group);
                        item.Value.Add(property);
                    }
                    else
                    {
                        var item = new KeyValuePair<int, List<PropertyInfo>>(
                            groupAttr.Group,
                            new List<PropertyInfo> { property }
                        );
                        groups.Add(item);

                        current = new KeyValuePair<int, List<PropertyInfo>>(
                            current.Key - 1,
                            new List<PropertyInfo>()
                        );
                        groups.Add(current);
                    }
                }
                else
                {
                    current.Value.Add(property);
                }

            }

            foreach (var group in groups)
            {
                var positions = group.Value
                    .GroupBy(p => p.GetCustomAttribute<GroupAttribute>()?.Position ?? 0)
                    .OrderBy(g => g.Key)
                    .ToList();

                if (positions.Count == 1)
                {
                    DrawGroup(Container, clone, positions
                        .First()
                        .GroupBy(p => p.GetCustomAttribute<EditPropertyAttribute>().Order, p => p)
                        .OrderBy(g => g.Key)
                        .ToList()
                    );
                }
                else if (positions.Count > 1)
                {
                    var panel = new Grid();
                    panel.ColumnDefinitions.Add(new ColumnDefinition());

                    var index = 0;
                    foreach (var item in positions)
                    {
                        index++;
                        var control = new SimpleStackPanel
                        {
                            Spacing = Container.Spacing,
                        };

                        DrawGroup(control, clone, item
                            .GroupBy(p => p.GetCustomAttribute<EditPropertyAttribute>().Order, p => p)
                            .OrderBy(g => g.Key)
                            .ToList()
                        );

                        Grid.SetColumn(control, panel.ColumnDefinitions.Count - 1);
                        panel.Children.Add(control);

                        if (index != positions.Count)
                        {
                            panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Container.Spacing * 2) });
                            panel.ColumnDefinitions.Add(new ColumnDefinition());
                        }
                    }

                    Container.Children.Add(panel);
                }
            }
        }

        if (source is ScriptSettings)
        {
            Container.Children.Add(new TextBlock
            {
                FontSize = 12,
                Opacity = 0.6,
                Margin = new Thickness(0, Container.Spacing * 2, 0, 0),
                Text = $"Ver.: {CommonHelper.GetVersionString()}"
            });
        }

        clone.NeedUpdateInvoke();

        Closing += (s, e) =>
        {
            if (e.Result == ContentDialogResult.Primary)
            {
                foreach (var property in properties)
                    property.SetValue(source, property.GetValue(clone));
            }
        };
    }

    private void DrawGroup(Panel container, IEditProperties clone, List<IGrouping<int, PropertyInfo>> properties)
    {
        var isLastCB = false;
        foreach (var order in properties)
        {
            if (order.First().GetCustomAttribute<SeparatorAttribute>() != null)
            {
                container.Children.Add(new Separator());
            }

            if (order.Count() == 1)
            {
                var control = GetControl(order.First(), clone);
                if (control != null)
                {
                    if (isLastCB && control is CheckBox checkBox)
                        checkBox.Margin = new Thickness(0, -Container.Spacing, 0, 0);

                    container.Children.Add(control);
                    isLastCB = control is CheckBox;
                }
            }
            else if (order.Count() > 1)
            {
                var panel = new Grid();
                panel.ColumnDefinitions.Add(new ColumnDefinition());

                var index = 0;
                foreach (var item in order)
                {
                    index++;

                    var control = GetControl(item, clone);
                    if (control != null)
                    {
                        Grid.SetColumn(control, panel.ColumnDefinitions.Count - 1);
                        panel.Children.Add(control);

                        if (index != order.Count())
                        {
                            panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Container.Spacing) });
                            panel.ColumnDefinitions.Add(new ColumnDefinition());
                        }
                    }
                }

                if (isLastCB && panel.Children[0] is CheckBox)
                    panel.Margin = new Thickness(0, -Container.Spacing, 0, 0);

                container.Children.Add(panel);
                isLastCB = panel.Children[0] is CheckBox;
            }
        }
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
        else if (property.PropertyType == typeof(ScreenRange) && attr is ScreenRangeEditPropertyAttribute rAttr)
        {
            result = GetScreenRangeEditControl(property, clone, rAttr);
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
        else if (property.PropertyType == typeof(string) && attr is ImageEditPropertyAttribute iAttr)
        {
            result = GetImageEditControl(property, clone, iAttr);
        }
        else if (property.PropertyType == typeof(bool) && attr is CheckBoxEditPropertyAttribute checkAttr)
        {
            result = GetCheckBoxEditControl(property, clone, checkAttr);
        }
        else if ((property.PropertyType == typeof(bool) || property.PropertyType == typeof(string) || property.PropertyType.IsEnum) && attr is ComboBoxEditPropertyAttribute cAttr)
        {
            result = GetComboBoxEditControl(property, clone, cAttr);
        }
        else if (property.PropertyType.IsGenericType && attr is MoveEditPropertyAttribute mAttr)
        {
            result = GetMoveEditControl(property, clone, mAttr);
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
        var viewColor = new ColorPicker
        {
            ColorOpacityEnabled = pAttr.UseOpacityColor,
        };

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
            ScreenRange range = null;

            if (!pAttr.ColorRangeProperty.IsNull())
            {
                var rangeProperty = clone
                    .GetType()
                    .GetProperty(pAttr.ColorRangeProperty);

                if (rangeProperty != null)
                    range = (ScreenRange)rangeProperty.GetValue(clone);
            }

            var point = ScreenWindow.GetPoint(oldPoint, range);

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

    private UIElement GetScreenRangeEditControl(PropertyInfo property, IEditProperties clone, ScreenRangeEditPropertyAttribute rAttr)
    {
        var viewColor = new ColorPicker();
        var control = new Button
        {
            Content = rAttr.Title,
            Margin = new Thickness(0, Container.Spacing, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            FocusVisualStyle = null,
        };

        control.Click += (s, e) =>
        {
            var old = (ScreenRange)property.GetValue(clone);
            var data = ScreenWindow.GetRange(old);

            if (data != null)
                property.SetValue(clone, data);
        };

        return control;
    }

    private UIElement GetTextEditControl(PropertyInfo property, IEditProperties clone, TextEditPropertyAttribute tAttr)
    {
        if (tAttr.IsPassword)
            return GetPasswordEditControl(property, clone, tAttr);

        if (!tAttr.VariantsProperty.IsNull())
            return GetTextEditControlWithVariant(property, clone, tAttr);

        var control = new TextBox
        {
            FocusVisualStyle = null,
            MaxWidth = 300,
        };

        Container.SizeChanged += (s, e) => control.MaxWidth = Container.ActualWidth;

        if (tAttr.Title != "-")
            ControlHelper.SetHeader(control, tAttr.Title ?? property.Name);

        clone.NeedUpdate += () => control.Text = (string)property.GetValue(clone);
        control.TextChanged += (s, e) => property.SetValue(clone, control.Text);

        return control;
    }

    private UIElement GetPasswordEditControl(PropertyInfo property, IEditProperties clone, TextEditPropertyAttribute tAttr)
    {
        var control = new PasswordBox
        {
            FocusVisualStyle = null,
            MaxWidth = 300,
        };

        Container.SizeChanged += (s, e) => control.MaxWidth = Container.ActualWidth;

        if (tAttr.Title != "-")
            ControlHelper.SetHeader(control, tAttr.Title ?? property.Name);

        clone.NeedUpdate += () => control.Password = (string)property.GetValue(clone);
        control.PasswordChanged += (s, e) => property.SetValue(clone, control.Password);

        return control;
    }

    private UIElement GetTextEditControlWithVariant(PropertyInfo property, IEditProperties clone, TextEditPropertyAttribute tAttr)
    {
        var data = clone
            .GetType()
            .GetProperty(tAttr.VariantsProperty)
            .GetValue(clone) as Dictionary<string, string>;

        var control = new AutoSuggestBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemsSource = data.Keys,
            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
            FocusVisualStyle = null,
            MaxWidth = 300,
        };

        Container.SizeChanged += (s, e) => control.MaxWidth = Container.ActualWidth;

        if (tAttr.Title != "-")
            ControlHelper.SetHeader(control, tAttr.Title ?? property.Name);

        clone.NeedUpdate += () => control.Text = (string)property.GetValue(clone);
        control.TextChanged += (s, e) =>
        {
            if (data.Keys.Contains(control.Text))
                control.Text = data[control.Text];

            property.SetValue(clone, control.Text);
        };

        return control;
    }

    private UIElement GetPathEditControl(PropertyInfo property, IEditProperties clone, LoadEditPropertyAttribute lAttr)
    {
        PropertyInfo nameProperty = null;
        if (!lAttr.PropertyName.IsNull())
            nameProperty = clone.GetType().GetProperty(lAttr.PropertyName);

        var control = new TextBox
        {
            IsReadOnly = true,
            FocusVisualStyle = null,
            MaxWidth = 300,
        };

        Container.SizeChanged += (s, e) => control.MaxWidth = Container.ActualWidth;

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
                    InitialDirectory = control.Text,
                    FileName = Path.Combine(control.Text, nameProperty == null ? sAttr.DefaultName : (string)nameProperty.GetValue(clone)),
                };
            else
                dialog = new VistaOpenFileDialog
                {
                    Filter = lAttr.Filter,
                    InitialDirectory = control.Text,
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
        var control = new TextBox
        {
            IsReadOnly = true,
            FocusVisualStyle = null,
            MaxWidth = 300,
        };

        Container.SizeChanged += (s, e) => control.MaxWidth = Container.ActualWidth;
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
            Grid.SetColumnSpan(cb1, 3);
            cb2.Visibility = Visibility.Collapsed;
            cb2.ItemsSource = null;

            if ((string)cb1.SelectedItem == "-")
                property.SetValue(clone, "");
            else
            {
                var variable = MainViewModel.Current.Variables.First(v => v.Name == (string)cb1.SelectedItem);

                if ((variable.VariableType == VariableType.Point || variable.VariableType == VariableType.Color) && vAttr.Target == VariableType.Number)
                {
                    Grid.SetColumnSpan(cb1, 1);
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
                    Grid.SetColumnSpan(cb1, 1);
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

    private UIElement GetImageEditControl(PropertyInfo property, IEditProperties clone, ImageEditPropertyAttribute iAttr)
    {
        var control = new TextBox
        {
            IsReadOnly = true,
            FocusVisualStyle = null,
            Tag = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            MaxWidth = 300,
        };

        Container.SizeChanged += (s, e) => control.MaxWidth = Container.ActualWidth;

        var image = new Image
        {
            FocusVisualStyle = null,
        };

        clone.NeedUpdate += () =>
        {
            var data = (string)property.GetValue(clone);

            if (!data.IsNull())
            {
                var bytes = Convert.FromBase64String(data);
                using var stream = new MemoryStream();

                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;

                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = stream;
                bi.EndInit();

                image.Source = bi;

                control.Text = "(image)";
                control.ToolTip = image;
            }
            else
            {
                control.Text = "(null)";
                control.ToolTip = null;
            }
        };

        control.PreviewMouseLeftButtonUp += (s, e) =>
        {
            var dialog = new VistaOpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;",
                InitialDirectory = (string)control.Tag,
            };

            if (dialog.ShowDialog(Application.Current.MainWindow).GetValueOrDefault())
            {
                control.Tag = Path.GetDirectoryName(dialog.FileName);

                try
                {
                    var img = System.Drawing.Image.FromFile(dialog.FileName);

                    using var stream = new MemoryStream();
                    img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                    var bytes = stream.ToArray();
                    property.SetValue(clone, Convert.ToBase64String(bytes));

                }
                catch
                {
                    property.SetValue(clone, null);
                }

                clone.NeedUpdateInvoke();
            }
        };

        if (iAttr.Title != "-")
            ControlHelper.SetHeader(control, iAttr.Title ?? property.Name);

        return control;
    }

    private UIElement GetMoveEditControl(PropertyInfo property, IEditProperties clone, MoveEditPropertyAttribute mAttr)
    {
        PropertyInfo displayProperty = null;
        if (!mAttr.PropertyName.IsNull())
            displayProperty = clone.GetType().GetProperty(mAttr.PropertyName);

        var control = new TextBox
        {
            IsReadOnly = true,
            FocusVisualStyle = null,
            MaxWidth = 300,
        };

        Container.SizeChanged += (s, e) => control.MaxWidth = Container.ActualWidth;
        clone.NeedUpdate += () => control.Text = (string)displayProperty.GetValue(clone);

        control.PreviewMouseLeftButtonUp += (s, e) =>
        {
            var paths = (IEnumerable<MovePart>)property.GetValue(clone);

            var result = MovePathWindow.Show(paths);
            if (result != null)
            {
                property.SetValue(clone, result);
                clone.NeedUpdateInvoke();
            }
        };

        if (mAttr.Title != "-")
            ControlHelper.SetHeader(control, mAttr.Title ?? property.Name);

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
        else if (cAttr.Source == ComboBoxEditPropertySource.Fonts)
        {
            control.ItemsSource = Fonts.SystemFontFamilies
                .Select(f => f.Source)
                .OrderBy(f => f)
                .ToList();

            clone.NeedUpdate += () =>
            {
                var value = (string)property.GetValue(clone);

                if (value.IsNull())
                    value = control.ItemsSource.OfType<string>().First();

                control.SelectedValue = value;
            };
            control.SelectionChanged += (s, e) => property.SetValue(clone, (string)control.SelectedValue);
        }

        return control;
    }
}
