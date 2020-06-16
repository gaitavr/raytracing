using UnityEngine;

public struct Sphere
{
    public Vector3 Position;
    public float Radius;
    public Vector3 Albedo;
    public Vector3 Specular;
    public float Speed;
    public float Amplitude;

    public static int GetSize()
    {
        var floatSize = sizeof(float);
        return floatSize * 3 + floatSize + floatSize * 3 + floatSize * 3 + floatSize + floatSize;
    }
}
