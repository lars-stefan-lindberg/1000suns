using System;

public static class IntroEvents
{
    public static event Action OnFirstForestRoomReady;
    
    public static void InvokeFirstForestRoomReady()
    {
        OnFirstForestRoomReady?.Invoke();
    }
    
    public static void ClearFirstForestRoomReady()
    {
        OnFirstForestRoomReady = null;
    }
}
