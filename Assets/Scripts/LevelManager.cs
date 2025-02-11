using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Button _playBtn;

    [SerializeField] private TMP_Text _worldLevel;
    [SerializeField] private TMP_Text _worldType;

    private int currentLevel;

    private void Start()
    {
        currentLevel = PlayerPrefs.GetInt("currentLevel", 2);
        _playBtn.onClick.AddListener(LoadLevel);
    }

    private void LoadLevel()
    {
        SceneManager.LoadScene(currentLevel);
    }
}