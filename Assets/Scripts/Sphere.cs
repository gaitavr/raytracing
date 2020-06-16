using UnityEngine;

public struct Sphere
{
    public Vector3 Position;
    public float Radius;
    public Vector3 Albedo;
    public Vector3 Specular;

    public void SetPosition(Vector3 pos)
    {
        Position = pos;
    }
}
