using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerAgent : MonoBehaviour
{
    public Text scoreText;
    public Text rewardText;
    public Text livesText;
    public Image fadeImage;

    private BladeAgent blade;
    private Spawner spawner;

    private int score;
    private int lives;

    private void Awake()
    {
        blade = FindObjectOfType<BladeAgent>();
        spawner = FindObjectOfType<Spawner>();
    }

    private void Start()
    {
        NewGame();
    }

    private void NewGame()
    {
        Time.timeScale = 1f;

        ClearScene();
        spawner.enabled = true;

        score = 0;
        SetLives(3);
    }

    private void ClearScene()
    {
        FruitAgent[] fruits = FindObjectsOfType<FruitAgent>();

        foreach (FruitAgent fruit in fruits) {
            Destroy(fruit.gameObject);
        }

        BombAgent[] bombs = FindObjectsOfType<BombAgent>();

        foreach (BombAgent bomb in bombs) {
            Destroy(bomb.gameObject);
        }
    }

    public void IncreaseScore(int points)
    {
        score += points;
    }

    public void Explode()
    {
        spawner.enabled = false;

        StartCoroutine(ExplodeSequence());
    }

    private IEnumerator ExplodeSequence()
    {
        float elapsed = 0f;
        float duration = 0.5f;

        // Fade to white
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            fadeImage.color = Color.Lerp(Color.clear, Color.white, t);

            Time.timeScale = 1f - t;
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);

        NewGame();

        elapsed = 0f;

        // Fade back in
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            fadeImage.color = Color.Lerp(Color.white, Color.clear, t);

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }
    }

    public void DecreaseLive()
    {
        SetLives(lives - 1);
    }

    private void SetLives(int lives)
    {
        if (lives <= 0)
        {
            Explode();
            blade.AddReward(-1f);
            blade.EndEpisode();
        }

        this.lives = lives;
    }

    private void Update()
    {
        rewardText.text = "Reward: " + blade.GetCumulativeReward().ToString("0.00");
        scoreText.text = "Score: " + score.ToString();
        livesText.text = "Lives: " + lives.ToString();
    }
}
