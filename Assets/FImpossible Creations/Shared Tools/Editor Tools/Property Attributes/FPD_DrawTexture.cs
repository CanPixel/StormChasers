using UnityEngine;

public class FPD_DrawTextureAttribute : PropertyAttribute
{
    public string path;
    public float width;
    public float height;
    public float labelWidth;
    public float fieldWidth;

    public FPD_DrawTextureAttribute(string path, float width, float height, float labelWidth = 0, float fieldWidth = 0)
    {
        this.path = path;
        this.width = width;
        this.height = height;
        this.labelWidth = labelWidth;
        this.fieldWidth = fieldWidth;
    }
}

