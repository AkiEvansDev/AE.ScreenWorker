using System.Windows;
using System.Windows.Controls;

using ScreenWorkerWPF.Model;
using ScreenWorkerWPF.ViewModel;

namespace ScreenWorkerWPF.Common;

internal class MenuItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate ItemTemplate { get; set; }
    public DataTemplate CustomItemTemplate { get; set; }
    public DataTemplate HeaderTemplate { get; set; }
    public DataTemplate SeparatorTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is NavigationMenuSeparator)
            return SeparatorTemplate;
        else if (item is NavigationMenuItem menuItem)
        {
            if (menuItem.Tab is CustomFunctionViewModel)
                return CustomItemTemplate;

            return ItemTemplate;
        }

        return HeaderTemplate;
    }
}

