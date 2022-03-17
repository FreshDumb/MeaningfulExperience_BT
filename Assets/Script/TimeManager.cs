using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void TimerDelegate();
public class TimerContainer
{
    public float endTime;
    public float duration;
    TimerDelegate endFunction;
    public bool going = true;
    public bool oneshot = true;
    private bool paused = false;
    public bool stopped = false;
    public TimerContainer(float _endTime, TimerDelegate _endfunction, float _duration, bool _oneshot = true)
    {
        endTime = _endTime;
        duration = _duration;
        endFunction = _endfunction;
        oneshot = _oneshot;
    }

    public void CancelTimer()
    {
        stopped = true;
        oneshot = true;
        PauseTimer();
    }

    public void TimerEnd()
    {
        if(stopped == false)
        {
            if (oneshot)
            {
                going = false;
            }
            endFunction?.Invoke();
        }
    }

    public void ResetTimer()
    {
        TimeManager.Instance.ResetTimer(this);
    }

    public void PauseTimer()
    {
        going = false;
        paused = true;
    }

    public bool IsPaused()
    {
        return paused;
    }

    public void ResumeTimer()
    {
        going = true;
        paused = false;
    }

    public void ResetTimerWrapper(float _endTime)
    {
        going = true;
        endTime = _endTime;
        paused = false;
    }
}

public class TimeManager : MonoBehaviour
{
    private List<TimerContainer> timers = new List<TimerContainer>();
    public int TimersCount;

    public static TimeManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void Update()
    {
        TimersCount = timers.Count;
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            if(timers[i].stopped)
            {
                timers.RemoveAt(i);
            }
            else
            {
                if (timers[i].IsPaused())
                {
                    timers[i].endTime += Time.deltaTime;
                }
                if ((timers[i].endTime <= Time.time && timers[i].IsPaused() == false))
                {
                    timers[i].TimerEnd();
                    if (timers[i].oneshot == true)
                    {
                        timers.RemoveAt(i);
                    }
                    else
                    {
                        timers[i].endTime += timers[i].duration;
                    }
                }
            }
        }
    }

    public TimerContainer SetTimer(float _duration, TimerDelegate _endfunction, bool _oneshot = true)
    {
        TimerContainer tempTimer = new TimerContainer(Time.time + _duration, _endfunction, _duration, _oneshot);
        timers.Add(tempTimer);
        return tempTimer;
    }

    public void ResetTimer(TimerContainer _timerToReset)
    {
        _timerToReset.ResetTimerWrapper(Time.time + _timerToReset.duration);
    }
}
