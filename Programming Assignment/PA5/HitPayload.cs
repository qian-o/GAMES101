namespace PA5;

internal struct HitPayload(Intersection intersection)
{
    public bool IsHit = true;

    public Intersection Intersection = intersection;

    public static HitPayload False => new() { IsHit = false, Intersection = Intersection.False };
}
