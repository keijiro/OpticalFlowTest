using Unity.Mathematics;
using UnityEngine;
using Klak.TestTools;

namespace OpticalFlowTest {

public sealed class OpticalFlowEstimator : MonoBehaviour
{
    #region Scene object references

    [SerializeField] ImageSource _source = null;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] ComputeShader _diffDetector = null;
    [SerializeField, HideInInspector] Shader _shader = null;

    #endregion

    #region Public accessors

    public RenderTexture AsRenderTexture => _output.flow;

    #endregion

    #region Private members

    Blitter _blitter;
    (RenderTexture prev, RenderTexture cur) _buffer;
    (RenderTexture grad, RenderTexture flow) _output;
    RenderTexture _diffMask;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _blitter = new Blitter(_shader);
        _buffer.prev = RTUtils.AllocColor(Config.FlowDims);
        _buffer.cur  = RTUtils.AllocColor(Config.FlowDims);
        _output.grad = RTUtils.AllocHalf4(Config.FlowDims);
        _output.flow = RTUtils.AllocHalf2(Config.FlowDims);
        _diffMask = RTUtils.AllocUAV(math.int2(1, 1));

    }

    void OnDestroy()
    {
        _blitter.Dispose();
        Destroy(_buffer.prev);
        Destroy(_buffer.cur);
        Destroy(_output.grad);
        Destroy(_output.flow);
        Destroy(_diffMask);
    }

    void Update()
    {
        Graphics.Blit(_source.AsRenderTexture, _buffer.cur);

        _diffDetector.SetTexture(0, "Previous", _buffer.prev);
        _diffDetector.SetTexture(0, "Current", _buffer.cur);
        _diffDetector.SetTexture(0, "Output", _diffMask);
        _diffDetector.Dispatch(0, 1, 1, 1);

        _blitter.Material.SetTexture("_PrevTex", _buffer.prev);
        _blitter.Material.SetTexture("_DiffMask", _diffMask);
        _blitter.Run(_buffer.cur, _output.grad, 0);

        _blitter.Run(_output.grad, _output.flow, 1);

        _buffer = (_buffer.cur, _buffer.prev);
    }

    #endregion
}

} // namespace OpticalFlowTest
