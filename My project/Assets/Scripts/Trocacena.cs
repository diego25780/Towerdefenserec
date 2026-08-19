using UnityEngine;
using UnityEngine.SceneManagement;

public class TrocarCena : MonoBehaviour
{
    public void MudarCena(int i)
    {
        SceneManager.LoadScene(i);
    }
}
