using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public GameManager game;
    public float speed = 8;
    public float horizontalLimit = 8.2f;
    public float bottomLimit = -4.1f;
    public float topLimit = 3.3f;
    public float protectionSeconds = 1.5f;
    public bool IsInvulnerable => protectionTime > 0;
    float protectionTime;
    SpriteRenderer spriteRenderer;

    void Awake() { spriteRenderer = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        if (game.State != GameManager.GameState.Playing || game.Paused) return;
        Vector2 movement = Vector2.zero;
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) movement.x--;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) movement.x++;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) movement.y--;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) movement.y++;
        }
        var pad = Gamepad.current;
        if (pad != null)
        {
            Vector2 axis = pad.leftStick.ReadValue() + pad.dpad.ReadValue();
            if (axis.magnitude > 0.15f) movement = axis;
        }
        // Normalizar impede que a diagonal fique mais rápida que os outros movimentos.
        movement = Vector2.ClampMagnitude(movement, 1);
        Vector3 position = transform.position + (Vector3)(movement * speed * Time.deltaTime);
        position.x = Mathf.Clamp(position.x, -horizontalLimit, horizontalLimit);
        position.y = Mathf.Clamp(position.y, bottomLimit, topLimit);
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
        bool hazard = other.GetComponent<Asteroid>() != null || other.GetComponent<EnemyShip>() != null;
        if (!hazard || game.State != GameManager.GameState.Playing || game.Paused) return;
        game.TakeDamage();
        Destroy(other.gameObject);
    }
}
