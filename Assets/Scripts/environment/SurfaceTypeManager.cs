public static class SurfaceTypeManager {
    public enum SurfaceType {
        Default,
        Rock,
        Roots,
        TreeBranch,
        Grass,
        Wood,
    }

    public static SurfaceType GetSurfaceType(string tag) {
        if(tag == "Rock")
            return SurfaceType.Rock;
        else if(tag == "Roots")
            return SurfaceType.Roots;
        else if(tag == "TreeBranch")
            return SurfaceType.TreeBranch;
        else if(tag == "Grass")
            return SurfaceType.Grass;
        else if(tag == "Wood")
            return SurfaceType.Wood;
        return SurfaceType.Default;
    }
}



