using TMPro;
using UnityEngine;

public class AvatarBillboard : MonoBehaviour
{
    public TextMeshPro textMesh;
    public float maxChars = 200;

    void Awake()
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();
    }

    public void SetUserSpeech(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            textMesh.text = "";
            return;
        }

        if (text.Length > maxChars)
            text = text.Substring(0, (int)maxChars) + "...";

        textMesh.text = "You: " + text;
    }

    public void SetAiReply(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            textMesh.text = "";
            return;
        }

        if (text.Length > maxChars)
            text = text.Substring(0, (int)maxChars) + "...";

        textMesh.text = "AI: " + text;
    }

    public void SetStatus(string text)
    {
        textMesh.text = text;
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            var cam = Camera.main.transform;
            transform.LookAt(cam.position);
            transform.Rotate(0, 180f, 0);
        }
    }
}
