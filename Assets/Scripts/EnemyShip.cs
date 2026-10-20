using UnityEngine;

// Persegue na horizontal durante a aproximação e segue reto depois de atacar.
public class EnemyShip : MonoBehaviour
{
    public GameManager game;
    public float fallSpeed = 2.2f;
    public float trackingSpeed = 2;
    bool attacking;

    void Update()
    {
        if (game.State != GameManager.GameState.Playing || game.Paused) return;
        Vector3 position = transform.position;
        if (position.y <= game.player.transform.position.y + 0.8f) attacking = true;
        if (!attacking)
        {
            position.x = Mathf.MoveTowards(position.x, game.player.transform.position.x,
                trackingSpeed * Time.deltaTime);
        }
        position.y -= fallSpeed * Time.deltaTime;
        transform.position = position;
        if (position.y < -6) Destroy(gameObject);
    }
}
