using System;

// One task in the morning routine (alarm, teeth, face, outfit...).
public interface IRoutineStep
{
    string StepName { get; }
    string Hint { get; }
    bool IsComplete { get; }
    float Progress { get; }          // 0..1
    void Begin();
    event Action<IRoutineStep> Completed;
}
