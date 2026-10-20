using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public GameManager game;
    public Sprite asteroidSprite;
    public Material asteroidMaterial;
    public float initialInterval = 0.95f;
    public float minimumInterval = 0.32f;
    float timer;

    void Update()
    {
        if (game.State != GameManager.GameState.Playing || game.Paused) return;
        timer -= Time.deltaTime;
        if (timer > 0) return;
        Spawn();
        // Limites mantêm o desafio possível mesmo em partidas longas.
        timer = Mathf.Max(minimumInterval, initialInterval - game.SurvivalTime * 0.006f);
    }

    void Spawn()
    {
        var rock = new GameObject("Asteroide");
        rock.transform.SetParent(transform);
        rock.transform.position = new Vector3(Random.Range(-8f, 8f), 5.6f, 0);
        rock.transform.localScale = Vector3.one * Random.Range(0.8f, 1.25f);
        var renderer = rock.AddComponent<SpriteRenderer>();
        renderer.sprite = asteroidSprite;
        renderer.sharedMaterial = asteroidMaterial;
        renderer.sortingOrder = 2;
        var collider = rock.AddComponent<CircleCollider2D>();
        collider.radius = 0.42f;
        collider.isTrigger = true;
        var asteroid = rock.AddComponent<Asteroid>();
        asteroid.game = game;
        asteroid.fallSpeed = Mathf.Min(7, 2.8f + game.SurvivalTime * 0.035f);
        asteroid.rotationSpeed = Random.Range(-90f, 90f);
    }

    public void ResetTimer() { timer = 0.8f; }

    public void Clear()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }
}
