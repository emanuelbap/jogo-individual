using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public GameManager game;
    public float fallSpeed;
    public float rotationSpeed;

    void Update()
    {
        if (game.State != GameManager.GameState.Playing || game.Paused) return;
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        if (transform.position.y < -6) Destroy(gameObject);
    }
}
