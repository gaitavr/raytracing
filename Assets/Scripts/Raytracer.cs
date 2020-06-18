using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raytracer : MonoBehaviour
{
    [SerializeField]
    private ComputeShader _rayTracingShader;
    [SerializeField]
    private Light _directionalLight;
    [SerializeField]
    private Texture _skyboxTexture;
    [SerializeField, Range(1f, 8f)]
    private int _reflectionsCount = 4;
    [SerializeField]
    private Vector2 _radiusRange = new Vector2(3.0f, 8.0f);
    [SerializeField]
    private uint _spheresCount = 100;
    [SerializeField]
    private float _spherePlacementRadius = 100f;

    private RenderTexture _target;
    private Camera _camera;

    private ComputeBuffer _sphereBuffer;
    private readonly List<Sphere> _spheres = new List<Sphere>();

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        SetupScene();
    }

    private void SetupScene()
    {
        _spheres.Clear();
        for (int i = 0; i < _spheresCount; i++)
        {
            var sphere = new Sphere();

            // Radius and radius
            sphere.Radius = _radiusRange.x + Random.value * (_radiusRange.y - _radiusRange.x);
            Vector2 randomPos = Random.insideUnitCircle * _spherePlacementRadius;
            sphere.Position = new Vector3(randomPos.x, sphere.Radius, randomPos.y);

            // Reject spheres that are intersecting others
            if (IsSphereIntersectsOther(_spheres, sphere))
            {
                continue;
            }

            // Albedo and specular color
            Color color = Random.ColorHSV();
            bool metal = Random.value < 0.8f;
            sphere.Albedo = metal ? Vector3.zero : new Vector3(color.r, color.g, color.b);
            sphere.Specular = metal ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;

            //Speed and amplitude
            sphere.Speed = Random.Range(0.1f, 5f);
            sphere.Amplitude = Random.Range(0.1f, 15f);

            // Add the sphere to the list
            _spheres.Add(sphere);
        }

        PassSpheres();
    }

    private void PassSpheres()
    {
        if (_sphereBuffer != null && _sphereBuffer.count != _spheres.Count)
        {
            _sphereBuffer.Release();
            _sphereBuffer = null;
        }
        if (_sphereBuffer == null)
        {
            _sphereBuffer = new ComputeBuffer(_spheres.Count, Sphere.GetSize());
        }
        _sphereBuffer.SetData(_spheres);
    }

    private bool IsSphereIntersectsOther(List<Sphere> spheres, Sphere sphere)
    {
        foreach (Sphere other in spheres)
        {
            float minDist = sphere.Radius + other.Radius;
            if (Vector3.SqrMagnitude(sphere.Position - other.Position) < 
                minDist * minDist)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDisable()
    {
        _sphereBuffer?.Release();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }

    private void Render(RenderTexture destination)
    {
        Init();

        _rayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        _rayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(_target, destination);
    }

    private void Init()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            _target?.Release();

            _target = new RenderTexture(Screen.width, Screen.height, 0, 
                RenderTextureFormat.ARGBFloat);
            _target.enableRandomWrite = true;
            _target.Create();
        }
    }

    private void SetShaderParameters()
    {
        _rayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        _rayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        _rayTracingShader.SetTexture(0, "_SkyboxTexture", _skyboxTexture);
        _rayTracingShader.SetInt("_ReflectionsCount", _reflectionsCount);

        Vector3 dir = _directionalLight.transform.forward;
        _rayTracingShader.SetVector("_DirectionalLight", new Vector4(dir.x, dir.y, dir.z, _directionalLight.intensity));
        if (_sphereBuffer != null)
        {
            _rayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);
        }

        _rayTracingShader.SetFloat("_Time", Time.time);
    }
}
