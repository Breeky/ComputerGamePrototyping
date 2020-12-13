using UnityEngine;

[System.Serializable]
public class Sound
{
    public string soundName;
    public string absolutePath;

    [Range(0f, 1f)] public float volume;
    [Range(0.1f, 3f)] public float pitch;
}
