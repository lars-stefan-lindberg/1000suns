using FMOD.Studio;

public static class AudioUtils
{
    public static void SafeStop(
        ref EventInstance instance,
        STOP_MODE stopMode = STOP_MODE.ALLOWFADEOUT
    )
    {
        if (!instance.isValid())
            return;

        instance.getPlaybackState(out var state);

        if (state != PLAYBACK_STATE.STOPPED)
        {
            instance.stop(stopMode);
        }

        instance.release();

        instance.clearHandle();
    }

    public static bool IsPlaying(EventInstance instance) {
        if (!instance.isValid())
            return false;

        instance.getPlaybackState(out var state);
        return state != PLAYBACK_STATE.STOPPED;
    }
}
