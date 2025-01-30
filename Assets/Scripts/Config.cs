using UnityEngine;
using Unity.Mathematics;
using Klak.TestTools;

namespace OpticalFlowTest {

public sealed class Config : MonoBehaviour
{
    [SerializeField] int2 _flowDims = math.int2(160, 96);
    [SerializeField] int2 _blockDims = math.int2(40, 24);

    public static int2 FlowDims => _instance._flowDims;
    public static int2 BlockDims => _instance._blockDims;
    public static int2 SourceDims => ToInt2(_imageSource.OutputResolution);

    static Config _instance;
    static ImageSource _imageSource;

    static int2 ToInt2(Vector2Int v) => math.int2(v.x, v.y);

    void Awake()
    {
        _instance = this;
        _imageSource = FindFirstObjectByType<ImageSource>();
    }
}

} // namespace OpticalFlowTest
