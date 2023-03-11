using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data;

[AESerializable]
public class CommentAction : BaseAction<CommentAction>
{
    public override ActionType Type => ActionType.Comment;

    public override string GetTitle() => $"<C>/* {GetTextForDisplay(Comment)} */</C>";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [TextEditProperty]
    public string Comment { get; set; }

    public CommentAction()
    {
        // Comment = "Add some commands here...";
    }

    public CommentAction(string comment = "")
    {
        Comment = comment;
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        return ActionResultType.False;
    }
}
