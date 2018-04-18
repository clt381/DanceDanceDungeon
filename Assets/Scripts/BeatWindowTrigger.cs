using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeatWindowTrigger : MonoBehaviour {
    
    //needs communication from playercontroller about which button is currently active
    PlayerController playerController;
    AudioController myAudioController;
    GameObject pController;
    ShowComboScript showCombo;
    UIController uiController;

    public Leviathan leviathanScript;           //replace this with other enemyscripts for future levels
    public Fencer fencerScript;
    public ShieldMaiden maidenScript;

    public List<string> activatedNotes = new List<string>();         //to store ongoing combinations
    public List<string> fencerActivatedNotes = new List<string>();           //to store ongoing combinations for fencer
    public List<string> maidenActivatedNotes = new List<string>();
    public Sprite[] buttonSprites;

    public bool triggerActive = false;         //to mark beginning of notes entering trigger
    [HideInInspector]
    public bool noComboMatch = false;

    void Start() {
        playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        myAudioController = GameObject.Find("AudioController").GetComponent<AudioController>();
        uiController = GameObject.Find("Stats").GetComponent<UIController>();
        showCombo = GameObject.Find("ShowCombination").GetComponent<ShowComboScript>();
        leviathanScript = GameObject.Find("Leviathan").GetComponent<Leviathan>();
        fencerScript = GameObject.Find("Fencer").GetComponent<Fencer>();
        maidenScript = GameObject.Find("ShieldMaiden").GetComponent<ShieldMaiden>();
    }

    void UpdateCombination(string newNote)
    {
        //current notes at this point is 0-3       
        activatedNotes.Add(newNote);            //this makes it 1-4
        StartCoroutine(playerController.IncrementMana(playerController.manaPerNote));
        if (activatedNotes.Count < 4)
        {
            showCombo.UpdateButtons(activatedNotes.Count, newNote);
        }
        else if (activatedNotes.Count == 4)      //if after adding a new note, it becomes full
        {
            showCombo.UpdateButtons(activatedNotes.Count, newNote);
            CheckCombination();             //consider adding a mana related condition to the showcomboscript!!!
        }
        else if (activatedNotes.Count == 5)      //i.e. if its already full
        {
            activatedNotes.Remove(activatedNotes[0]);           //remove the first note, retaining 4 note length
            showCombo.UpdateButtons(6, newNote);            //returning 6 as an exception
            CheckCombination();                             //check it again after shifting the buttons
        }
    }

    void CheckCombination()         //THIS CODE executes player abilities
    {
        string newCombination = string.Empty;
        for (int i = 0; i < activatedNotes.Count; i++)
        {
            newCombination += activatedNotes[i];            //adds all activated notes to a new string at the same time
        }
        if (newCombination.Length == 4)
        {
            ExecuteCombination(newCombination);
        }
    }

    public void ExecuteCombination(string _combination)
    {
        if (_combination != playerController.move)
        {
            foreach (string ability in playerController.fencerAbilities)
            {
                if (_combination == ability)
                {
                    if (ability == playerController.fencerMaidenSpecial)            //exception because this requires both players to be available
                    {
                        if (maidenScript.canUseAbilities && fencerScript.canUseAbilities)
                        {
                            if (playerController.curMana >= playerController.maxMana)
                            {
                                fencerScript.UseAbility(_combination);
                                maidenScript.UseAbility(_combination);
                                //playerController.UpdateStreak(1);                       //move this to fencer and maiden scripts
                            }
                            else
                            {
                                //play not enough mana animation?
                                print("not enough mana!");
                            }
                        }
                    }
                    else
                    {
                        if (fencerScript.canUseAbilities)
                        {
                            fencerScript.UseAbility(_combination);
                            //playerController.UpdateStreak(1);
                        }
                    }
                }
            }

            foreach (string ability in playerController.maidenAbilities)
            {
                if (_combination == ability)
                {
                    if (maidenScript.canUseAbilities)
                    {
                        maidenScript.UseAbility(_combination);
                        //playerController.UpdateStreak(1);               
                    }
                }
            }
        }
        else if (_combination == playerController.move)
        {
            if (maidenScript.canUseAbilities && fencerScript.canUseAbilities)
            {
                StartCoroutine(playerController.SwitchSlots());
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        triggerActive = true;
        if (col.name == "FullNote")         //only increments on fullnotes
        {
            playerController.gameStarted = true;
            myAudioController.notesSincePlayed += 1;
            if (leviathanScript.countingNotes && !leviathanScript.regenerating)          //if it's in a state where it can record notes (not regenerating)
            {
                leviathanScript.notesPassed += 1;          //incrementing enemy's record of passed notes
                uiController.countDown -= 1;                        //decrements countdown
                uiController.UpdateCountDown(leviathanScript.nextAttackString);
            }
            maidenScript.notePassed = true;
            fencerScript.notePassed = true;
            leviathanScript.notePassed = true;
        }
    }

    void OnTriggerStay2D(Collider2D col)        //every frame the note is within the trigger
    {
        NoteScript noteScript = col.GetComponentInParent<NoteScript>();
        Transform button = col.GetComponentInChildren<Transform>();       //gets the 'button' child from fullnote or halfnote gameobject

        if (!noteScript.changed)
        {
            if (playerController.pressed)
            {
                StartCoroutine(ChangeSprite(button, noteScript, GetKeyNum(GetPressedKey())));
                UpdateCombination(GetPressedKey());
                playerController.pressed = false;
            }
        }
        else if (noteScript.changed)
        {
            if (playerController.pressed)           //if the player button spams, clear the notes
            {
                showCombo.ClearButtons();           //clear the number of active notes and then reset the combo  
                activatedNotes.Clear();                 //if you  miss a fullnote, clear the array 
                playerController.UpdateStreak(0);               //add a nullnote
                playerController.pressed = false;
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        triggerActive = false;
        playerController.alreadyPressed = false;                //so player can only trigger one button per note; separate from 'pressed' bool
        //check if buttons were pressed or not
        NoteScript noteScript = col.GetComponentInParent<NoteScript>();
        Transform button = col.GetComponentInChildren<Transform>();
        if (!noteScript.changed)
        {
            if (button.name == "HalfNote")              //ignore non changes in halfnote
            {
                return;
            }
            else if (button.name == "FullNote")
            {
                showCombo.ClearButtons();           //clear the number of active notes and then reset the combo  
                activatedNotes.Clear();                 //if you  miss a fullnote, clear the array 
                playerController.UpdateStreak(0);               //add a nullnote
            }
        }
    }

    string GetPressedKey()
    {
        return playerController.pressedKey;
    }

    int GetKeyNum(string key)
    {
        if (key == "A")
            return 0;
        else if (key == "B")
            return 1;
        else if (key == "X")
            return 2;
        else if (key == "Y")
            return 3;
        else
            return 4;
    }

    IEnumerator ChangeSprite(Transform _button, NoteScript _noteScript, int spriteNumber)
    {
        yield return new WaitForSeconds(0f);                
        if (_button.name == "FullNote")
        {
            if (!_noteScript.changed)
            {
                Image buttonSprite = _button.GetComponentInChildren<Image>();
                buttonSprite.sprite = buttonSprites[spriteNumber];
                buttonSprite.sprite = buttonSprites[spriteNumber];
                buttonSprite.color = new Color(1, 1, 1, 1);
                _noteScript.changed = true;
            }
        }
    }








}
