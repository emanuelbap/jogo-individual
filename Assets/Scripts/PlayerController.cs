using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public GameManager game;
    public float speed = 8;
    public float horizontalLimit = 8.2f;
    public float protectionSeconds = 1.5f;
    public bool IsInvulnerable => protectionTime > 0;
    float protectionTime;
    SpriteRenderer spriteRenderer;

    void Awake() { spriteRenderer = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        if (game.State != GameManager.GameState.Playing || game.Paused) return;
        float movement = 0;
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) movement--;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) movement++;
        }
        var pad = Gamepad.current;
        if (pad != null)
        {
            float axis = pad.leftStick.ReadValue().x + pad.dpad.ReadValue().x;
            if (Mathf.Abs(axis) > 0.15f) movement = axis;
        }
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x + Mathf.Clamp(movement, -1, 1) * speed * Time.deltaTime,
            -horizontalLimit, horizontalLimit);
        transform.position = position;
        protectionTime = Mathf.Max(0, protectionTime - Time.deltaTime);
        spriteRenderer.color = new Color(1, 1, 1, IsInvulnerable && Mathf.Sin(Time.time * 32) > 0 ? 0.3f : 1);
    }

    public void Protect() { protectionTime = protectionSeconds; }

    public void ResetPosition()
    {
        transform.position = new Vector3(0, -3.5f, 0);
        protectionTime = 0;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var asteroid = other.GetComponent<Asteroid>();
        if (asteroid == null || game.State != GameManager.GameState.Playing || game.Paused) return;
        game.TakeDamage();
        Destroy(asteroid.gameObject);
    }
}
