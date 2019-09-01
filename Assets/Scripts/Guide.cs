using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Guide : MonoBehaviour
{

    public Dialogue dialogue;
    private Dialogue firstDialogue;
    private DialogueManager _DM;
    //para reiniciar los botones
    public Button[] allButtons;


    public void Start()
    {
        firstDialogue = dialogue;
        _DM = FindObjectOfType<DialogueManager>();
    }

    public void Talking()
    {
        _DM.StartDialogue(firstDialogue);
        foreach (var button in allButtons)
        {
            button.gameObject.SetActive(false);
        }
        if (firstDialogue.Buttons.Any())
        {
            foreach (var button in firstDialogue.Buttons)
            {
                button.gameObject.SetActive(true);
            }
        }
    }
    //para cambiar los dialogos.
    public void ChangeDialogue(Dialogue newDialogue)
    {
        dialogue = newDialogue;
        if (dialogue.Buttons.Any())
        {
            foreach (var button in dialogue.Buttons)
            {
                button.gameObject.SetActive(true);
            }
        }
            _DM.DisplayNextSentence(dialogue);

    }
    #region sistema de dialogo viejo 
    //public Text text;
    //public Image textBackground;
    //public GameObject portal;
    //private string _textToSay;
    //private string _currentText;
    //private string[] _Alltext = new string[7];
    //bool isTalking;
    //void Start()
    //{
    //    _Alltext[0] = "Oh! Alguien nuevo.\n";
    //    _Alltext[1] = "Veo que te atrapó también la Torre. ¿Qué *qué* es la Torre? Todo es la Torre del tiempo. La dimensión de la Torre se maneja distinto de la que venís y estas acostumbrado.\n";
    //    _Alltext[2] = "Imagina que la Torre tiene vida y conciencia propia. Con su habilidad puede controlar todo lo que esta dentro y fuera de ella. Incluso nosotros.\n";
    //    _Alltext[3] = "No podes confiar en tus sentidos. La torre adora alterarlos y confundirlos. ¿Ahora mismo parece que estamos en el espacio no? Hace un siglo estaba en el fondo del océano y antes en una especie de jungla. ¿Quien sabe que vendrá después?\n";
    //    _Alltext[4] = "Aún recuerdo cuando era nuevo como tu. Quería escapar a toda costa. Tengo malas noticias, eso es imposible. No hay salida física y La muerte no es un escape. Aunque si por alguna razon logras pasar a traves de los portales y derrotar a la torre...pero eso es imposible";
    //    _Alltext[5] = "Todos estamos malditos, se podría decir. Dentro de la Torre nos debilitamos segundo a segundo, y cuando no podemos más, volvemos a este lugar. Esto somos ahora...\n";
    //    _Alltext[6] = "Bienvenido a la Torre.";

    //}
    //private void Update()
    //{
    //if(Input.GetKeyDown(KeyCode.E) && isTalking)
    //{
    //    StopAllCoroutines();
    //    //portal.gameObject.SetActive(true);
    //    textBackground.gameObject.SetActive(false);
    //    isTalking = false;
    //}
    //}

    //public void Talking()
    //{
    //    textBackground.gameObject.SetActive(true);
    //    StartCoroutine(CharactherByCharacter());
    //}

    //IEnumerator CharactherByCharacter()
    //{
    //    isTalking = true;
    //    foreach (var eachText in _Alltext)
    //    {
    //        for (int j = 0; j < eachText.Length; j++)
    //        {
    //            text.text = string.Concat(text.text, eachText[j]);
    //            yield return new WaitForSeconds(0.04f);
    //        }
    //        yield return new WaitForSeconds(1);
    //        text.text = "";
    //    }
    //    portal.gameObject.SetActive(true);
    //    textBackground.gameObject.SetActive(false);
    //    isTalking = false;
    //}
    #endregion
}
