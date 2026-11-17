using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    IScoreStrategy strategy = new ThemeScoreStrategy();

    public int LastHearts { get; private set; }
    public float LastThemeMatch { get; private set; }
    public event Action<int> Scored;

    public void SetStrategy(IScoreStrategy s) => strategy = s ?? strategy;

    public int Calculate(ScoreContext c)
    {
        LastThemeMatch = strategy.ThemeMatch(c);
        LastHearts = strategy.Hearts(c);
        Scored?.Invoke(LastHearts);
        return LastHearts;
    }
}
