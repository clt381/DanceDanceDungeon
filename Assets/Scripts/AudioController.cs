using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour {

    PlayerController pController;

    //generic beat audioclips
    public AudioClip fullNoteBeat;
    public AudioClip halfNoteBeat;

    //every stage (chorusmain, chorusstreak1, chorusstreak2, chorusstreak3, etc) has up to four stages: 0(playing self), 3(transition to next streak), 1(endStreak), 2(loseStreak)
    //determine the state of the game

    //bools to determine when streaks have been entered or exited
    public bool streak1;
    public bool streak2;
    public bool streak3;
    public bool streakGained;
    public bool streakLost;
    public bool streakEnded;        //waited too long before next combo

    public bool playTransition;
    //public bool playStreak;

    public int curStreak;
    public int transition;

    public string curStage;                //current stage string
    public string nextClip;                 //name of next clip 
    public int notesSincePlayed;            //notes passed since the last audioclip started playing; incremented ontriggerenter
    int notesToNextAudio;                   //notes to next audioclip being played

    public AudioClip[] audioClips;

    public string[] clipNames12;                //clips that have 12 beats
    public string[] clipNames4;                 //clips that have 4 beats
    public string[] clipNames64;                //clips that have 64

    AudioSource myBeatAudio;

    public AudioSource[] mySongAudio;       //should have 2 to alternate between
    [HideInInspector]
    public int flip = 0;                       //changes between 0 and 1 to determine which audio source is playing; this will allow for some flexibility when transitioning

    void Start()
    {
        myBeatAudio = GetComponent<AudioSource>();
        
        pController = GameObject.Find("PlayerController").GetComponent<PlayerController>();

        curStage = "Chorus";            //starting at chorus for test
        notesToNextAudio = 0;
        notesSincePlayed = 0;
        SetNextAudioClip(mySongAudio[1 - flip]);
        flip = 1 - flip;
        //StartCoroutine(PlayNextAudioClip());
        //mySongAudio[0].Play();                          //for now, until we find a better way to handle switching
        
    }

    void Update()
    {
    }

    public void PlayFullBeat()
    {
        myBeatAudio.clip = fullNoteBeat;
        myBeatAudio.Play();
    }

    public void PlayHalfBeat()
    {
        myBeatAudio.clip = halfNoteBeat;
        myBeatAudio.Play();
    }

    public void PlayAudioClip()
    {
        notesSincePlayed = 0;
        //print(mySongAudio[flip].clip.name);                 //this should be called every 'length'
        //mySongAudio[flip].Play();                                       //play the current audio source
        SetNextAudioClip(mySongAudio[1 - flip]);            //set the audioclip of the other audiosource to the right clip
        flip = 1 - flip;                                                //0 or 1; switch the audiosource to play next
        notesToNextAudio = DetermineNextLength();
        StartCoroutine(PlayNextAudioClip());
    }

    public void SetNextAudioClip(AudioSource nextAudioSource)          //loads an audio source with the next clip to play
    {
        if (playTransition)                     //transitions take priority
        {
            //DELETED PLAYERCONTROLLER RELATED AUDIO CHANGES

        }
        else                //if not transitioning or playing a new streak, stick to default
        {
            transition = 0;
        }

        string clipToPlay = curStage;                   //example: chorus

        clipToPlay += curStreak.ToString();            //example: chorus1
        clipToPlay += transition.ToString();      //example: chorus10

        //print(clipToPlay);
        foreach (AudioClip clip in audioClips)
        {
            if (clip.name == clipToPlay)
            {
                nextAudioSource.clip = clip;
                nextClip = clip.name;
            }
        }
    }

    int DetermineNextLength()
    {
        foreach (string name in clipNames12)
        {
            if (nextClip == name)
            {
                return 12;
            }
        }
        foreach (string name in clipNames4)
        {
            if (nextClip == name)
            {
                return 4;
            }
        }
        return 0;
    }

    IEnumerator PlayNextAudioClip()
    {
        while (notesSincePlayed < notesToNextAudio)     //iterated per ontriggerexit
        {
            yield return null;
        }
        PlayAudioClip();
    }
}
