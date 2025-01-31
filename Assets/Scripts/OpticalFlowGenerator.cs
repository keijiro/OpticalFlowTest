using UnityEngine;
using Klak.TestTools;

namespace OpticalFlowTest {

public sealed class OpticalFlowGenerator : MonoBehaviour
{
    [SerializeField] ImageSource _source = null;
    [SerializeField] float _frameRate = 30;

    [SerializeField, HideInInspector] Shader _gradShader = null;
    [SerializeField, HideInInspector] Shader _flowShader = null;

    public RenderTexture AsRenderTexture => _rt.flow;

    float _elapsed;
    (Material grad, Material flow) _material;
    (RenderTexture prev, RenderTexture curr,
     RenderTexture grad, RenderTexture flow) _rt;

    void Start()
    {
        _material.grad = new Material(_gradShader);
        _material.flow = new Material(_flowShader);

        var dims = Config.FlowDims;
        _rt.prev = RTUtils.AllocColor(dims);
        _rt.curr = RTUtils.AllocColor(dims);
        _rt.grad = RTUtils.AllocHalf4(dims);
        _rt.flow = RTUtils.AllocHalf2(dims);
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

        Graphics.Blit(_source.AsRenderTexture, _rt.curr);
        Graphics.Blit(_rt.curr, _rt.grad, _material.grad, 0);
        Graphics.Blit(_rt.grad, _rt.flow, _material.flow, 0);
        Graphics.Blit(_rt.curr, _rt.prev);
    }
}

} // namespace OpticalFlowTest
