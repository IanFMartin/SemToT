using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public int SceneToGo;
    
    public void LoadLevel()
    {
        SceneManager.LoadScene(SceneToGo);
    }
}
