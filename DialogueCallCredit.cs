using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueCallCredit : MonoBehaviour
{
    public void CallCredit()
    {
        DialogueManager.instance.FadeObj.SetActive(true);
    }
}
