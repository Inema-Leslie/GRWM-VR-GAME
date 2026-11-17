using System;
using System.Collections.Generic;
using UnityEngine;

// Runs the steps in order, starts the clock once the alarm is off, and scores the day at the end.
public class RoutineManager : MonoBehaviour
{
    public DayTheme theme;
    public GameTimer timer;
    public ScoreManager score;
    public List<RoutineStep> steps = new List<RoutineStep>();

    public RoutineStep Current { get; private set; }
    public int CurrentIndex { get; private set; } = -1;
    public int CompletedCount { get; private set; }
    public bool Finished { get; private set; }
    public int Hearts { get; private set; }

    public event Action<RoutineStep> StepStarted;
    public event Action<RoutineStep> StepCompleted;
    public event Action<int> DayFinished;

    // Glow Meter: finished steps plus how far into the current one
    public float Glow => steps.Count == 0 ? 0f :
        (CompletedCount + (Current && !Finished ? Current.Progress : 0f)) / steps.Count;

    void Start()
    {
        if (timer) timer.TimeUp += Finish;
        foreach (var s in steps) s.Completed += OnStepDone;
        StartStep(0);
    }

    void StartStep(int i)
    {
        if (i >= steps.Count) { Finish(); return; }
        CurrentIndex = i;
        Current = steps[i];
        StepStarted?.Invoke(Current);
        Current.Begin();
    }

    void OnStepDone(IRoutineStep step)
    {
        if (Finished) return;
        CompletedCount++;
        StepCompleted?.Invoke((RoutineStep)step);
        // the morning rush starts once you're out of bed
        if (CurrentIndex == 0 && timer && !timer.Running) timer.StartTimer(theme ? theme.timeLimit : 300f);
        StartStep(CurrentIndex + 1);
    }

    void Finish()
    {
        if (Finished) return;
        Finished = true;
        if (Current && !Current.IsComplete) Current.Abort();
        if (timer) timer.Stop();

        var outfit = new List<ClothingItem>();
        foreach (var s in steps)
            if (s is OutfitStep o) outfit.AddRange(o.Outfit);

        var ctx = new ScoreContext
        {
            stepsDone = CompletedCount,
            stepsTotal = steps.Count,
            timeLeft = timer ? timer.Remaining : 0f,
            timeLimit = timer ? timer.Duration : 0f,
            outfit = outfit,
            theme = theme
        };
        Hearts = score ? score.Calculate(ctx) : 0;
        DayFinished?.Invoke(Hearts);
    }
}
