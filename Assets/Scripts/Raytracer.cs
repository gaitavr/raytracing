using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raytracer : MonoBehaviour
{
    [SerializeField]
    private ComputeShader _rayTracingShader;

    [SerializeField]
    private Texture _skyboxTexture;

    private RenderTexture _target;
    private Camera _camera;

    private uint _currentSample = 0;
    private Material _addMaterial;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            _currentSample = 0;
            transform.hasChanged = false;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Render(destination);
    }

    private void Render(RenderTexture destination)
    {
        Init();

        _rayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        _rayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        SetShaderParameters();

        if (_addMaterial == null)
        {
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        }
        _addMaterial.SetFloat("_Sample", _currentSample);
        Graphics.Blit(_target, destination, _addMaterial);
        _currentSample++;
    }

    private void Init()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            _currentSample = 0;

            _target?.Release();

            _target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat,
                RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }
    }

    private void SetShaderParameters()
    {
        _rayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        _rayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        _rayTracingShader.SetTexture(0, "_SkyboxTexture", _skyboxTexture);
        _rayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
    }
}
