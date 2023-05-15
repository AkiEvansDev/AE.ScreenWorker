using ModernWpf.Controls;

using ScreenWorkerWPF.ViewModel;

namespace ScreenWorkerWPF.Model;

internal class VariableNavigationMenuItem : NavigationMenuItem
{
    public VariableNavigationMenuItem() : base("Variables", Symbol.AllApps, new VariablesViewModel(), null) { }

    public void OnAddVariable()
    {
        (Tab as VariablesViewModel).OnAdd();
    }
}
