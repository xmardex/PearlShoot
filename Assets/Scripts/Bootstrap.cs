using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private Slider _loadingBar;
    [SerializeField] private int _menuSceneId;
    private Coroutine _loadSceneIE;

    private void Start()
    {
        _loadSceneIE = StartCoroutine(LoadSceneIE());
    }

    private IEnumerator LoadSceneIE()
    {
        AsyncOperation loadSceneOp = SceneManager.LoadSceneAsync(_menuSceneId);
        loadSceneOp.allowSceneActivation = false;

        while (loadSceneOp.progress < 0.9f)
        {
            _loadingBar.value = loadSceneOp.progress;
            yield return null;
        }
        _loadingBar.value = 1f;
        yield return new WaitForSeconds(0.5f);
        loadSceneOp.allowSceneActivation = true;
    }
}
