using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowComboAnimEvents : MonoBehaviour {

    public ShowComboScript showCombo;

    void _ResetTrigger()
    {
        showCombo.ResetTriggers();
    }
}
