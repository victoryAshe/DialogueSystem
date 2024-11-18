using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueFadeEffect : MonoBehaviour
{
    int eventKey = 0;

    public GameObject[] targetObjects;
    public GameObject[] offObjects;
    public GameObject CreditObj;

    public void PlayCredit()
    {
        if (eventKey >= targetObjects.Length)
        {
            CreditObj.SetActive(true);
            gameObject.GetComponent<Animator>().enabled = false;
        }
        else
        {
            offObjects[eventKey].SetActive(false);
            targetObjects[eventKey].SetActive(true);
        }
    }

    public void PlayAnimation()
    {
        targetObjects[eventKey++].GetComponent<Animator>().enabled = true;
        gameObject.SetActive(false);
    }
}
