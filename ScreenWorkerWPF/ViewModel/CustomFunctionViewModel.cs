using ScreenBase.Data;

using ScreenWorkerWPF.Model;

namespace ScreenWorkerWPF.ViewModel;

internal class CustomFunctionViewModel : FunctionViewModelBase
{
    public CustomFunctionViewModel() : base(true, true)
    {
        Items.Add(new ActionItem(Items, new CommentAction(), OnEdit, null) { IsSelected = true });
    }
}