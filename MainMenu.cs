using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public TMP_Dropdown difficultyDropdown;
    public TMP_Dropdown sideDropdown;
    public Button playAgainstAIButton;
    public Button playAgainstFriendButton;

    private void Start()
    {
        playAgainstAIButton.onClick.AddListener(StartGame);
        playAgainstFriendButton.onClick.AddListener(StartHumanGame);
    }

    private void StartGame()
    {
        int depth = difficultyDropdown.value + 1;
        string side = sideDropdown.options[sideDropdown.value].text.ToLower();

        PlayerPrefs.SetInt("AIDepth", depth);
        PlayerPrefs.SetString("UserSide", side);

        SceneManager.LoadScene("HumanAndAI");
    }

    private void StartHumanGame()
    {
        PlayerPrefs.SetString("UserSide", "white");
        SceneManager.LoadScene("HumanAndHuman");
    }

    public void Exit()
    {
        Application.Quit();
    }
}