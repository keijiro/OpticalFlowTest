using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace OpticalFlowTest {

[AddComponentMenu("VFX/Property Binders/Block Picker Binder")]
[VFXBinder("Block Picker")]
public sealed class VFXBlockBinder : VFXBinderBase
{
    [VFXPropertyBinding("UnityEngine.Texture")]
    public ExposedProperty Property = "Block";

    public BlockPicker Target = null;

    public override bool IsValid(VisualEffect component)
      => Target != null && component.HasTexture(Property);

    public override void UpdateBinding(VisualEffect component)
    {
        if (Target.AsRenderTexture != null)
            component.SetTexture(Property, Target.AsRenderTexture);
    }

    public override string ToString()
      => $"BlockPicker : '{Property}' -> " +
         (Target != null ? Target.name : "(null)");
}

} // namespace OpticalFlowTest
