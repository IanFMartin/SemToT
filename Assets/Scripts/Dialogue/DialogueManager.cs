using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    private Queue<string> sentences;
    private string sentence;
    public Text dialogueText;
    public Image dialogueCanvas;
    public bool isCorrutineOn;

    void Start()
    {
        sentences = new Queue<string>();
        dialogueCanvas.gameObject.SetActive(false);
    }

    public void StartDialogue(Dialogue dialogue)
    {

        dialogueCanvas.gameObject.SetActive(true);
        sentences.Clear();
        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }
    public void DisplayNextSentence()
    {
        isCorrutineOn = false;
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        sentence = sentences.Dequeue();

        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));

    }
    public void DisplayFullSentence()
    {
        StopAllCoroutines();
        dialogueText.text = sentence;
        isCorrutineOn = false;
    }
    IEnumerator TypeSentence(string sentence)
    {
        isCorrutineOn = true;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
        isCorrutineOn = false;
    }
    public void EndDialogue()
    {
        dialogueCanvas.gameObject.SetActive(false);
    }
}
