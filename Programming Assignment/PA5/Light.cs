using System.Runtime.InteropServices;
using Maths;

namespace PA5;

[StructLayout(LayoutKind.Sequential)]
internal struct Light(Vector3d position, Vector3d intensity)
{
    public Vector3d Position { get; set; } = position;

    public Vector3d Intensity { get; set; } = intensity;
}
