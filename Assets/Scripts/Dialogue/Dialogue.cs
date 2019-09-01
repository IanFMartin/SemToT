using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[System.Serializable]

public class Dialogue : MonoBehaviour
{
    [TextArea(3, 10)]
    //que se dice en el texto
    public string[] sentences;
    //dialogos siguientes posibles
    public Dialogue[] nextDialogues;
    //que botones tienen que activarse
    public Button[] Buttons;
    public Guide guide;

    private DialogueManager _DM;
    public void NextDialogue(int WhichDialogue)
    {
        if (nextDialogues.Any())
        {
            foreach (var button in Buttons)
            {
                button.gameObject.SetActive(false);
            }
            //cambiar el dialogo actual
            guide.ChangeDialogue(nextDialogues[WhichDialogue]);
        }
    }
}

