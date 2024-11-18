using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DCharacter : MonoBehaviour
{
    public string cName;
    public Emotion Emotion;
    public AudioClip[] ChatSE;
    public AudioClip[] CallSE;
}
