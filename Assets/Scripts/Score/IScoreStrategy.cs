using System.Collections.Generic;

public struct ScoreContext
{
    public int stepsDone, stepsTotal;
    public float timeLeft, timeLimit;
    public List<ClothingItem> outfit;
    public DayTheme theme;
}

// Strategy: swap how a day is scored without touching the manager.
public interface IScoreStrategy
{
    int Hearts(ScoreContext c);           // 1..5
    float ThemeMatch(ScoreContext c);     // 0..1
}
