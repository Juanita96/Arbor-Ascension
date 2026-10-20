using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("PlayMenu");
    }

    public void Gacha()
    {
        SceneManager.LoadScene("Gacha");
    }

    public void Inventory()
    {
        SceneManager.LoadScene("Inventory");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Settings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void ArrangeTeam()
    {
        SceneManager.LoadScene("ArrangeTeam");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Battle()
    {
        // if there ain't a selected team then switch to the selectio nscene
        if (PlayerInventory.Instance.AttackTeam.Count == 0)
        {
            Debug.LogWarning("[SceneChanger] No hay equipo de ataque, abriendo ArrangeTeam");
            SceneManager.LoadScene(GameConfig.Instance.TeamScene);
            return;
        }

        SceneManager.LoadScene(GameConfig.Instance.CombatScene);
    }
}
