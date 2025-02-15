using UnityEngine;
using Unity.Mathematics;
using Klak.TestTools;

namespace OpticalFlowTest {

public sealed class Datamosh : MonoBehaviour
{
    #region Scene object references

    [SerializeField] ImageSource _imageSource = null;
    [SerializeField] BlockPicker _blockPicker = null;
    [SerializeField] OpticalFlowEstimator _flowSource = null;
    [SerializeField] RenderTexture _destination = null;

    #endregion

    #region Editable properties

    [field:SerializeField] public float Interval { get; set; } = 1;
    [field:SerializeField] public float FlowAmplitude { get; set; } = 2;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] ComputeShader _compute = null;
    [SerializeField, HideInInspector] Shader _shader = null;

    #endregion

    #region Private members

    Blitter _blitter;
    (RenderTexture flow, RenderTexture buffer) _rt;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _blitter = new Blitter(_shader);
        _rt.flow = RTUtil.AllocHalf2UAV(Config.BlockDims);
        _rt.buffer = RTUtil.AllocColorNoFilter(Config.SourceDims);
    }

    void OnDestroy()
    {
        _blitter.Dispose();
        Destroy(_rt.flow);
        Destroy(_rt.buffer);
    }

    void Update()
    {
        // Feedback
        Graphics.Blit(_destination, _rt.buffer);

        // Flow map downsampling
        _compute.SetTexture(0, "FlowMap", _flowSource.AsRenderTexture);
        _compute.SetTexture(0, "Output", _rt.flow);
        _compute.SetInt("Stride", Config.FlowDims.x / Config.BlockDims.x);
        _compute.DispatchThreads(0, Config.BlockDims);

        // Datamosh pass
        _blitter.Material.SetTexture("_SourceTex", _imageSource.AsTexture);
        _blitter.Material.SetTexture("_FlowTex", _rt.flow);
        _blitter.Material.SetTexture("_BlockTex", _blockPicker.AsRenderTexture);
        _blitter.Material.SetFloat("_FlowAmp", FlowAmplitude);
        _blitter.Run(_rt.buffer, _destination, 0);
    }

    #endregion
}

} // namespace OpticalFlowTest
