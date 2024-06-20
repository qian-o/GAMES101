﻿using Maths;

namespace PA5;

internal struct Camera(Vector3d position, Angle fov)
{
    public Vector3d Position = position;

    public Angle Fov = fov;
}