using UnityEngine;
using Unity.Mathematics;

namespace OpticalFlowTest {

static class RTUtils
{
    public static RenderTexture AllocColor(int2 dims)
      => new RenderTexture(dims.x, dims.y, 0);

    public static RenderTexture AllocColorNoFilter(int2 dims)
      => new RenderTexture(dims.x, dims.y, 0)
           { filterMode = FilterMode.Point };

    public static RenderTexture AllocHalf4(int2 dims)
      => new RenderTexture(dims.x, dims.y, 0, RenderTextureFormat.ARGBHalf);

    public static RenderTexture AllocHalf2(int2 dims)
      => new RenderTexture(dims.x, dims.y, 0, RenderTextureFormat.RGHalf);
}

} // namespace OpticalFlowTest
