using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    [SerializeField] private GameObject creditPanel;
    [SerializeField] private GameObject mainMenus;
    private void Start()
    {
        creditPanel.SetActive(false);
    }
    public void OnPlay()
    {
        SceneManager.LoadScene("Main");
    }

    public void OnCredit()
    {
        creditPanel.SetActive(true);
        mainMenus.SetActive(false);
    }

    public void RemoveCredit()
    {
        creditPanel.SetActive(false);
        mainMenus.SetActive(true);
    }

    public void OnQuit()
    {
        Application.Quit();
    }


}
