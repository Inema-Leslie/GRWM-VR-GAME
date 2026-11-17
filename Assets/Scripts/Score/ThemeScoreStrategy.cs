using UnityEngine;

// 2 hearts for finishing the routine, 1 for time left, 2 for matching the day's theme.
public class ThemeScoreStrategy : IScoreStrategy
{
    public float ThemeMatch(ScoreContext c)
    {
        if (c.theme == null || c.outfit == null || c.outfit.Count == 0) return 0f;
        int hits = 0;
        foreach (var item in c.outfit) if (item && item.Matches(c.theme)) hits++;
        return hits / 3f;   // top, bottom, shoes
    }

    public int Hearts(ScoreContext c)
    {
        float steps = c.stepsTotal > 0 ? (float)c.stepsDone / c.stepsTotal : 0f;
        float time = c.timeLimit > 0f ? Mathf.Clamp01(c.timeLeft / c.timeLimit * 2f) : 0f;
        float total = 2f * steps + 1f * time + 2f * Mathf.Clamp01(ThemeMatch(c));
        return Mathf.Clamp(Mathf.RoundToInt(total), 1, 5);
    }
}
