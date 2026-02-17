using System;
using System.Collections.Generic;

public class YG_ForbiddingEvents
{
    private readonly List<Func<bool>> _events = new List<Func<bool>>();

    public void AddEvent(Func<bool> eventMethod) => _events.Add(eventMethod);
    public void RemoveEvent(Func<bool> eventMethod) => _events.Remove(eventMethod);

    public bool IsForbidding {
        get {
            for (int i = 0; i < _events.Count; i++) {
                if (_events[i] != null && _events[i].Invoke()) return true;
            }

            return false;
        }
    }
}