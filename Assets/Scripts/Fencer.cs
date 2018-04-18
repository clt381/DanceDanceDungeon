using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fencer : MonoBehaviour {

    PlayerController pController;
    AudioController aController;
    //stats
    [Range(200f,500f)]
    public float maxHealth = 300f;
    public float minHealth = 0f;
    [HideInInspector]
    public float curHealth;
    float mappedHealth;                 //mapped variable to fit fillamount of UI image

    public Image myHealthBar;           //UI healthbar component
    public Animator myHBAnim;               //animator atached to healthbar object (attached to heatlhbar parent)
    public Animator myEffectsAnim;          //animator that manages effects
    Animator myAnim;

    Transform target;            //referenced by animator helper script; changes with each ability
    Transform mySlot;           //this determines whether to execute one ability or another; will change with the 'move' ability
    [HideInInspector]
    public int targetSlot;
    [HideInInspector]
    public float damage;

    [HideInInspector]
    string slot1Trigger = "Fencer_HolyLance";
    public float holyLanceDamage = -50f;
    string slot2Trigger = "Fencer_SkewerAndSlash";
    public float skewerAndSlashDamage = -75f;         //direct damage values need to be negative!
    public float blindingRayDamage = -200f;             //this is an ultimate damage attack
    public float riposteDamage = -100f;

    //bools to be controlled via animation events
    public bool canTakeDamage = true;             //whether player can be damaged
    public bool canUseAbilities = true;
    public bool usingAbilities = false;

    [HideInInspector]
    public bool notePassed = false;             //for notebased abilities                    
    [HideInInspector]
    public bool hitOnCounter = false;           //for counterattack abilities (bool that triggers when fencer is damaged)
    [HideInInspector]
    public bool countering = false;

    void Start()
    {
        myAnim = GetComponentInChildren<Animator>();        //getting animator from graphics child object
        pController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        aController = GameObject.Find("AudioController").GetComponent<AudioController>();
        curHealth = maxHealth;          //start with full health
        myHealthBar.fillAmount = 1;
    }

    void Update()
    {
        mappedHealth = Map(curHealth, minHealth, maxHealth, 0, 1);
        myHealthBar.fillAmount = mappedHealth;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(StartRiposte());
        }
    }

    public void UseAbility(string _combination)
    {
        if (canUseAbilities)                //to prevent potential multi-ability stacking
        {
            if (CheckMana(_combination) != false)
            {
                if (_combination == pController.holyLance)
                {
                    myAnim.SetTrigger("Fencer_HolyLance");
                    pController.UpdateStreak(1);
                }
                else if (_combination == pController.skewerAndSlash)
                {
                    myAnim.SetTrigger("Fencer_SkewerAndSlash");
                    StartCoroutine(pController.IncrementMana(-pController.skewerAndSlashCost));
                    pController.UpdateStreak(1);
                }
                else if (_combination == pController.riposte)
                {
                    StartCoroutine(StartRiposte());
                    StartCoroutine(pController.IncrementMana(-pController.riposteCost));
                    pController.UpdateStreak(1);
                }
                else if (_combination == pController.fencerMaidenSpecial)
                {
                    myAnim.SetTrigger("Fencer_BlindingRay");
                    StartCoroutine(pController.IncrementMana(-pController.specialManaCost));
                    pController.UpdateStreak(1);
                }
            }
        }
    }

    public void SkewerAndSlash()
    {
        target = GameObject.Find("Leviathan").transform;
        damage = skewerAndSlashDamage * pController.dmgMultiplier;
        targetSlot = ReturnTargetSlot(target);
        if (target.GetComponentInChildren<Leviathan>() != null)         //if the target we found in slot 3 is leviathan
        {
            Leviathan LeviathanScript = target.GetComponent<Leviathan>();
            if (LeviathanScript != null)
            {
                StartCoroutine(LeviathanScript.IncrementHealth(damage, 0.1f));
            }
        }
    }

    public void HolyLance()
    {
        target = GameObject.Find("Leviathan").transform;
        damage = holyLanceDamage * pController.dmgMultiplier;
        targetSlot = ReturnTargetSlot(target);
        if (target.GetComponent<Leviathan>() != null)         //if the target we found in slot 3 is leviathan
        {
            Leviathan LeviathanScript = target.GetComponentInChildren<Leviathan>();
            if (LeviathanScript != null)
            {
                StartCoroutine(LeviathanScript.IncrementHealth(damage, 0.1f));
            }
        }
    }

    public IEnumerator StartRiposte()
    {
        int turns = 5;
        int curTurn = 0;
        canTakeDamage = false;                      //will be set false by disable anim event, but just in case
        countering = true;
        myAnim.SetBool("Fencer_StartRiposte", true);
        while (curTurn < turns)
        {
            if (notePassed)
            {
                curTurn++;
                notePassed = false;
            }

            if (hitOnCounter)             //hit can occur between notepassed; should be a separate statement
            {
                print("hit while countering!");
                myAnim.SetTrigger("Fencer_Riposte");                //animation event will handle actual damage dealing
                countering = false;
                hitOnCounter = false;
                canTakeDamage = true;                
            }
            yield return null;
        }
        canTakeDamage = true;
        canUseAbilities = true;
        countering = false;
        myAnim.SetBool("Fencer_StartRiposte", false);
    }

    public void Riposte()
    {
        target = GameObject.Find("Leviathan").transform;
        damage = riposteDamage * pController.dmgMultiplier;
        targetSlot = ReturnTargetSlot(target);
        if (target.GetComponent<Leviathan>() != null)
        {
            Leviathan LeviathanScript = target.GetComponentInChildren<Leviathan>();
            StartCoroutine(LeviathanScript.IncrementHealth(damage, 0.1f));
        }
    }

    public void BlindingRay()                       //this is called from fencer animation which is in turn triggered by beatwindowtrigger referencing playercontroller ability string
    {
        target = GameObject.Find("Leviathan").transform;
        damage = blindingRayDamage * pController.dmgMultiplier;
        targetSlot = ReturnTargetSlot(target);
        if (target.GetComponent<Leviathan>() != null)
        {
            Leviathan LeviathanScript = target.GetComponentInChildren<Leviathan>();
            StartCoroutine(LeviathanScript.IncrementHealth(damage, 0.1f));
            StartCoroutine(LeviathanScript.Stun());
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
            else if (!canTakeDamage && countering)                      //if countering (which also sets invulnerability
            {
                hitOnCounter = true;                //sets edge condition in 'start riposte' coroutine
            }
            else if (!canTakeDamage && !countering)                 //if invulnerable and not countering, play blocked animation
            {

            }
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

    int ReturnTargetSlot(Transform target)
    {
        if (target.name == "Leviathan")
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

    bool CheckMana(string _combination)
    {
        if (_combination == pController.holyLance)          //low tier abilities cost no mana
        {
            return true;
        }
        else if (_combination == pController.skewerAndSlash && pController.curMana >= pController.skewerAndSlashCost)           //can't cast on empty mana, basically
        {
            return true;
        }
        else if (_combination == pController.riposte && pController.curMana >= pController.riposteCost)
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
