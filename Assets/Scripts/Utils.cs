using UnityEngine;
using Unity.Mathematics;

namespace OpticalFlowTest {

// Simple replacement of Graphics.Blit
class Blitter : System.IDisposable
{
    Material _material;

    public Material Material => _material;

    public Blitter(Shader shader)
      => _material = new Material(shader);

    public void Run(RenderTexture source, RenderTexture dest, int pass)
    {
        RenderTexture.active = dest;
        _material.mainTexture = source;
        _material.SetPass(pass);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 3, 1);
    }

    public void Dispose()
    {
        if (_material != null) Object.Destroy(_material);
    }
}

// Render texture allocation utility
static class RTUtil
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

    public static RenderTexture AllocUAV(int2 dims)
    {
        var rt = new RenderTexture(dims.x, dims.y, 0);
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }
}

// Graphics buffer allocation utility
static class GpuBufferUtil
{
    unsafe public static GraphicsBuffer Alloc<T>(int count) where T : unmanaged
      => new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, sizeof(T));
}

} // namespace OpticalFlowTest
