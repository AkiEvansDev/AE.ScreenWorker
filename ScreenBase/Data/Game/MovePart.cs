using System;

using AE.Core;

using ScreenBase.Data.Base;

namespace ScreenBase.Data.Game;

[AESerializable]
public class MovePart : IEditProperties
{
    [NumberEditProperty(0, minValue: 1)]
    public int Count { get; set; }

    [NumberEditProperty(1000, $"{nameof(DelayAfter)} (ms)", smallChange: 50, largeChange: 1000)]
    public int DelayAfter { get; set; }

    public MoveType MoveType { get; set; }

    public MovePart()
    {
        Count = 20;
        DelayAfter = 100;
    }

    public MovePart(MoveType moveType) : this()
    {
        MoveType = moveType;
    }

    public event Action NeedUpdate;
    public void NeedUpdateInvoke()
    {
        NeedUpdate?.Invoke();
    }

    public IEditProperties Clone()
    {
        return DataHelper.Clone<MovePart>(this);
    }
}
