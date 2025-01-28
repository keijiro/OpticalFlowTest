using UnityEngine;
using Klak.TestTools;

public sealed class OpticalFlowGenerator : MonoBehaviour
{
    [SerializeField] Vector2Int _resolution = new Vector2Int(160, 90);
    [SerializeField] ImageSource _source = null;
    [SerializeField] float _frameRate = 30;

    [SerializeField, HideInInspector] Shader _gradShader = null;
    [SerializeField, HideInInspector] Shader _flowShader = null;

    public RenderTexture AsRenderTexture => _rt.flow;

    float _elapsed;
    (Material grad, Material flow) _material;
    (RenderTexture prev, RenderTexture curr,
     RenderTexture grad, RenderTexture flow) _rt;

    void Setup(RenderTexture source)
    {
        _material.grad = new Material(_gradShader);
        _material.flow = new Material(_flowShader);

        var (w, h) = (_resolution.x, _resolution.y);
        _rt.prev = new RenderTexture(w, h, 0);
        _rt.curr = new RenderTexture(w, h, 0);
        _rt.grad = new RenderTexture(w, h, 0, RenderTextureFormat.ARGBHalf);
        _rt.flow = new RenderTexture(w, h, 0, RenderTextureFormat.RGHalf);

        _material.grad.SetTexture("_PrevTex", _rt.prev);
    }

    void OnDestroy()
    {
        Destroy(_material.grad);
        Destroy(_material.flow);
        Destroy(_rt.prev);
        Destroy(_rt.curr);
        Destroy(_rt.grad);
        Destroy(_rt.flow);
    }

    void Update()
    {
        _elapsed += Time.deltaTime;
        if (_elapsed < 1 / _frameRate) return;
        _elapsed -= 1 / _frameRate;

        var source = _source.AsRenderTexture;
        if (_material.grad == null) Setup(source);

        Graphics.Blit(source, _rt.curr);
        Graphics.Blit(_rt.curr, _rt.grad, _material.grad, 0);
        Graphics.Blit(_rt.grad, _rt.flow, _material.flow, 0);
        Graphics.Blit(_rt.curr, _rt.prev);
    }
}
