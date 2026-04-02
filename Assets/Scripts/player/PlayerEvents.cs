public static class PlayerEvents
{
    public static event System.Action OnTentExitComplete;
    
    public static void TriggerTentExitComplete() {
        OnTentExitComplete?.Invoke();
    }
}
