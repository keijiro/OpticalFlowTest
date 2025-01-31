using UnityEngine;
using Klak.TestTools;

namespace OpticalFlowTest {

public sealed class OpticalFlowEstimator : MonoBehaviour
{
    #region Scene object references

    [SerializeField] ImageSource _source = null;
    [SerializeField] float _frameRate = 30;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] Shader _gradShader = null;
    [SerializeField, HideInInspector] Shader _flowShader = null;

    #endregion

    #region Public accessors

    public RenderTexture AsRenderTexture => _output.flow;

    #endregion

    #region Private members

    float _elapsed;
    (Material grad, Material flow) _material;
    (RenderTexture prev, RenderTexture cur) _buffer;
    (RenderTexture grad, RenderTexture flow) _output;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _material.grad = new Material(_gradShader);
        _material.flow = new Material(_flowShader);
        _buffer.prev = RTUtils.AllocColor(Config.FlowDims);
        _buffer.cur  = RTUtils.AllocColor(Config.FlowDims);
        _output.grad = RTUtils.AllocHalf4(Config.FlowDims);
        _output.flow = RTUtils.AllocHalf2(Config.FlowDims);
        _material.grad.SetTexture("_PrevTex", _buffer.prev);
    }

    void OnDestroy()
    {
        Destroy(_material.grad);
        Destroy(_material.flow);
        Destroy(_buffer.prev);
        Destroy(_buffer.cur);
        Destroy(_output.grad);
        Destroy(_output.flow);
    }

    void Update()
    {
        _elapsed += Time.deltaTime;
        if (_elapsed < 1 / _frameRate) return;
        _elapsed -= 1 / _frameRate;

        Graphics.Blit(_source.AsRenderTexture, _buffer.cur);
        Graphics.Blit(_buffer.cur, _output.grad, _material.grad, 0);
        Graphics.Blit(_output.grad, _output.flow, _material.flow, 0);
        Graphics.Blit(_buffer.cur, _buffer.prev);
    }

    #endregion
}

} // namespace OpticalFlowTest
