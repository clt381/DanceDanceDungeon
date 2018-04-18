using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FencerAnimEvents : MonoBehaviour {

    Fencer fencerScript;
    public EffectsManager effectsManager;
    int targetSlot;

    void Start()
    {
        fencerScript = GetComponentInParent<Fencer>();
    }

    public void _SkewerAndSlash()
    {
        fencerScript.SkewerAndSlash();
        targetSlot = fencerScript.targetSlot;
    }

    public void _HolyLance()
    {
        fencerScript.HolyLance();
        targetSlot = fencerScript.targetSlot;           //this gets the int target slot 1-3 which in turn gets slot number from target variable
    }

    public void _Riposte()
    {
        fencerScript.Riposte();
        targetSlot = fencerScript.targetSlot;
    }

    public void _BlindingRay()
    {
        fencerScript.BlindingRay();
        targetSlot = fencerScript.targetSlot;
    }

    public void _UsingAbilities()
    {
        fencerScript.UsingAbilities();
    }

    public void _ShowDamage()
    {
        effectsManager.ShowDamage(fencerScript.damage, targetSlot);
    }

    public void _Disable()      //to be played at start of animations
    {
        fencerScript.Disable();
    }

    public void _Reset()            //to be played at end of animations
    {
        fencerScript.Reset();
    }
}
