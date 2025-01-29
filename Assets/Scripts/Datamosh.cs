using UnityEngine;
using Klak.TestTools;

public sealed class Datamosh : MonoBehaviour
{
    [SerializeField] Vector2Int _resolution = new Vector2Int(1280, 720);
    [SerializeField] ImageSource _source = null;
    [SerializeField] OpticalFlowGenerator _generator = null;
    [SerializeField] float _interval = 1;

    [SerializeField, HideInInspector] Shader _effectShader = null;
    [SerializeField, HideInInspector] Shader _displayShader = null;
    [SerializeField, HideInInspector] Mesh _quadMesh = null;

    (Material effect, Material display) _material;
    (RenderTexture source, RenderTexture dest) _rt;
    float _timer;

    void Start()
    {
        _material.effect = new Material(_effectShader);
        _material.display = new Material(_displayShader);

        var (w, h) = (_resolution.x, _resolution.y);
        _rt.source = new RenderTexture(w, h, 0);
        _rt.dest = new RenderTexture(w, h, 0);

        _rt.source.filterMode = FilterMode.Point;
        _rt.dest.filterMode = FilterMode.Point;
    }

    void OnDestroy()
    {
        Destroy(_material.effect);
        Destroy(_material.display);
        Destroy(_rt.source);
        Destroy(_rt.dest);
    }

    void Update()
    {
        _timer -= Time.deltaTime;

        _generator.AsRenderTexture.filterMode = FilterMode.Point;

        if (_timer <= 0)
        {
            Graphics.Blit(_source.AsTexture, _rt.source);
            _timer += _interval;
        }

        _material.effect.SetTexture("_FlowTex", _generator.AsRenderTexture);
        Graphics.Blit(_rt.source, _rt.dest, _material.effect, 0);

        _material.display.mainTexture = _rt.dest;
        Graphics.DrawMesh
          (_quadMesh, transform.localToWorldMatrix,
           _material.display, gameObject.layer);

        _rt = (_rt.dest, _rt.source);
    }
}
