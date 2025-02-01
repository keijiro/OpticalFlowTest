using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;
using Klak.TestTools;

namespace OpticalFlowTest {

[AddComponentMenu("VFX/Property Binders/Image Source Binder")]
[VFXBinder("Image Source")]
public sealed class VFXImageSourceBinder : VFXBinderBase
{
    [VFXPropertyBinding("UnityEngine.Texture")]
    public ExposedProperty Property = "ImageSource";

    public ImageSource Target = null;

    public override bool IsValid(VisualEffect component)
      => Target != null && component.HasTexture(Property);

    public override void UpdateBinding(VisualEffect component)
    {
        if (Target.AsTexture != null)
            component.SetTexture(Property, Target.AsTexture);
    }

    public override string ToString()
      => $"ImageSource : '{Property}' -> " +
         (Target != null ? Target.name : "(null)");
}

} // namespace OpticalFlowTest
