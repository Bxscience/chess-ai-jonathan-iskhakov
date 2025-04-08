using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI pgnText;
    public Button copyPGNButton;
    public Button mainMenuButton;

    private void Start()
    {
        string result = PlayerPrefs.GetString("ResultText");
        string pgn = PlayerPrefs.GetString("PGN");

        resultText.text = result;
        // pgnText.text = pgn;

        copyPGNButton.onClick.AddListener(() => GUIUtility.systemCopyBuffer = pgn);
        mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
    }
}