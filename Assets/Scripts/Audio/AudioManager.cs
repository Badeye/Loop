﻿using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    //Here we create an array for instances of our Sound class
    public Sound[] sounds;
    public AudioClip[] goalSounds;
    public AudioClip[] deathSounds;

    public static int deathCounter;
    public int deathCount = 4;

    public static AudioManager instance = null;
    public static bool playDeathSounds = false;

    private static bool firstStart = true;

    private bool pianoHasStarted = false;

    // Use this for initialization before Start()
    void Awake()
    {
        //We dont want that there can be multiple AudioManagers, whenever a new scene starts, so we use a Singleton pattern,
        //to check if there is already an instance of our AudioManager, and if yes, we just destroy it.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        //Each instance of our Sound class will now get a AudioSource that is stored in the variable "source", we defined in our Sound class
        //We then set the diffrent variables of our Sound class equals to the variables the AudioClip brings with it.
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    private void Start()
    {
        if (firstStart)
        {
            RythmManager.onBPM.AddListener(RythmCall);
            Game.onGameStateChange.AddListener(GameStateChanged);
            AudioManager.instance.Play("Background");
            //AudioManager.instance.Play("Radio");
            firstStart = false;
        }
    }

    private void Update()
    {
        if (!pianoHasStarted)
        {
            pianoHasStarted = true;
            AudioManager.RestartPiano();

        }
    }

    public void RythmCall(BPMinfo bpmInfo)
    {
        // bass plays on the level bpm
        if (bpmInfo.Equals(RythmManager.animationBPM))
            AudioManager.instance.Play("Bass");

        if (bpmInfo.Equals(RythmManager.playerBPM) && playDeathSounds)
        {
            playDeathSounds = false;
            Play("Death");
        }
    }

    private void GameStateChanged(Game.State newState)
    {
        switch (newState)
        {
            case Game.State.Playing:
                AudioManager.instance.SwitchGoalSound(Game.level);
                break;

            default: break;
        }
    }

    public static void RestartPiano()
    {
        AudioManager.instance.Play("Piano");
    }

    //This enables us to Play an AudioClip just through his name.
    //When using System; we can use the Array.Find Method.
    //We first define the array we want to look through.
    //Then we define a variable that refers to the element in the Sound array, which in our case is an instance of the Sound class.
    //We then want to find that sound which name is equal to the name given as an argument in the Play() method.
    //sound here is just a variable and can be named what ever you want, but .name is important, because we refer to the name of the sound in our Instance.
    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.Play();
    }

    public void SwitchGoalSound(int levelIndex)
    {
        AudioClip activeClip = goalSounds[levelIndex - 1];
        Sound s = Array.Find(sounds, sound => sound.name == "Goal");

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.clip = activeClip;
        s.source.clip = s.clip;
    }

    public void PlayRandomDeathSound()
    {
        if(deathSounds.Length == 0)
        {
            Debug.Log("NO DEATH SOUNDS IN ARRAY");
        }

        else
        {
            float maxElementIndex = deathSounds.Length - 1;
            float randomClipNumber = UnityEngine.Random.Range(0f, maxElementIndex);

            AudioClip randomClip = deathSounds[Mathf.RoundToInt(randomClipNumber)];
            Sound s = Array.Find(sounds, sound => sound.name == "Death");

            s.clip = randomClip;
            s.source.clip = s.clip;
            s.source.Play();
        }
    }
}
