using System.Collections;
using UnityEngine;
using System;
using extOSC;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public Sound[] musics;
    
    // OSC Connexion
    [SerializeField] private OSCTransmitter _transmitter;
    
    // Puredata
    public int currentIndex = 0;
    public int maxIndex = 2;
    

    public static AudioManager Instance;

    private void Start()
    {
        // One AudioManager only
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        
        // OSC initialization
        // _transmitter = GetComponent<OSCTransmitter>();
    }

    private void Update()
    {
        // SOUNDS
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("Start music.");
            PlayMusic("Potato");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Stop sounds & music.");
            StopSounds();
        }
    }

    public void PlaySound(string soundName)
    {
        // Find sound
        Sound s = Array.Find(sounds, sound => sound.soundName == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound '"+soundName+"' not found!");
            return;
        }
        
        if (currentIndex + 1 <= maxIndex)
        {
            currentIndex += 1;
        }
        else
        {
            currentIndex = 0;
        }
        
        // Play sound
        // Create message
        var message = new OSCMessage("/playSound");
        // Populate values.
        message.AddValue(OSCValue.Float((float)currentIndex));
        message.AddValue(OSCValue.String("useless"));  
        message.AddValue(OSCValue.String(s.absolutePath));      // filename
        message.AddValue(OSCValue.Float(s.volume)); // volume 
        message.AddValue(OSCValue.Float(s.pitch));  // pitch
        // Send message
        _transmitter.Send(message);
    }
    
    public void PlayMusic(string musicName)
    {
        // Find sound
        Sound s = Array.Find(musics, sound => sound.soundName == musicName);
        if (s == null)
        {
            Debug.LogWarning("Music '"+musicName+"' not found!");
            return;
        }

        // Play music
        // Create message
        var message = new OSCMessage("/playMusic");
        // Populate values.
        message.AddValue(OSCValue.String("useless"));  
        message.AddValue(OSCValue.String(s.absolutePath)); // filename
        message.AddValue(OSCValue.Float(s.volume)); // volume 
        message.AddValue(OSCValue.Float(s.pitch));  // pitch
        // Send message
        _transmitter.Send(message);
    }

    public void StopSounds()
    {
        // Create message
        var message = new OSCMessage("/stop");
        // Populate values.
        message.AddValue(OSCValue.String("useless"));
        // Send message
        _transmitter.Send(message);
    }

    private void OnApplicationQuit()
    {
        StopSounds();
    }
}
