using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShieldMaiden : MonoBehaviour {

    PlayerController pController;
    AudioController aController;
    EffectsManager fxManager;
    //stats
    [Range(400f, 700f)]
    public float maxHealth = 600f;
    public float minHealth = 0f;
    [HideInInspector]
    public float curHealth;
    float mappedHealth;                 //mapped variable to fit fillamount of UI image

    public Image myHealthBar;           //UI healthbar component
    public Animator myHBAnim;               //animator atached to healthbar object (attached to heatlhbar parent)
    public Animator myEffectsAnim;
    Animator myAnim;

    Transform target;            //referenced by animator helper script; changes with each ability
    Transform mySlot;           //this determines whether to execute one ability or another; will change with the 'move' ability
    //[HideInInspector]
    public int targetSlot;
    [HideInInspector]
    public float damage;

    [HideInInspector]
    string slot1Trigger = "ShieldMaiden_HealingWord";
    public float healingWordAmount = 30f;                            //positive to increment health on partner
    public float healingRuneAmount = 5f; 
    string slot2Trigger = "ShieldMaiden_DivineBulwark";
    float slot2Amount = 7f;                             //how long to set invulnerability for (4 beats)

    //bools to be controlled via animation events
    public bool canTakeDamage = true;             //whether player can be damaged
    public bool canUseAbilities = true;
    public bool usingAbilities = false;

    //[HideInInspector]
    public bool notePassed = false;             //for notebased abilities

    void Start()
    {
        myAnim = GetComponentInChildren<Animator>();        //getting animator from graphics child object
        pController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        aController = GameObject.Find("AudioController").GetComponent<AudioController>();
        fxManager = GameObject.Find("Stats").GetComponent<EffectsManager>();
        curHealth = maxHealth;          //start with full health
        myHealthBar.fillAmount = 1;
    }

    void Update()
    {
        mappedHealth = Map(curHealth, minHealth, maxHealth, 0, 1);
        myHealthBar.fillAmount = mappedHealth;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            damage = healingRuneAmount;
            myAnim.SetTrigger("ShieldMaiden_HealingRune");
        }
    }

    public void UseAbility(string _combination)
    {
        if (canUseAbilities)                //to prevent potential multi-ability stacking
        {
            if (CheckMana(_combination) != false)
            {
                if (_combination == pController.divineBulwark)
                {
                    myAnim.SetTrigger("ShieldMaiden_DivineBulwark");
                    pController.UpdateStreak(1);
                }
                else if (_combination == pController.healingWord)
                {
                    myAnim.SetTrigger("ShieldMaiden_HealingWord");
                    StartCoroutine(pController.IncrementMana(-pController.healingWordCost));
                    pController.UpdateStreak(1);
                }
                else if (_combination == pController.healingRune)
                {
                    myAnim.SetTrigger("ShieldMaiden_HealingRune");
                    StartCoroutine(pController.IncrementMana(-pController.healingRuneCost));
                    pController.UpdateStreak(1);
                }
                else if (_combination == pController.fencerMaidenSpecial)
                {
                    myAnim.SetTrigger("ShieldMaiden_BlindingRay");                  //no need to update streak here; updated once already by fencer script
                }
            }

        }
    }

    public void HealingWord()
    {
        target = GameObject.Find("Fencer").transform;           //targeting other player slot       !!!can't use findwithtag here!!! msut use find string name
        targetSlot = ReturnTargetSlot(target);
        damage = healingWordAmount;
        if (target.GetComponentInChildren<Fencer>() != null)         //if the target we found has a fencer script
        {
            Fencer fencerScript = target.GetComponentInChildren<Fencer>();
            if (fencerScript != null)
            {
                StartCoroutine(fencerScript.IncrementHealth(healingWordAmount));
                //StartCoroutine(IncrementHealth(slot1Amount));                       //heal self too?
            }
        }
    }

    public void HealingRune()
    {
        //leaves a static rune on a slot that heals whatever target is in the slot per turn for x turns
        if (transform.parent.name == "Slot1")
        {
            targetSlot = 1;
            StartCoroutine(HealingRuneForTurns("Player1"));
        }
        else if (transform.parent.name == "Slot2")
        {
            targetSlot = 2;
            StartCoroutine(HealingRuneForTurns("Player2"));
        }
    }

    IEnumerator HealingRuneForTurns(string tag)
    {
        int turns = 25;
        int curTurns = 0;
        while (curTurns < turns)
        {
            if (notePassed)
            {
                if (tag == "Player1")
                {
                    fxManager.ShowDamage(healingRuneAmount, 1);
                }
                else if (tag == "Player2")
                {
                    fxManager.ShowDamage(healingRuneAmount, 2);
                }

                if (GameObject.FindWithTag(tag).transform.Find("Fencer") != null)
                {
                    Fencer fencer = GameObject.Find("Fencer").GetComponent<Fencer>();
                    StartCoroutine(fencer.IncrementHealth(healingRuneAmount));
                }
                else if (GameObject.FindWithTag(tag).transform.Find("ShieldMaiden") != null)
                {
                    StartCoroutine(IncrementHealth(healingRuneAmount));
                }

                curTurns++;
                notePassed = false;
            }
            yield return null;
        }
    }

    public void DivineBulwark()
    {
        if (transform.parent.CompareTag("Player1"))             //if we are player 1
        {
            target = GameObject.FindWithTag("Player2").transform;
            targetSlot = ReturnTargetSlot(target);
        } else if (transform.parent.CompareTag("Player2"))             //if we are player 2
        {
            target = GameObject.FindWithTag("Player1").transform;
            targetSlot = ReturnTargetSlot(target);
        }

        if (target.GetComponentInChildren<Fencer>() != null)
        {
            Fencer fencerScript = target.GetComponentInChildren<Fencer>();
            if (fencerScript != null)
            {
                StartCoroutine(SetInvulnerability(fencerScript));
            }
        }
    }

    public IEnumerator IncrementHealth(float amount)                  //amount needs to be negative to decrease health!!!
    {
        //PLAY RELEVANT ANIMATION HERE
        float elapsedTime = 0f;
        float incrementTime = 0.1f;
        float startValue = curHealth;
        float endValue = curHealth + amount;
        if (amount < 0)     //if the health is to decrease
        {
            if (canTakeDamage)
            {
                pController.ResetStreak();
                myHBAnim.SetTrigger("HealthBar_Flash");               //make the healthbar flash while health is decremented; for some reason it needs to be settrigger not set bool               
                while (elapsedTime < incrementTime)
                {
                    curHealth = Mathf.Lerp(startValue, endValue, elapsedTime / incrementTime);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                yield return new WaitForSeconds(0.3f);                      //wait this long before healthbar stops flashing
                myHBAnim.SetTrigger("HealthBar_StopFlash");
            }
            else if (!canTakeDamage)
            { }
        }
        else if (amount > 0)                            //can be healed even if mid ability
        {
            while (elapsedTime < incrementTime)
            {
                curHealth = Mathf.Lerp(startValue, endValue, elapsedTime / incrementTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
            
    }

    public IEnumerator SetInvulnerability(Fencer fencer)               //maybe add future parameters for variable targets
    {
        Animator fencerAnim = fencer.GetComponentInChildren<Animator>();
        //play BULWARK start animation FOR BOTH CHARACTERS HERE
        int curTurns = 0;
        int turns = 5;
        print(curTurns);
        while (curTurns < turns)
        {
            if (notePassed)
            {
                fencer.canTakeDamage = false;           //invulnerability on fencer
                canTakeDamage = false;              //invulnerability on self
                curTurns++;
                notePassed = false;        
            }
            yield return null;
        }
        //can replace this with reset()
        myAnim.SetBool(slot2Trigger, false);
        fencer.canTakeDamage = true;
        Reset();
    }

    public IEnumerator Stun()
    {
        //myAnim.SetBool("Fencer_Stunned", true);
        //myEffectsAnim.SetTrigger("Effect_Stunned");
        canUseAbilities = false;
        int turns = 10;
        int curTurns = 0;
        while (curTurns < turns)
        {
            if (notePassed)
            {
                print("stunned");
                curTurns++;
                notePassed = false;
            }
            yield return null;
        }
        //myAnim.SetBool("Fencer_Stunned", false);
        //myEffectsAnim.SetTrigger("Effect_EndStunned");
        canUseAbilities = true;
    }

    bool CheckMana(string _combination)
    {
        if (_combination == pController.divineBulwark)          //low tier abilities cost no mana
        {
            return true;
        }
        else if (_combination == pController.healingWord && pController.curMana >= pController.healingWordCost)           //can't cast on empty mana, basically
        {
            return true;
        }
        else if (_combination == pController.healingRune && pController.curMana >= pController.healingRuneCost)
        {
            return true;
        }
        else if (_combination == pController.fencerMaidenSpecial && pController.curMana >= pController.specialManaCost)
        {
            return true;
        }
        else                                    //else if we don't have enough mana...                     
        {
            print("not enough mana");
            return false;
        }

    }

    float Map(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;         //remapping stat progress to fillamount 
    }

    int ReturnTargetSlot(Transform target)
    {
        if (target.name == "Fencer")
        {
            if (target.transform.parent.name == "Slot1")
                return 1;
            if (target.transform.parent.name == "Slot2")
                return 2;
            if (target.transform.parent.name == "Slot3")
                return 3;
            else
                return 4;
        }
        else
        {
            if (target.name == "Slot1")
                return 1;
            if (target.name == "Slot2")
                return 2;
            if (target.name == "Slot3")
                return 3;
            else
                return 4;
        }
    }

    public void Disable()
    {
        canUseAbilities = false;
        canTakeDamage = false;
    }

    public void Reset()
    {
        canUseAbilities = true;
        usingAbilities = false;
        canTakeDamage = true;
    }

    public void UsingAbilities()
    {
        usingAbilities = true;
    }
}
