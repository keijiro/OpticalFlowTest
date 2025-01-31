using UnityEngine;
using Unity.Mathematics;
using Klak.TestTools;

namespace OpticalFlowTest {

public sealed class Datamosh : MonoBehaviour
{
    [SerializeField] ImageSource _source = null;
    [SerializeField] OpticalFlowEstimator _estimator = null;
    [SerializeField] float _interval = 1;
    [SerializeField] float _vectorScale = 2;

    [SerializeField, HideInInspector] Shader _effectShader = null;
    [SerializeField, HideInInspector] Shader _displayShader = null;
    [SerializeField, HideInInspector] Mesh _quadMesh = null;

    (Material effect, Material display) _material;
    (RenderTexture src, RenderTexture dst) _rt;
    float _timer;

    void Start()
    {
        _material.effect = new Material(_effectShader);
        _material.display = new Material(_displayShader);
        _rt.src = RTUtils.AllocColorNoFilter(Config.SourceDims);
        _rt.dst = RTUtils.AllocColorNoFilter(Config.SourceDims);
    }

    void OnDestroy()
    {
        Destroy(_material.effect);
        Destroy(_material.display);
        Destroy(_rt.src);
        Destroy(_rt.dst);
    }

    void Update()
    {
        _timer -= Time.deltaTime;

        _estimator.AsRenderTexture.filterMode = FilterMode.Point;

        if (_timer <= 0)
        {
            Graphics.Blit(_source.AsTexture, _rt.src);
            _timer += _interval;
        }

        _material.effect.SetFloat("_VectorScale", _vectorScale);
        _material.effect.SetTexture("_FlowTex", _estimator.AsRenderTexture);
        Graphics.Blit(_rt.src, _rt.dst, _material.effect, 0);

        _material.display.mainTexture = _rt.dst;
        Graphics.DrawMesh
          (_quadMesh, transform.localToWorldMatrix,
           _material.display, gameObject.layer);

        _rt = (_rt.dst, _rt.src);
    }
}

} // namespace OpticalFlowTest
