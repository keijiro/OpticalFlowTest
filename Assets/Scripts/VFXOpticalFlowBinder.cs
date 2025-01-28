using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Optical Flow Binder")]
[VFXBinder("Optical Flow")]
public sealed class VFXOpticalFlowBinder : VFXBinderBase
{
    [VFXPropertyBinding("UnityEngine.Texture")]
    public ExposedProperty Property = "OpticalFlow";

    public OpticalFlowGenerator Target = null;

    public override bool IsValid(VisualEffect component)
      => Target != null && component.HasTexture(Property);

    public override void UpdateBinding(VisualEffect component)
    {
        if (Target.AsRenderTexture != null)
            component.SetTexture(Property, Target.AsRenderTexture);
    }

    public override string ToString()
      => $"OpticalFlow : '{Property}' -> " +
         (Target != null ? Target.name : "(null)");
}
