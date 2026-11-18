using System;
using UnityEngine;

// Template Method: Begin() and CompleteStep() are fixed, each step fills in OnBegin/OnEnd.
public abstract class RoutineStep : MonoBehaviour, IRoutineStep
{
    public string stepName = "Step";
    [TextArea] public string hint = "";
    [Tooltip("Where this step happens; Mirror's hint bubble floats above it.")]
    public Transform station;

    public string StepName => stepName;
    public virtual string Hint => hint;
    public bool IsComplete { get; private set; }
    public bool IsActive { get; private set; }
    public virtual float Progress => IsComplete ? 1f : 0f;

    public event Action<IRoutineStep> Completed;

    public void Begin()
    {
        if (IsActive || IsComplete) return;
        IsActive = true;
        OnBegin();
    }

    // called by the manager when time runs out
    public void Abort()
    {
        if (!IsActive) return;
        IsActive = false;
        OnEnd();
    }

    protected abstract void OnBegin();
    protected virtual void OnEnd() { }

    // editor testing only: finish this step straight away
    public void DebugSkip() { if (IsActive && !IsComplete) CompleteStep(); }

    protected void CompleteStep()
    {
        if (IsComplete) return;
        IsComplete = true;
        IsActive = false;
        OnEnd();
        Completed?.Invoke(this);
    }
}
