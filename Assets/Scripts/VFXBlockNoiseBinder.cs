using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace OpticalFlowTest {

[AddComponentMenu("VFX/Property Binders/Block noise Binder")]
[VFXBinder("Block Noise")]
public sealed class VFXBlockNoiseBinder : VFXBinderBase
{
    [VFXPropertyBinding("UnityEngine.Texture")]
    public ExposedProperty Property = "BlockNoise";

    public BlockNoiseGenerator Target = null;

    public override bool IsValid(VisualEffect component)
      => Target != null && component.HasTexture(Property);

    public override void UpdateBinding(VisualEffect component)
    {
        if (Target.AsRenderTexture != null)
            component.SetTexture(Property, Target.AsRenderTexture);
    }

    public override string ToString()
      => $"BlockNoise : '{Property}' -> " +
         (Target != null ? Target.name : "(null)");
}

} // namespace OpticalFlowTest
