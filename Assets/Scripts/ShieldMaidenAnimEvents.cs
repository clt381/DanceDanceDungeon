using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldMaidenAnimEvents : MonoBehaviour {

	ShieldMaiden maidenScript;
    public EffectsManager effectsManager;
    int targetSlot;

    void Start()
    {
        maidenScript = GetComponentInParent<ShieldMaiden>();
    }

    public void _HealingWord()
    {
        maidenScript.HealingWord();
        targetSlot = maidenScript.targetSlot;
    }

    public void _DivineBulwark()
    {
        maidenScript.DivineBulwark();
        targetSlot = maidenScript.targetSlot;
    }

    public void _HealingRune()
    {
        maidenScript.HealingRune();
        targetSlot = maidenScript.targetSlot;
    }

    public void _ShowDamage()
    {
        print(maidenScript.damage);
        effectsManager.ShowDamage(maidenScript.damage, targetSlot);
    }

    public void _Disable()      //to be played at start of animations
    {
        maidenScript.Disable();
    }

    public void _UsingAbilities()
    {
        maidenScript.UsingAbilities();
    }

    public void _Reset()            //to be played at end of animations
    {
        maidenScript.Reset();
    }
}
