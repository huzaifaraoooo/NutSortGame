using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WinPanel : MonoBehaviour
{
    public static WinPanel instance;

    public Button nextLevelButton;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // WinPanel ko start par chhupa do
       // gameObject.SetActive(false);

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevel);
    }

    public void ShowWinPanel()
    {
        gameObject.SetActive(true);

        // Animation (Pop-up Effect)
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }

    public void OnNextLevel()
    {
        // Scene change se pehle DOTween animations ko rok do
        transform.DOKill();

        transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
              //  gameObject.SetActive(false);

                int nextLevel = PlayerPrefs.GetInt("CurrentLevel", 1) + 1;
                PlayerPrefs.SetInt("CurrentLevel", nextLevel);
                PlayerPrefs.Save();

                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            });
    }
}