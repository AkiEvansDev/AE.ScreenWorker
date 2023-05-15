using System.Collections.Generic;
using System.Linq;
using System.Threading;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Game;

[AESerializable]
public class GameMoveAction : BaseDelayAction<GameMoveAction>
{
    public override ActionType Type => ActionType.GameMove;

    public override string GetTitle() => $"GameMove({GetValueString(MovePath)});";
    public override string GetExecuteTitle(IScriptExecutor executor) => GetTitle();

    [AEIgnore]
    public string MovePath => GetPartsDisplay();

    [MoveEditProperty(nameof(MovePath), 0, nameof(MovePath))]
    public List<MovePart> Parts { get; set; }

    [ComboBoxEditProperty(1, trimStart: "Key", source: ComboBoxEditPropertySource.Enum)]
    public KeyFlags W { get; set; }

    [ComboBoxEditProperty(1, trimStart: "Key", source: ComboBoxEditPropertySource.Enum)]
    public KeyFlags S { get; set; }

    [ComboBoxEditProperty(2, trimStart: "Key", source: ComboBoxEditPropertySource.Enum)]
    public KeyFlags A { get; set; }

    [ComboBoxEditProperty(2, trimStart: "Key", source: ComboBoxEditPropertySource.Enum)]
    public KeyFlags D { get; set; }

    [NumberEditProperty(1000, minValue: 0)]
    public int PressDelay { get; set; }

    public GameMoveAction()
    {
        Parts = new List<MovePart>();

        W = KeyFlags.KeyW;
        S = KeyFlags.KeyS;
        A = KeyFlags.KeyA;
        D = KeyFlags.KeyD;
    }

    private string GetPartsDisplay()
    {
        var result = "";

        foreach (var part in Parts)
        {
            switch (part.MoveType)
            {
                case MoveType.Forward:
                    result += W.Name()[3..];
                    break;
                case MoveType.ForwardLeft:
                    result += W.Name()[3..];
                    result += A.Name()[3..];
                    break;
                case MoveType.ForwardRight:
                    result += W.Name()[3..];
                    result += D.Name()[3..];
                    break;
                case MoveType.Left:
                    result += A.Name()[3..];
                    break;
                case MoveType.Right:
                    result += D.Name()[3..];
                    break;
                case MoveType.Backward:
                    result += S.Name()[3..];
                    break;
                case MoveType.BackwardLeft:
                    result += S.Name()[3..];
                    result += A.Name()[3..];
                    break;
                case MoveType.BackwardRight:
                    result += S.Name()[3..];
                    result += D.Name()[3..];
                    break;
            }

            result += $"{part.Count} ";
        }

        return result.TrimEnd();
    }

    public override ActionResultType Do(IScriptExecutor executor, IScreenWorker worker)
    {
        if (Parts.Any(p => p.Count > 0))
        {
            foreach (var part in Parts)
            {
                if (part.Count <= 0)
                    continue;

                KeyFlags? k1 = null, k2 = null;

                switch (part.MoveType)
                {
                    case MoveType.Forward:
                        k1 = W;
                        break;
                    case MoveType.ForwardLeft:
                        k1 = W;
                        k2 = A;
                        break;
                    case MoveType.ForwardRight:
                        k1 = W;
                        k2 = D;
                        break;
                    case MoveType.Left:
                        k1 = A;
                        break;
                    case MoveType.Right:
                        k1 = D;
                        break;
                    case MoveType.Backward:
                        k1 = S;
                        break;
                    case MoveType.BackwardLeft:
                        k1 = S;
                        k2 = A;
                        break;
                    case MoveType.BackwardRight:
                        k1 = S;
                        k2 = D;
                        break;
                }

                for (var i = 0; i < part.Count; ++i)
                {
                    worker.KeyDown(k1.Value, false);
                    if (k2 != null)
                        worker.KeyDown(k2.Value, false);

                    if (PressDelay > 0)
                        Delay(executor, PressDelay);

                    worker.KeyUp(k1.Value);
                    if (k2 != null)
                        worker.KeyUp(k2.Value);
                }

                if (part.DelayAfter > 0)
                    Delay(executor, part.DelayAfter);
            }

            return ActionResultType.Completed;
        }
        else
        {
            executor.Log($"<E>{Type.Name()} ignored</E>", true);
            return ActionResultType.Cancel;
        }
    }
}
