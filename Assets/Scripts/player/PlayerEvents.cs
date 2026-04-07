public static class PlayerEvents
{
    public static event System.Action OnTentExitComplete;
    public static event System.Action OnForestGlyphTouched;
    
    public static void TriggerTentExitComplete() {
        OnTentExitComplete?.Invoke();
    }
    
    public static void TriggerForestGlyphTouched() {
        OnForestGlyphTouched?.Invoke();
    }
}
