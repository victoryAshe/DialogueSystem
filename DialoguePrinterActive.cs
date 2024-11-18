using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialoguePrinterActive : MonoBehaviour
{
    public GameObject Printer;

    public void SetPrinterActive()
    {
        DialogueManager dialogueManager = DialogueManager.instance;
        dialogueManager.Show(dialogueManager.Story);

    }
}
