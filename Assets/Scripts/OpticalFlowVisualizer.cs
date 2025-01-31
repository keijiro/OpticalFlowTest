using UnityEngine;
using Klak.TestTools;

namespace OpticalFlowTest {

public sealed class OpticalFlowVisualizer : MonoBehaviour
{
    [SerializeField] ImageSource _source = null;
    [SerializeField] OpticalFlowEstimator _estimator = null;

    [HideInInspector, SerializeField] Mesh _mesh = null;
    [HideInInspector, SerializeField] Shader _shader = null;

    Material _material;

    void Start()
      => _material = new Material(_shader);

    void OnDestroy()
      => Destroy(_material);

    void Update()
    {
        _material.mainTexture = _source.AsTexture;
        _material.SetTexture("_FlowTex", _estimator.AsRenderTexture);
        Graphics.DrawMesh
          (_mesh, transform.localToWorldMatrix, _material, gameObject.layer);
    }
}

} // namespace OpticalFlowTest
