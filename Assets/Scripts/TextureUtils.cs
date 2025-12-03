using UnityEngine;

public static class TextureUtils
{
    public static byte[] RenderTextureToPNG(RenderTexture rt)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        Object.Destroy(tex);

        RenderTexture.active = prev;
        return bytes;
    }
}
