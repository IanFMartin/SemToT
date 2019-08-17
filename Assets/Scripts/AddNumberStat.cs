using UnityEngine;
using UnityEngine.UI;

public class AddNumberStat : MonoBehaviour
{
    public Text txtStat;
    public Text txtToAdd;
    public string statName;
    internal float statToAdd;
    public float stat;

    internal float timer;
    private float timeToAdd;

    private void Update()
    {
        if (statToAdd > 0)
        {
            timer += Time.deltaTime;
            timeToAdd -= Time.deltaTime;
            if (timer >= 2f && timeToAdd <= 0)
                AddNumber();
            txtToAdd.text = "+ " + Mathf.RoundToInt(statToAdd).ToString();
            txtStat.text = statName + Mathf.RoundToInt(stat).ToString();
        }
        else
            timer = 0;
    }

    private void AddNumber()
    {
        timeToAdd = 0.08f;
        statToAdd--;
        stat++;
    }
}
