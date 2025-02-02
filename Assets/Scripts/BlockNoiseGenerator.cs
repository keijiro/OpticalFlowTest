using UnityEngine;

namespace OpticalFlowTest {

public sealed class BlockNoiseGenerator : MonoBehaviour
{
    #region Project asset references

    [SerializeField] ComputeShader _compute = null;

    #endregion

    #region Editable properties

    [field:SerializeField] int Iteration { get; set;} = 4;

    #endregion

    #region Public accessors

    public RenderTexture AsRenderTexture => _rt;

    #endregion

    #region Private members

    // This value must match kBufferSize in the compute shader.
    const int ThreadBufferSize = 1024;

    RenderTexture _rt;
    int _threadCount;
    float _timer;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        var dims = Config.BlockDims;
        _rt = RTUtil.AllocIntNoFilterUAV(dims);
        _threadCount = (dims.x * dims.y - 1) / ThreadBufferSize + 1;
    }

    void OnDestroy()
      => Destroy(_rt);

    void Update()
    {
        // Timer
        _timer -= Time.deltaTime;
        if (_timer > 0) return;
        _timer = Mathf.Pow(Random.value, 4) * 0.1f;

        // Zero clear
        Graphics.Blit(Texture2D.blackTexture, _rt);

        // Random fill
        _compute.SetInt("Seed", Time.frameCount);
        _compute.SetInt("Iteration", Iteration);
        _compute.SetTexture(0, "Output", _rt);
        _compute.DispatchThreads(0, _threadCount);
    }

    #endregion
}

} // namespace OpticalFlowTest
