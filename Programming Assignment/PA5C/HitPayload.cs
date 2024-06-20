namespace PA5;

internal struct HitPayload(int objectIndex, Intersection intersection)
{
    public int ObjectIndex = objectIndex;

    public Intersection Intersection = intersection;

    public static HitPayload False => new(-1, Intersection.False);
}
