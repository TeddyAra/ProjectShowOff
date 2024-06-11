using UnityEngine.Audio; 
using System; 
using UnityEngine;


public class SFXManager : MonoBehaviour
{
    public SoundEffect[] soundEffects;

    void Awake()
    {
        foreach (SoundEffect s in soundEffects)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip; 

            s.source.volume = s.volume; 
            s.source.pitch = s.pitch; 
        }
    }

    public void Play(string name)
    {
        SoundEffect s = Array.Find(soundEffects, sound => sound.name == name); 
        s.source.Play(); 
    }
}
