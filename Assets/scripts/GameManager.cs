using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindFirstObjectByType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    public int maxMisses = 3;
    public int score = 0;
    public int misses = 0;
    public bool isGameOver = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Time.timeScale = 1f;
        EnsureBallExists();
    }

    void Update()
    {
        if (isGameOver)
        {
            bool restartPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                restartPressed = true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.R))
            {
                restartPressed = true;
            }
#endif

            if (restartPressed)
            {
                RestartGame();
            }
        }
    }

    public void AddScore(int amount = 1)
    {
        if (isGameOver) return;
        score += amount;
    }

    public void RegisterMiss()
    {
        if (isGameOver) return;
        misses++;

        if (misses >= maxMisses)
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        score = 0;
        misses = 0;
        isGameOver = false;

        Ball[] balls = Object.FindObjectsByType<Ball>(FindObjectsSortMode.None);
        if (balls != null && balls.Length > 0)
        {
            foreach (var b in balls)
            {
                b.FullReset();
            }
        }
        else
        {
            SpawnDefaultBall();
        }

        catcher player = Object.FindFirstObjectByType<catcher>();
        if (player != null)
        {
            player.ResetPosition();
        }
    }

    public void EnsureBallExists()
    {
        Ball existingBall = Object.FindFirstObjectByType<Ball>();
        if (existingBall == null)
        {
            SpawnDefaultBall();
        }
    }

    public void SpawnDefaultBall()
    {
        GameObject ballObj = new GameObject("Ball");
        SpriteRenderer sr = ballObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(64);
        sr.color = new Color(1f, 0.85f, 0.1f);
        ballObj.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        ballObj.AddComponent<Ball>();
    }

    private Sprite CreateCircleSprite(int resolution)
    {
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[resolution * resolution];
        float center = (resolution - 1) / 2f;
        float radius = center - 1f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radius)
                {
                    float alpha = Mathf.Clamp01((radius - dist) + 0.5f);
                    pixels[y * resolution + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    pixels[y * resolution + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
    }

    void OnGUI()
    {
        GUIStyle hudStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold
        };
        hudStyle.normal.textColor = Color.white;

        GUIStyle shadowStyle = new GUIStyle(hudStyle);
        shadowStyle.normal.textColor = Color.black;

        int livesLeft = Mathf.Max(0, maxMisses - misses);

        GUI.Label(new Rect(22, 22, 300, 35), $"Score: {score}", shadowStyle);
        GUI.Label(new Rect(22, 57, 300, 35), $"Lives: {livesLeft} / {maxMisses}", shadowStyle);

        GUI.Label(new Rect(20, 20, 300, 35), $"Score: {score}", hudStyle);
        GUI.Label(new Rect(20, 55, 300, 35), $"Lives: {livesLeft} / {maxMisses}", hudStyle);

        if (isGameOver)
        {
            float boxWidth = 360f;
            float boxHeight = 220f;
            float boxX = (Screen.width - boxWidth) / 2f;
            float boxY = (Screen.height - boxHeight) / 2f;

            GUI.Box(new Rect(boxX, boxY, boxWidth, boxHeight), "");

            GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            headerStyle.normal.textColor = new Color(1f, 0.25f, 0.25f);
            GUI.Label(new Rect(boxX, boxY + 20, boxWidth, 40), "GAME OVER", headerStyle);

            GUIStyle subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };
            subStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(boxX, boxY + 70, boxWidth, 30), $"Final Score: {score}", subStyle);
            GUI.Label(new Rect(boxX, boxY + 105, boxWidth, 30), "Press 'R' or Click Below", subStyle);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            if (GUI.Button(new Rect(boxX + 90, boxY + 150, 180, 45), "Play Again", btnStyle))
            {
                RestartGame();
            }
        }
    }
}
