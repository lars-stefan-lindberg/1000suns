using System;

[Serializable]
public class CaveTimeline
{
    private CaveTimelineId.Id timelineId;

    public CaveTimeline(CaveTimelineId.Id timelineId) {
        this.timelineId = timelineId;
    }

    public bool IsTimeline(CaveTimelineId caveTimelineId) {
        return caveTimelineId.id == timelineId;
    }

    public void SetTimeline(CaveTimelineId.Id timelineId) {
        this.timelineId = timelineId;
    }

    public void Clear() {
        timelineId = default;
    }
}
