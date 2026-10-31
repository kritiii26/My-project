using UnityEngine;

public class Ball : MonoBehaviour
{
    public float fallSpeed = 5f;
    public float speedIncreasePerCatch = 0.2f;
    public float maxSpeed = 15f;

    public float spawnY = 6f;
    public float spawnLimitX = 7f;
    public float bottomLimitY = -5.5f;

    private float currentSpeed;
    private bool isCaughtThisCycle = false;

    void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null && GetComponent<Collider2D>() == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Start()
    {
        currentSpeed = fallSpeed;
        ResetBall();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        Vector3 prevPosition = transform.position;
        transform.Translate(Vector3.down * currentSpeed * Time.deltaTime);
        Vector3 newPosition = transform.position;

        CheckCatchWithCatcher(prevPosition, newPosition);

        if (transform.position.y < bottomLimitY)
        {
            OnMiss();
        }
    }

    public void FullReset()
    {
        currentSpeed = fallSpeed;
        ResetBall();
    }

    public void ResetBall()
    {
        isCaughtThisCycle = false;
        float randomX = Random.Range(-spawnLimitX, spawnLimitX);
        transform.position = new Vector3(randomX, spawnY, 0f);
    }

    private void CheckCatchWithCatcher(Vector3 prevPos, Vector3 newPos)
    {
        if (isCaughtThisCycle) return;

        catcher player = Object.FindFirstObjectByType<catcher>();
        if (player == null) return;

        float leftX, rightX, topY, bottomY;
        Collider2D col = player.GetComponent<Collider2D>();
        Renderer ren = player.GetComponent<Renderer>();

        if (col != null)
        {
            Bounds b = col.bounds;
            leftX = b.min.x;
            rightX = b.max.x;
            topY = b.max.y;
            bottomY = b.min.y;
        }
        else if (ren != null)
        {
            Bounds b = ren.bounds;
            leftX = b.min.x;
            rightX = b.max.x;
            topY = b.max.y;
            bottomY = b.min.y;
        }
        else
        {
            Vector3 p = player.transform.position;
            Vector3 s = player.transform.localScale;
            leftX = p.x - Mathf.Abs(s.x) * 0.5f;
            rightX = p.x + Mathf.Abs(s.x) * 0.5f;
            topY = p.y + Mathf.Abs(s.y) * 0.5f;
            bottomY = p.y - Mathf.Abs(s.y) * 0.5f;
        }

        float tolerance = 0.35f;
        leftX -= tolerance;
        rightX += tolerance;

        float ballX = newPos.x;
        if (ballX >= leftX && ballX <= rightX)
        {
            float higherY = Mathf.Max(prevPos.y, newPos.y);
            float lowerY = Mathf.Min(prevPos.y, newPos.y);

            if (higherY >= (bottomY - 0.2f) && lowerY <= (topY + 0.2f))
            {
                HandleCatch(player.gameObject);
            }
        }
    }

    private void OnMiss()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterMiss();

            if (!GameManager.Instance.isGameOver)
            {
                ResetBall();
            }
        }
        else
        {
            ResetBall();
        }
    }

    private void HandleCatch(GameObject hitObject)
    {
        if (isCaughtThisCycle) return;

        if (hitObject.GetComponentInParent<catcher>() != null || hitObject.CompareTag("Player"))
        {
            isCaughtThisCycle = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore();
            }

            currentSpeed = Mathf.Min(currentSpeed + speedIncreasePerCatch, maxSpeed);
            ResetBall();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCatch(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCatch(collision.gameObject);
    }
}
