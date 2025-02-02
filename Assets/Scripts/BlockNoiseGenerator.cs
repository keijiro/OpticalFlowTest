using UnityEngine;

namespace OpticalFlowTest {

public sealed class BlockNoiseGenerator : MonoBehaviour
{
    #region Project asset references

    [SerializeField] ComputeShader _compute = null;

    #endregion

    #region Public accessors

    public RenderTexture AsRenderTexture => _rt;

    #endregion

    #region Private members

    RenderTexture _rt;

    #endregion

    #region MonoBehaviour implementation

    void Start()
      => _rt = RTUtil.AllocSingleNoFilterUAV(Config.BlockDims);

    void OnDestroy()
      => Destroy(_rt);

    void Update()
    {
        _compute.SetInt("Seed", Time.frameCount);
        _compute.SetTexture(0, "Output", _rt);
        _compute.DispatchThreads(0, Config.BlockDims.y);
    }

    #endregion
}

} // namespace OpticalFlowTest
