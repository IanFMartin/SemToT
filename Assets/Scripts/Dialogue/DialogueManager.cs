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
    internal bool isCorrutineOn;
    public float SecondsToWait;
    internal bool IsTalking;

    void Start()
    {
        sentences = new Queue<string>();
        dialogueCanvas.gameObject.SetActive(false);
    }
    public void StartDialogue(Dialogue dialogue)
    {
        IsTalking = true;
        dialogueCanvas.gameObject.SetActive(true);
        sentences.Clear();
        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }

    //el que no tiene parametros de entrada es el primero que se ejecuta
    //el otro es el que usa el guia para navegar los distintos dialogos.
    public void DisplayNextSentence(Dialogue dialogue)
    {
        isCorrutineOn = false;
        sentences.Clear();
        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        sentence = sentences.Dequeue();

        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));

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
    public void EndDialogue()
    {
        IsTalking = false;
        dialogueCanvas.gameObject.SetActive(false);
    }
    IEnumerator TypeSentence(string sentence)
    {
        isCorrutineOn = true;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(SecondsToWait);
        }
        isCorrutineOn = false;
    }
    //public void StartDialogue(Dialogue dialogue)
    //{

    //    dialogueCanvas.gameObject.SetActive(true);
    //    sentences.Clear();
    //    foreach (string sentence in dialogue.sentences)
    //    {
    //        sentences.Enqueue(sentence);
    //    }
    //    DisplayNextSentence();
    //}
    //public void DisplayNextSentence()
    //{
    //    isCorrutineOn = false;
    //    if (sentences.Count == 0)
    //    {
    //        EndDialogue();
    //        return;
    //    }
    //    sentence = sentences.Dequeue();

    //    StopAllCoroutines();
    //    StartCoroutine(TypeSentence(sentence));

    //}
    //public void DisplayFullSentence()
    //{
    //    StopAllCoroutines();
    //    dialogueText.text = sentence;
    //    isCorrutineOn = false;
    //}
    //IEnumerator TypeSentence(string sentence)
    //{
    //    isCorrutineOn = true;
    //    dialogueText.text = "";
    //    foreach (char letter in sentence.ToCharArray())
    //    {
    //        dialogueText.text += letter;
    //        yield return new WaitForSeconds(SecondsToWait);
    //    }
    //    isCorrutineOn = false;
    //}
    //public void EndDialogue()
    //{
    //    dialogueCanvas.gameObject.SetActive(false);
    //}
}
