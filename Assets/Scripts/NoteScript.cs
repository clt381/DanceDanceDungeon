using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteScript : MonoBehaviour {

    //needs references to sprites
    public Sprite[] buttons;

    //bool to check if it has changed
    [HideInInspector]
    public bool changed = false;
}
