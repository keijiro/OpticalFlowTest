using UnityEngine;
using Unity.Mathematics;
using Klak.TestTools;

namespace OpticalFlowTest {

public sealed class Datamosh : MonoBehaviour
{
    #region Scene object references

    [SerializeField] ImageSource _imageSource = null;
    [SerializeField] OpticalFlowEstimator _flowSource = null;
    [SerializeField] RenderTexture _destination = null;

    #endregion

    #region Editable properties

    [field:SerializeField] public float Interval { get; set; } = 1;
    [field:SerializeField] public float FlowAmplitude { get; set; } = 2;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] Shader _shader = null;

    #endregion

    #region Private members

    Blitter _blitter;
    RenderTexture _buffer;
    float _timer;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _blitter = new Blitter(_shader);
        _buffer = RTUtil.AllocColorNoFilter(Config.SourceDims);
    }

    void OnDestroy()
    {
        _blitter.Dispose();
        Destroy(_buffer);
    }

    void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer > 0)
        {
            Graphics.Blit(_destination, _buffer);
        }
        else
        {
            Graphics.Blit(_imageSource.AsTexture, _buffer);
            _timer += Interval;
        }

        _blitter.Material.SetFloat("_FlowAmp", FlowAmplitude);
        _blitter.Material.SetTexture("_FlowTex", _flowSource.AsRenderTexture);
        _blitter.Run(_buffer, _destination, 0);
    }

    #endregion
}

} // namespace OpticalFlowTest
