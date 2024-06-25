using Maths;
using PA.Graphics;

namespace PA6;

internal class Triangle(Vertex a, Vertex b, Vertex c) : Geometry
{
    public Vertex A = a;

    public Vertex B = b;

    public Vertex C = c;

    public override Intersection GetIntersection(Ray ray)
    {
        Intersection intersection = new();

        Vector3d v0 = A.Position;
        Vector3d v1 = B.Position;
        Vector3d v2 = C.Position;

        Vector3d e1 = v1 - v0;
        Vector3d e2 = v2 - v0;
        Vector3d s0 = ray.Origin - v0;
        Vector3d s1 = Vector3d.Cross(ray.Direction, e2);
        Vector3d s2 = Vector3d.Cross(s0, e1);

        float invE1DotS1 = 1.0f / Vector3d.Dot(e1, s1);
        float tnear = Vector3d.Dot(s2, e2) * invE1DotS1;
        float b1 = Vector3d.Dot(s0, s1) * invE1DotS1;
        float b2 = Vector3d.Dot(ray.Direction, s2) * invE1DotS1;

        if (tnear >= 0 && b1 >= 0 && b2 >= 0 && (b1 + b2) <= 1)
        {
            intersection.Happened = true;
            intersection.Distance = tnear;
            intersection.Geometry = Handle;
            intersection.Position = ray.PointAt(tnear);
            intersection.Normal = Vector3d.Normalize(Vector3d.Cross(e1, e2));
            intersection.BarycentricCoords = new Vector3d(1.0f - b1 - b2, b1, b2);

            Vertex interpolation = Vertex.Interpolate(A, B, C, intersection.BarycentricCoords);

            intersection.TexCoord = interpolation.TexCoord;
        }

        return intersection;
    }

    public override Vector3d EvalDiffuseColor(Intersection intersection)
    {
        return new Vector3d(0.5f);
    }

    public override Bounds3d GetBounds()
    {
        return Bounds3d.Union(new Bounds3d(A.Position, B.Position), C.Position);
    }
}
