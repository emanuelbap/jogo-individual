using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public GameManager game;
    public Sprite asteroidSprite;
    public Sprite enemySprite;
    public Material asteroidMaterial;
    public float initialInterval = 0.95f;
    public float minimumInterval = 0.48f;
    public float secondsToMaxDifficulty = 40;
    float timer;
    float enemyTimer;

    float Difficulty => Mathf.Clamp01(game.SurvivalTime / Mathf.Max(1, secondsToMaxDifficulty));

    void Update()
    {
        if (game.State != GameManager.GameState.Playing || game.Paused) return;
        timer -= Time.deltaTime;
        enemyTimer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnAsteroid();
            timer = Mathf.Lerp(initialInterval, minimumInterval, Difficulty);
        }
        if (enemyTimer <= 0)
        {
            // No máximo duas naves inimigas: o jogador sempre pode fugir delas.
            int enemies = 0;
            foreach (Transform child in transform)
                if (child.gameObject.activeSelf && child.GetComponent<EnemyShip>() != null) enemies++;
            if (enemies < 2) SpawnEnemy();
            enemyTimer = Mathf.Lerp(7, 4, Difficulty);
        }
    }

    float SpawnX()
    {
        // Evita criar um perigo diretamente sobre a nave quando ela está no alto.
        Vector3 player = game.player.transform.position;
        if (player.y > 1.5f)
            return player.x >= 0 ? Random.Range(-8f, -1.5f) : Random.Range(1.5f, 8f);
        return Random.Range(-8f, 8f);
    }

    GameObject MakeHazard(string name, Sprite sprite)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(transform);
        obj.transform.position = new Vector3(SpawnX(), 5.6f, 0);
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = asteroidMaterial;
        renderer.sortingOrder = 2;
        var collider = obj.AddComponent<CircleCollider2D>();
        collider.radius = 0.42f;
        collider.isTrigger = true;
        return obj;
    }

    void SpawnAsteroid()
    {
        var rock = MakeHazard("Asteroide", asteroidSprite);
        rock.transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);
        var asteroid = rock.AddComponent<Asteroid>();
        asteroid.game = game;
        asteroid.fallSpeed = Mathf.Lerp(2.8f, 6, Difficulty);
        asteroid.rotationSpeed = Random.Range(-90f, 90f);
    }

    void SpawnEnemy()
    {
        var obj = MakeHazard("Nave inimiga", enemySprite);
        var enemy = obj.AddComponent<EnemyShip>();
        enemy.game = game;
        enemy.fallSpeed = Mathf.Lerp(2.2f, 3.4f, Difficulty);
        enemy.trackingSpeed = Mathf.Lerp(2, 3, Difficulty);
    }

    public void ResetTimer() { timer = 0.8f; enemyTimer = 8; }

    public void Clear()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }
}
