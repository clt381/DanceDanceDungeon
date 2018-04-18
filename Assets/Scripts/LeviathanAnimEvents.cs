using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeviathanAnimEvents : MonoBehaviour {

    Leviathan leviathan;
    EffectsManager fxManager;

    void Start()
    {
        leviathan = GetComponentInParent<Leviathan>();
        fxManager = GameObject.Find("Stats").GetComponent<EffectsManager>();
    }

    void _FangStrike()
    {
        leviathan.FangStrike();
    }

    void _HellFire()
    {
        leviathan.HellFire();
    }

    void _SerpentSong()
    {
        leviathan.SerpentSong();
    }

    void _Regenerate()
    {
        StartCoroutine(leviathan.IncrementHealth(leviathan.regenAmount, 0.1f));
        fxManager.ShowDamage(leviathan.regenAmount, 3);                         //hard coded showdamage for regeneration
    }

    void _BeginAttack()
    {
        leviathan.invulnerable = true;
        leviathan.countingNotes = false;        //doesn't record notes while attacking
    }

    void _EndAttack()
    {
        leviathan.invulnerable = false;
        leviathan.countingNotes = true;         //resumes counting notes after attacks are finished
    }
}
