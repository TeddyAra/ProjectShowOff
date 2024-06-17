using UnityEngine.Audio; 
using System; 
using UnityEngine;
using System.Security.Cryptography;

public class SFXManager : MonoBehaviour
{
    public SoundEffect[] soundEffects;

    private void Start()
    {
        DontDestroyOnLoad(gameObject); 
    }

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
        if (s == null)
        {
            return; 
        }
        if (s.source == null) 
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip; 

            s.source.volume = s.volume; 
            s.source.pitch = s.pitch; 
        }
        s.source.Play(); 
        
    }
}
