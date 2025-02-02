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

    public void Run(Texture source, RenderTexture dest, int pass)
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

    public static RenderTexture AllocSingle(int2 dims)
      => new RenderTexture(dims.x, dims.y, 0, RenderTextureFormat.R8);

    public static RenderTexture AllocSingleNoFilter(int2 dims)
      => new RenderTexture(dims.x, dims.y, 0, RenderTextureFormat.R8)
           { filterMode = FilterMode.Point };

    public static RenderTexture AllocHalf4(int2 dims)
      => new RenderTexture(dims.x, dims.y, 0, RenderTextureFormat.ARGBHalf);

    public static RenderTexture AllocHalf2(int2 dims)
      => new RenderTexture(dims.x, dims.y, 0, RenderTextureFormat.RGHalf);

    public static RenderTexture AllocColorUAV(int2 dims)
    {
        var rt = AllocColor(dims);
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }

    public static RenderTexture AllocSingleUAV(int2 dims)
    {
        var rt = AllocSingle(dims);
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }

    public static RenderTexture AllocSingleNoFilterUAV(int2 dims)
    {
        var rt = AllocSingleNoFilter(dims);
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }

    public static RenderTexture AllocHalf2UAV(int2 dims)
    {
        var rt = AllocHalf2(dims);
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }
}

// Extension methods for ComputeShader
static class ComputeShaderExtensions
{
    public static void DispatchThreads
      (this ComputeShader compute, int kernel, int x, int y, int z)
    {
        uint xc, yc, zc;
        compute.GetKernelThreadGroupSizes(kernel, out xc, out yc, out zc);
        x = (x + (int)xc - 1) / (int)xc;
        y = (y + (int)yc - 1) / (int)yc;
        z = (z + (int)zc - 1) / (int)zc;
        compute.Dispatch(kernel, x, y, z);
    }

    public static void DispatchThreads
      (this ComputeShader compute, int kernel, int2 v)
      => DispatchThreads(compute, kernel, v.x, v.y, 1);

    public static void DispatchThreads
      (this ComputeShader compute, int kernel, int x)
      => DispatchThreads(compute, kernel, x, 1, 1);
}

// Graphics buffer allocation utility
static class GpuBufferUtil
{
    unsafe public static GraphicsBuffer Alloc<T>(int count) where T : unmanaged
      => new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, sizeof(T));
}

} // namespace OpticalFlowTest
