using UnityEngine;

namespace OpticalFlowTest {

public sealed class BlockPicker : MonoBehaviour
{
    #region Editable properties

    [field:SerializeField] int Iteration { get; set; } = 2;

    #endregion

    #region Public accessors

    public RenderTexture AsRenderTexture => _rt;

    #endregion

    #region Project asset references

    [SerializeField] ComputeShader _compute = null;

    #endregion

    #region Private members

    // This value must match kBufferSize in the compute shader.
    const int ThreadGroupBufferSize = 1024;

    RenderTexture _rt;
    int _dispatchCount;
    float _timer;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        var dims = Config.BlockDims;
        _rt = RTUtil.AllocIntNoFilterUAV(dims);
        _dispatchCount = (dims.x * dims.y - 1) / ThreadGroupBufferSize + 1;
    }

    void OnDestroy()
      => Destroy(_rt);

    void OnValidate()
      => Iteration = Mathf.Clamp(Iteration, 1, 32);

    void Update()
    {
        // Timer
        _timer -= Time.deltaTime;
        if (_timer > 0) return;
        _timer = Mathf.Pow(Random.value, 4) * 0.3f;

        // Zero clear
        Graphics.Blit(Texture2D.blackTexture, _rt);

        // Random fill
        _compute.SetInt("Seed", Time.frameCount);
        _compute.SetInt("Iteration", Iteration);
        _compute.SetInt("Iteration", Random.Range(1, 30));
        _compute.SetTexture(0, "Output", _rt);
        _compute.Dispatch(0, _dispatchCount, 1, 1);
    }

    #endregion
}

} // namespace OpticalFlowTest
