using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Leviathan : MonoBehaviour {

    [Header("Stats")]
    [Range(1000f, 2000f)]
    public float maxHealth = 1000f;
    float minHealth = 0f;
    public float maxArmor = 300f;
    float minArmor = 0f;
    public float curHealth;
    float mappedHealth;                 //mapped variable to fit fillamount of UI image
    public float curArmor;
    public float regenAmount = 50f;
    float damage = 120f;     //attack damage

    [Header("Images and Animators")]
    public Image myHealthBar;           //UI healthbar component
    public Animator myHBAnim;               //animator atached to healthbar object (attached to heatlhbar parent)

    public Image[] myArmorBars;             //UI armorbars component; contains an array of armor sprites arranged in a vertical row in front of body sprite
    public Animator myABAnim;                   //armor bars should flash while being decremented

    [HideInInspector]
    public int nextAttack;      //changed per attack; referenced by uicontroller

    Transform myTarget;

    [Header("Abilities")]
    public string fangStrike = "FangStrike";        //referenced by uicontroller to determine attacktype
    public int fangStrikeCD;
    public string hellFire = "HellFire";
    public int hellFireCD;
    public string serpentSong = "SerpentSong";
    public int serpentSongCD;
    public string regenerate = "Regenerate";
    public int regenerateCD;

    [HideInInspector]
    public string nextAttackString = "Empty";                      //to determined by random value (REMEMBER: strings must be initialized with some characters, or they stay blank?)
    [HideInInspector]
    public int notesToNextAttack;
    [HideInInspector]
    public bool invulnerable = false;               //change this in 'reset' function via animevents script during animation attacks
    [HideInInspector]
    public bool countingNotes = true;                   //determine if it can track passing notes
    [HideInInspector]
    public bool regenerating;
    public bool dead = false;
    int timesRegenerated;                               //to prevent leviathan from regenerating several times

    Animator myAnim;
    UIController uiController;

    [HideInInspector]
    public int notesPassed;            //to keep track of all notes passed since start of game; should be iterated by beatwindow trigger (on enter or on exit)
    [HideInInspector]
    public bool notePassed = false;                 //separate from notespassed; for tracking turn based abilities (can probably change bool into int)

    void Start()
    {
        myAnim = GetComponentInChildren<Animator>();            //should grab from the 'graphics' child object
        uiController = GameObject.Find("Stats").GetComponent<UIController>();       //grabs from canvas 'stats' object
        curHealth = maxHealth;              //starts at full health
        DetermineNextAttack();                      //determines first attack on start
        StartCoroutine(StartAttack(notesToNextAttack, nextAttackString));
    }

    void Update()
    {
        UpdateHealth();
        UpdateArmor();
    }

    void UpdateHealth()
    {
        curHealth = Mathf.Clamp(curHealth, minHealth, maxHealth);       //keeping health between min and max
        mappedHealth = Map(curHealth, minHealth, maxHealth, 0, 1);      //mapping health to fillamount
        myHealthBar.fillAmount = mappedHealth;

        if (curHealth <= minHealth && !dead)
        {
            StartCoroutine(Die());
        }
    }

    void UpdateArmor()
    {
        curArmor = Mathf.Clamp(curArmor, minArmor, maxArmor);
        if (curArmor < maxArmor && curArmor > 2 * (maxArmor/3))
        {
            myArmorBars[0].enabled = false;
        }
        else if (curArmor < 2 * (maxArmor/3) && curArmor > 1 * (maxArmor / 3))
        {
            myArmorBars[0].enabled = false;
            myArmorBars[1].enabled = false;
        }
        else if (curArmor < 1 * (maxArmor/3))
        {
            myArmorBars[0].enabled = false;
            myArmorBars[1].enabled = false;
            myArmorBars[2].enabled = false;
        }

    }

    void DetermineNextAttack()
    {
        float value = Random.Range(1, 100);
        if (curHealth <= maxHealth / 2 && !regenerating)           //if we're at half health or less and our last attack wasn't regeneration
        {
            if (timesRegenerated < 1)                  //put an int check here that checks if we've already used regeneration before
            {
                nextAttackString = regenerate;
                notesToNextAttack = regenerateCD;
                timesRegenerated++;
            }
            else                                //if we've already regenerated multiple times, return to determining an attack
            {
                value = Random.Range(1, 100);         //some abilities become available after half health?
                if (value < 25)
                {
                    nextAttackString = fangStrike;
                    notesToNextAttack = fangStrikeCD;
                }
                else if (value > 25 && value < 60)
                {
                    nextAttackString = hellFire;
                    notesToNextAttack = hellFireCD;
                }
                else if (value > 60 && value < 100)
                {
                    nextAttackString = serpentSong;
                    notesToNextAttack = serpentSongCD;
                }
            }
        }
        else
        {
            if (value < 60)
            {
                nextAttackString = fangStrike;
                notesToNextAttack = fangStrikeCD;
            }
            else if (value > 60 && value < 90)
            {
                nextAttackString = hellFire;
                notesToNextAttack = hellFireCD;
            }
            else if (value > 90 && value < 100)
            {
                nextAttackString = serpentSong;
                notesToNextAttack = serpentSongCD;
                //print("next attack is serpent song");
            }
        }           

        uiController.countDown = notesToNextAttack;     //essentially restarting the countdown after determining attack
        uiController.countDownThreshold = notesToNextAttack - 10;        //threshold will always be less than notes to next attack
    }

    public IEnumerator StartAttack(int _notesToNextAttack, string attackType)      //attack type should be 'nextattack' string which is public
    {
        nextAttack = notesPassed + _notesToNextAttack;                  //notes passed is incremented by ontriggerexit of notes
        while (notesPassed <= nextAttack)                           //<= allows uicontroller countdown to reach zero and play corresponding animations
        {
            yield return null;
        }

        if (attackType == fangStrike)
        {
            myAnim.SetTrigger("Leviathan_Attack_FangStrike");           //use animator helper script to determine which one it attacks
        }
        else if (attackType == hellFire)
        {
            myAnim.SetTrigger("Leviathan_Attack_HellFire");
        }
        else if (attackType == serpentSong)
        {
            myAnim.SetTrigger("Leviathan_Attack_SerpentSong");
        }
        else if (attackType == regenerate)
        {
            StartCoroutine(Regenerate(40));             //regenerates for this many notes unless interrupted (getting back more than half its health as rule of thumb)
        }

        DetermineNextAttack();                                      //changes turns till next attack and the next attack type
        StartCoroutine(StartAttack(notesToNextAttack, nextAttackString));
    }

    public void FangStrike()                //to be called as event by child animator component
    {
        myTarget = GameObject.FindWithTag("Player2").transform;          //attack targets the front slot
        if (myTarget.transform.Find("Fencer") != null)          //if the target in slot2 is fencer
        {
            Fencer fencer = myTarget.GetComponentInChildren<Fencer>();
            StartCoroutine(fencer.IncrementHealth(-damage));
        }
        else if (myTarget.transform.Find("ShieldMaiden") != null)
        {
            ShieldMaiden maiden = myTarget.GetComponentInChildren<ShieldMaiden>();
            StartCoroutine(maiden.IncrementHealth(-damage));
        }
    }

    public void HellFire()
    {
        myTarget = GameObject.FindWithTag("Player2").transform;          //ATTACKS BOTH
        if (myTarget.transform.Find("Fencer") != null)          //if the target in slot2 is fencer
        {
            Fencer fencer = myTarget.GetComponentInChildren<Fencer>();
            StartCoroutine(fencer.IncrementHealth(-damage));
        }
        else if (myTarget.transform.Find("ShieldMaiden") != null)
        {
            ShieldMaiden maiden = myTarget.GetComponentInChildren<ShieldMaiden>();
            StartCoroutine(maiden.IncrementHealth(-damage));
        }

        myTarget = GameObject.FindWithTag("Player1").transform;      //changes transform target to attack back slot
        if (myTarget.transform.Find("Fencer") != null)          //if the target in slot2 is fencer
        {
            Fencer fencer = myTarget.GetComponentInChildren<Fencer>();
            StartCoroutine(fencer.IncrementHealth(-damage));
        }
        else if (myTarget.transform.Find("ShieldMaiden") != null)
        {
            ShieldMaiden maiden = myTarget.GetComponentInChildren<ShieldMaiden>();
            StartCoroutine(maiden.IncrementHealth(-damage));
        }
    }

    public void SerpentSong()
    {
        myTarget = GameObject.FindWithTag("Player2").transform;
        if (myTarget.transform.Find("Fencer") != null)          //if the target in slot2 is fencer
        {
            Fencer fencer = myTarget.GetComponentInChildren<Fencer>();
            StartCoroutine(fencer.Stun());
        }
        else if (myTarget.transform.Find("ShieldMaiden") != null)
        {
            ShieldMaiden maiden = myTarget.GetComponentInChildren<ShieldMaiden>();
            StartCoroutine(maiden.Stun());
        }

        myTarget = GameObject.FindWithTag("Player1").transform;
        if (myTarget.transform.Find("Fencer") != null)          //if the target in slot2 is fencer
        {
            Fencer fencer = myTarget.GetComponentInChildren<Fencer>();
            StartCoroutine(fencer.Stun());
        }
        else if (myTarget.transform.Find("ShieldMaiden") != null)
        {
            ShieldMaiden maiden = myTarget.GetComponentInChildren<ShieldMaiden>();
            StartCoroutine(maiden.Stun());
        }
    }

    public IEnumerator Regenerate(int numOfNotes)               //leviathan should be counting notes here!
    {
        regenerating = true;
        for (int i = 0; i < myArmorBars.Length; i++)
        {
            myArmorBars[i].enabled = true;                  //enable the sprites of all armor bars
        }
        //play armor bars spawned animation
        curArmor = maxArmor;
        int counter = notesPassed + numOfNotes;
        countingNotes = true;                                   //just in case
        myAnim.SetTrigger("Leviathan_StartRegen");
        while (notesPassed < counter && curArmor > 0)
        {
            yield return null;
        }
        //disable armor element in uicontroller
        for (int i = 0; i < myArmorBars.Length; i++)
        {
            myArmorBars[i].enabled = false;                  //disable the sprites of all armor bars (probably already disabled)
        }
        curArmor = minArmor;
        myAnim.SetTrigger("Leviathan_StopRegen");
        myHBAnim.SetTrigger("HealthBar_StopFlash");
        regenerating = false;
    }

    public void IncrementArmor(float amount)           //call this in increment health if curarmor is greater than 0
    {
        curArmor += amount;
    }

    public IEnumerator Stun()
    {
        //myAnim.SetBool("Leviathan_Stunned", true);
        //myEffectsAnim.SetTrigger("Effect_Stunned");
        if (!regenerating)                              //leviathan can't be stunned while regenerating
        {
            countingNotes = false;
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
            //myAnim.SetBool("Leviathan_Stunned", false);
            //myEffectsAnim.SetTrigger("Effect_EndStunned");
            countingNotes = true;
        }
    }

    public IEnumerator IncrementHealth(float amount, float time)                  //amount needs to be negative to decrease health!!!
    {
        float elapsedTime = 0f;
        float incrementTime = time;
        float startValue = curHealth;
        float endValue = curHealth + amount;
        if (!regenerating)          //if the leviathan isn't regenerating
        {
            if (amount < 0)     //if the health is to decrease
            {
                myHBAnim.SetTrigger("HealthBar_Flash");               //make the healthbar flash while health is decremented; for some reason it needs to be settrigger not set bool
            }
            while (elapsedTime < incrementTime)
            {
                curHealth = Mathf.Lerp(startValue, endValue, elapsedTime / incrementTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            curHealth = endValue;                                   //snap to final position as insurance
            yield return new WaitForSeconds(0.3f);                      //wait this long before healthbar stops flashing
            myHBAnim.SetTrigger("HealthBar_StopFlash");
        }
        else if (regenerating)                  //will be set to false once armor is reduced to zero
        {
            if (amount < 0)         //armor can only be decremented
            {
                IncrementArmor(amount);
            }
            else if (amount > 0)            //if this is healing from self
            {
                myHBAnim.SetTrigger("HealthBar_FlashGreen");
                while (elapsedTime < incrementTime)
                {
                    curHealth = Mathf.Lerp(startValue, endValue, elapsedTime / incrementTime);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                curHealth = endValue;
            }            
        }      
    }

    IEnumerator Die()               //to be called when leviathan enters less than 0 health
    {
        dead = true;                //for edge condition
        countingNotes = false;
        myAnim.SetTrigger("Leviathan_Dead");
        yield return new WaitForSeconds(5f);
        print("Leviathan has died!");
        //add victory menu code here!   
    }

    float Map(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;         //remapping stat progress to fillamount 
    }
}
