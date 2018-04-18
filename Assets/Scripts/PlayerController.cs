using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    UIController uiController;
    AudioController myAudioController;
    BeatWindowTrigger beatTrigger;
    ShowComboScript showCombo;

    Fencer fencer;
    ShieldMaiden maiden;
    Leviathan leviathan;

    //storing transforms of slots to switch agents between
    public Transform slot1;
    public Transform slot2;
    public Transform slot1HealthBar;                //NEED TO LERP THESE TO EACH OTHER'S PREVIOUS POSITIONS MAYBE
    public Transform slot2HealthBar;
    Transform player1;
    Transform player2;
    string fencerMoveAnim = "Fencer_Move";
    string shieldMaidenMoveAnim = "ShieldMaiden_Move";

    [Header("Abilities")]
    //list of possible combinations   
    public string move = "AAAB";
    public string fencerMaidenSpecial = "XXYY";
    public float specialManaCost = 100f;
    public string holyLance = "AAAX";
    public string skewerAndSlash = "AXXA";
    public float skewerAndSlashCost = 20f;
    public string riposte = "ABBX";
    public float riposteCost = 25f;
    public string divineBulwark = "AAAY";
    public string healingWord = "AYYA";
    public float healingWordCost = 20f;
    public string healingRune = "ABBY";
    public float healingRuneCost = 25f;
    [HideInInspector]
    public List<string> fencerAbilities = new List<string>();
    [HideInInspector]
    public List<string> maidenAbilities = new List<string>();

    [Header("Streak Info")]
    //storing streak progress
    public int streak;              //increments or decrements based on combinations executed THIS VALUE IS UPDATED BY BEATWINDOWTRIGGER ON EXECUTE COMBINATION
    public int streakStage;             //thresholds for activating streaks
    public int streakLostDelay = 5;     //number of beats that pass before a streak is considered lost
    public int nullBeats;                   //increment for each empty beat while player can input
    public float dmgMultiplier;       //referenced by fencer and shieldmaiden scripts

    [Header("Mana Info")]
    public float maxMana = 100f;                   //shared mana pool, used by higher tier abilities
    float mappedMana;
    [HideInInspector]
    public float minMana = 0f;
    public float curMana = 0f;
    public float manaPerNote = 5f;                  //amount gained per note change
    public Image manaUI;
    public Animator manaAnimator;

    [Header("Input Control")]
    public bool pressed;                //if we pressed one of the 4 input keys
    public bool alreadyPressed = false;
    public string pressedKey;               //what we pressed

    public bool canPress = false;           //bool to control rate of input; this should be set to true on first note entering beatwindowtrigger
    [HideInInspector]
    public bool gameStarted = false;

    //cooldowntime on canpress; seems to work between 0.15 and 0.3
    float cooldownTime = 0.15f;

    public Image[] showKeys;
    Color unpressedColor = new Color();

	void Start () {
        uiController = GameObject.Find("Stats").GetComponent<UIController>();
        myAudioController = GameObject.Find("AudioController").GetComponent<AudioController>();
        beatTrigger = GameObject.Find("BeatWindow").GetComponent<BeatWindowTrigger>();
        showCombo = GameObject.Find("ShowCombination").GetComponent<ShowComboScript>();
        unpressedColor = showKeys[0].color;

        fencer = GameObject.Find("Fencer").GetComponent<Fencer>();
        maiden = GameObject.Find("ShieldMaiden").GetComponent<ShieldMaiden>();
        leviathan = GameObject.Find("Leviathan").GetComponent<Leviathan>();

        curMana = minMana;                      //setting mana to zero on start
        dmgMultiplier = 1;                          //starting with no multiplier for damage

        fencerAbilities.Add(skewerAndSlash);
        fencerAbilities.Add(holyLance);
        fencerAbilities.Add(riposte);
        fencerAbilities.Add(fencerMaidenSpecial);

        maidenAbilities.Add(divineBulwark);
        maidenAbilities.Add(healingWord);
        maidenAbilities.Add(healingRune);
    }

    void Update()
    {
        if (gameStarted && canPress)
        {
            if (Input.GetKeyDown(KeyCode.A) && !alreadyPressed)
            {
                alreadyPressed = true;                                  //TO MAKE IT EASIER, MOVE THIS INTO IF(BEATTRIGGER ACTIVE) CONDITIONAL
                if (beatTrigger.triggerActive)
                {
                    pressed = true;
                    pressedKey = "A";
                    StartCoroutine(HighlightKey(showKeys[0]));
                }
                else if (!beatTrigger.triggerActive)
                {
                    MissedKey();
                }
                
            }
            else if (Input.GetKeyDown(KeyCode.S) && !alreadyPressed)
            {
                alreadyPressed = true;
                if (beatTrigger.triggerActive)
                {
                    pressed = true;
                    pressedKey = "B";
                    StartCoroutine(HighlightKey(showKeys[1]));
                }
                else if(!beatTrigger.triggerActive)
                {
                    MissedKey();
                }
            }
            else if (Input.GetKeyDown(KeyCode.D) && !alreadyPressed)
            {
                alreadyPressed = true;
                if (beatTrigger.triggerActive)
                {
                    pressed = true;
                    pressedKey = "X";
                    StartCoroutine(HighlightKey(showKeys[2]));
                }
                else if (!beatTrigger.triggerActive)
                {
                    MissedKey();
                }
            }
            else if (Input.GetKeyDown(KeyCode.F) && !alreadyPressed)
            {
                alreadyPressed = true;
                if (beatTrigger.triggerActive)
                {
                    pressed = true;
                    pressedKey = "Y";
                    StartCoroutine(HighlightKey(showKeys[3]));
                }
                else if (!beatTrigger.triggerActive)
                {
                    MissedKey();
                }
            }
        }

        curMana = Mathf.Clamp(curMana, minMana, maxMana);
        mappedMana = Map(curMana, minMana, maxMana, 0, 1);
        manaUI.fillAmount = mappedMana;
	}               

    public void MissedKey()
    {
        showCombo.ClearButtons();           //clear the number of active notes and then reset the combo  
        beatTrigger.activatedNotes.Clear();                 //if you  miss a fullnote, clear the array 
        UpdateStreak(0);               //add a nullnote
        print("Missed a key");
    }

    public void UpdateStreak(int increment)               //this doesn't manage music, it only manages player stat increases CALL THIS ON COMBINATION EXECUTED
    {
        bool inStreak = false;
        if (streak > 0)     //if the player is currently in a streak
        {
            inStreak = true;
        }

        if (increment != 0)
        {
            streak += increment;
            nullBeats = 0;          //reset null beats if player successfully hits a streak
        }
        else if (increment == 0)        //hard coded return that determines whether to reset to zero or not
        {
            if (!fencer.usingAbilities && !maiden.usingAbilities && inStreak)            //if player can use abilities (if not locked in animation) && player is actually in a streak
            {
                nullBeats++;
                if (nullBeats > streakLostDelay)   //if the player hasn't missed enough nullbeats yet
                {
                    if (myAudioController.curStreak != 0)
                    {
                        myAudioController.playTransition = true;
                        myAudioController.streakEnded = true;
                    }
                    streak = 0;
                    nullBeats = 0;          //resetting null beats
                }                
            }    
        }
        
        if (streak == 0)            //if after updating streak, it's equal to...
        {
            streakStage = 0;
            dmgMultiplier = 1f;
        }
        else if (streak == 5)               //if the player hits five successful combinations in a row
        {
            streakStage = 1;
            dmgMultiplier = 1.2f;
        }
        else if (streak == 12)
        {
            streakStage = 2;
            dmgMultiplier = 1.5f;
        }
        else if (streak == 20)
        {
            streakStage = 3;
            dmgMultiplier = 1.8f;
        }
        
    }

    IEnumerator HighlightKey(Image button)          //separate from triggering combinations
    {
        float elapsedTime = 0f;
        float highlightTime = 0.15f;   
        Color newColor = new Color(255/255, 255/255, 255/255, 255/255);     //completely opaque
        while (elapsedTime < highlightTime)
        {
            button.color = newColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        ResetButtons(button);
    }

    public IEnumerator SwitchSlots()
    {
        StartCoroutine(uiController.SwitchDynamicElements());   //switching the health bars and other elements
        player1 = slot1.GetChild(0);    //should grab transform of the first child
        player2 = slot2.GetChild(0);
        float elapsedTime = 0f;
        float moveTime = 0.6f;          //will take this long for animation to complete (they disappear before this timer runs out and reappear after)
        if (player1 != null && player2 != null)
        {
            Animator player1Anim = player1.GetComponentInChildren<Animator>();
            Animator player2Anim = player2.GetComponentInChildren<Animator>();

            if (player1.name == "Fencer")
            {
                player1Anim.SetTrigger(fencerMoveAnim);
            }
            else if (player1.name == "ShieldMaiden")
            {
                player1Anim.SetTrigger(shieldMaidenMoveAnim);
            }

            if (player2.name == "Fencer")
            {
                player2Anim.SetTrigger(fencerMoveAnim);
            }
            else if (player2.name == "ShieldMaiden")
            {
                player2Anim.SetTrigger(shieldMaidenMoveAnim);
            }

            while (elapsedTime < moveTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            player1.SetParent(slot2, false);               //switch their parent transforms; 'false' should also change their world space
            player2.SetParent(slot1, false);
        }
        
    }

    public void FencerMaidenSpecial()               //only usable with enough 'mana'?
    {
        player1 = slot1.GetChild(0);    //should grab transform of the first child
        player2 = slot2.GetChild(0);

        if (player1 != null && player2 != null)
        {
            Animator player1Anim = player1.GetComponentInChildren<Animator>();
            Animator player2Anim = player2.GetComponentInChildren<Animator>();

            if (player1.name == "Fencer")
                player1Anim.SetTrigger("Fencer_BlindingRay");
            else if (player1.name == "ShieldMaiden")
                player1Anim.SetTrigger("ShieldMaiden_BlindingRay");

            if (player2.name == "Fencer")
                player2Anim.SetTrigger("Fencer_BlindingRay");
            else if (player2.name == "ShieldMaiden")
                player2Anim.SetTrigger("ShieldMaiden_BlindingRay");
        }
    }

    public IEnumerator IncrementMana(float amount)              //to be called from beatwindowtrigger on note change and combination execution
    {
        float time = 0.15f;
        float elapsedTime = 0f;
        float startValue = curMana;
        float endValue = curMana + amount;
        if (amount > 0)                         //if we're positively incrementing our mana...
        {
            if (curMana < maxMana)
            {
                manaAnimator.SetTrigger("Mana_Increase");           //play mana gain animation only if we're not at full mana already
            }
            else if (curMana >= maxMana)
            {
                manaAnimator.SetBool("Mana_Full", true);            //maybe move this to update or else it will only trigger AFTER mana bar is already full
            }
        }
        else if (amount < 0)
        {
            time = 0.5f;                            //takes longer to decrement mana
            manaAnimator.SetBool("Mana_Full", false);
            manaAnimator.SetTrigger("Mana_Decrease");
            
        }
        while (elapsedTime < time)
        {
            curMana = Mathf.Lerp(startValue, endValue, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        curMana = endValue;             //just making sure
    }

    float Map(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;         //remapping stat progress to fillamount 
    }

    void ResetButtons(Image button)
    {
        button.color = unpressedColor;
    }

    public void ResetStreak()
    {
        streakStage = 0;
        streak = 0;
        nullBeats = 0;
        dmgMultiplier = 1;
    }
}
