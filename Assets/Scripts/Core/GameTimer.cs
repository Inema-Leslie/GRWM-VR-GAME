using System;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public float Duration { get; private set; }
    public float Remaining { get; private set; }
    public bool Running { get; private set; }

    public event Action TimeUp;

    public void StartTimer(float seconds)
    {
        Duration = Remaining = seconds;
        Running = true;
    }

    public void Stop() => Running = false;

    void Update()
    {
        if (!Running) return;
        Remaining -= Time.deltaTime;
        if (Remaining > 0f) return;
        Remaining = 0f;
        Running = false;
        TimeUp?.Invoke();
    }
}
