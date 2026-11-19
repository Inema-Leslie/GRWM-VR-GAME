using UnityEngine;

// 2 hearts for finishing the routine, 1 for time left, 2 for matching the day's theme.
public class ThemeScoreStrategy : IScoreStrategy
{
    public float ThemeMatch(ScoreContext c) => Match(c);

    public int Hearts(ScoreContext c) => Mathf.Clamp(Mathf.RoundToInt(Total(c)), 1, 5);

    public int GlowScore(ScoreContext c) => Mathf.Clamp(Mathf.RoundToInt(Total(c) * 20f), 0, 100);

    static float Total(ScoreContext c)
    {
        float steps = c.stepsTotal > 0 ? (float)c.stepsDone / c.stepsTotal : 0f;
        float time = c.timeLimit > 0f ? Mathf.Clamp01(c.timeLeft / c.timeLimit * 2f) : 0f;
        return 2f * steps + 1f * time + 2f * Mathf.Clamp01(Match(c));
    }

    static float Match(ScoreContext c)
    {
        if (c.theme == null || c.outfit == null || c.outfit.Count == 0) return 0f;
        int hits = 0;
        foreach (var item in c.outfit) if (item && item.Matches(c.theme)) hits++;
        return hits / (float)Mathf.Max(3, c.outfit.Count);   // top, bottom, shoes (+ accessory)
    }
}
