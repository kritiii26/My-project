using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class catcher : MonoBehaviour
{
    public float speed = 10f;
    public float limit = 8f;

    void Awake()
    {
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            if (GetComponent<SpriteRenderer>() == null && col.size == Vector2.zero)
            {
                col.size = new Vector2(2f, 0.5f);
            }
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        float input = GetHorizontalInput();
        transform.Translate(speed * input * Vector3.right * Time.deltaTime);
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -limit, limit);
        transform.position = pos;
    }

    private float GetHorizontalInput()
    {
        float input = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
            {
                input -= 1f;
            }
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)
            {
                input += 1f;
            }
        }

        if (Gamepad.current != null)
        {
            float stickX = Gamepad.current.leftStick.x.ReadValue();
            if (Mathf.Abs(stickX) > 0.1f)
            {
                input = stickX;
            }
            else if (Gamepad.current.dpad.left.isPressed)
            {
                input -= 1f;
            }
            else if (Gamepad.current.dpad.right.isPressed)
            {
                input += 1f;
            }
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Mathf.Approximately(input, 0f))
        {
            input = Input.GetAxisRaw("Horizontal");
        }
#endif

        return input;
    }

    public void ResetPosition()
    {
        Vector3 pos = transform.position;
        pos.x = 0f;
        transform.position = pos;
    }
}
