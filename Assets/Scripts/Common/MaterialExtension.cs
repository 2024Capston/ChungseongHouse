using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MaterialExtension
{
    private static readonly int ObjectColor_ID = Shader.PropertyToID("_ObjectColor");
    private static readonly int PlayerColor_ID = Shader.PropertyToID("_PlayerColor");
    private static readonly int ViewType_ID = Shader.PropertyToID("_ViewType");
    private static readonly int TargetColor_ID = Shader.PropertyToID("_TargetColor");
    private static readonly int Interpolate_ID = Shader.PropertyToID("_Interpolate");

    public static void SetObjectColor(this Material material, int colorType)
    {
        if (material.HasProperty(ObjectColor_ID))
        {
            material.SetInt(ObjectColor_ID, colorType);
        }
    }
    public static void SetObjectColor(this Material material, ColorType colorType) => SetObjectColor(material, (int)colorType);

    public static void SetPlayerColor(this Material material, int colorType)
    {
        if (material.HasProperty(PlayerColor_ID))
        {
            material.SetInt(PlayerColor_ID, colorType);
        }
    }
    public static void SetPlayerColor(this Material material, ColorType colorType) => SetPlayerColor(material, (int)colorType);

    public static void SetViewType(this Material material, int viewType)
    {
        if (material.HasProperty(ViewType_ID))
        {
            material.SetInt(ViewType_ID, viewType);
        }
    }

    public static void SetTargetColor(this Material material, int colorType)
    {
        if (material.HasProperty(TargetColor_ID))
        {
            material.SetInt(TargetColor_ID, colorType);
        }
    }
    public static void SetTargetColor(this Material material, ColorType colorType) => SetTargetColor(material, (int)colorType);

    public static void SetInterpolate(this Material material, float interpolate)
    {
        if (material.HasProperty(Interpolate_ID))
        {
            material.SetFloat(Interpolate_ID, interpolate);
        }
    }
    
    public static void SetMaterial(this Material material, ColorType objectColor, ColorType playerColor, int viewType)
    {
        SetPlayerColor(material, (int)playerColor);
        SetObjectColor(material, (int)objectColor);
        SetViewType(material, viewType);
    }
}
