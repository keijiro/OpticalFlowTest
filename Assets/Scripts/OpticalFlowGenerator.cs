using UnityEngine;
using UnityEngine.UIElements;
using Klak.TestTools;

public sealed class OpticalFlowGenerator : MonoBehaviour
{
    [SerializeField] ImageSource _source = null;
    [SerializeField] UIDocument _ui = null;

    [SerializeField, HideInInspector] Shader _gradShader = null;
    [SerializeField, HideInInspector] Shader _flowShader = null;

    float _elapsed;
    (Material grad, Material flow) _material;
    (RenderTexture prev, RenderTexture grad, RenderTexture flow) _rt;

    void Setup(RenderTexture source)
    {
        _material.grad = new Material(_gradShader);
        _material.flow = new Material(_flowShader);

        var (w, h) = (source.width, source.height);
        _rt.prev = new RenderTexture(w, h, 0);
        _rt.grad = new RenderTexture(w, h, 0, RenderTextureFormat.ARGBHalf);
        _rt.flow = new RenderTexture(w, h, 0, RenderTextureFormat.RGHalf);

        _material.grad.SetTexture("_PrevTex", _rt.prev);

        var root = _ui.rootVisualElement;
        root.Q("image1").style.backgroundImage = Background.FromRenderTexture(source);
        root.Q("image2").style.backgroundImage = Background.FromRenderTexture(_rt.grad);
        root.Q("image3").style.backgroundImage = Background.FromRenderTexture(_rt.flow);
    }

    void OnDestroy()
    {
        Destroy(_material.grad);
        Destroy(_material.flow);
        Destroy(_rt.prev);
        Destroy(_rt.grad);
        Destroy(_rt.flow);
    }

    void Update()
    {
        _elapsed += Time.deltaTime;
        if (_elapsed < 1.0f / 30) return;
        _elapsed -= 1.0f / 30;

        var source = _source.AsRenderTexture;
        if (_material.grad == null) Setup(source);

        Graphics.Blit(source, _rt.grad, _material.grad, 0);
        Graphics.Blit(_rt.grad, _rt.flow, _material.flow, 0);
        Graphics.Blit(source, _rt.prev);
    }
}
