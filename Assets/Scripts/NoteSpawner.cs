using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour {

    //START AND END OF RHYTHM BAR
    public GameObject note;
    public Transform startPos;
    public Transform endPos;
    Transform canvasParent;
    AudioController myAudio;

    //SONG TRACKING
    float songPosition;         //position of song
    float songPosInBeats;       //postion of song in beats
    float beatsPerSecond;           //duration of beat?
    float beatTime;             //time between beats
    float nextBeatTime;         //time to next beat
    float dspTimeSongStart;      //time since song started
    float timeSongStart;
    float delay = 1f;           //use this if there is delay to song start (or else first note has an odd offset)
    //bool isHalfNote = false;

    bool songStarted = false;           //for garbage code

    //SONG INFORMATION
    float bpm = 120;          //beats per minute
    float[] notes;      //position in beats of the song
    float beatsInAdvance;       //number of beats to spawn before songs starts

    void Start()
    {
        //recording start of song
        timeSongStart = Time.time;
        canvasParent = GameObject.Find("Canvas").transform;
        myAudio = GameObject.Find("AudioController").GetComponent<AudioController>();
        //playing song or looping music
        if (GetComponent<AudioSource>() != null)
        {
            GetComponent<AudioSource>().Play();
        }

        beatTime = 60f / bpm;       //multiplied by 2 to accommodate halfnotes
        nextBeatTime = timeSongStart + beatTime + delay;
    }

    void FixedUpdate()
    {
        if (Time.time >= nextBeatTime)
        {
            SpawnNote();
            nextBeatTime += beatTime;
        }

    }

    void SpawnNote()    //alternates between half and full notes
    {
        if (!songStarted)           //temporary gross code
        {
            myAudio.mySongAudio[0].Play();
            songStarted = true;
        }


        GameObject notePrefab = Instantiate(note, startPos.position, transform.rotation, canvasParent);
        //List<GameObject> prefabChildren = new List<GameObject>();
        if (myAudio.fullNoteBeat != null)
        {
            myAudio.PlayFullBeat();
        }
        StartCoroutine(Move(notePrefab.transform));
        
    }

    IEnumerator Move(Transform _note)
    {
        float time = 5f;            //takes this long to move from start to end
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            _note.position = Vector2.Lerp(startPos.position, endPos.position, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(_note.gameObject);     //destroy the note after its reached its destination;
    }


}
