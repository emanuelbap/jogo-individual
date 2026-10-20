using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Ferramenta de montagem: a cena e as referências ficam salvas no projeto.
public static class SpaceProjectBuilder
{
    const string ScenePath = "Assets/Scenes/DefesaEspacial.unity";

    [InitializeOnLoadMethod]
    static void WatchBuildRequest()
    {
        EditorApplication.update -= CheckRequest;
        EditorApplication.update += CheckRequest;
    }

    static void CheckRequest()
    {
        const string request = "Delivery/request-build";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlaying) return;
        File.Delete(request);
        try { BuildWebGL(); }
        catch (Exception e) { File.WriteAllText("Delivery/build-error.txt", e.ToString()); Debug.LogException(e); }
    }

    [MenuItem("Defesa Espacial/1 - Montar cena")]
    public static void CreateScene()
    {
        // No editor interativo, nunca descarte uma cena com mudanças sem avisar.
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            throw new OperationCanceledException("Montagem cancelada para preservar a cena aberta.");
        ImportSprite("Assets/Art/ship.png", 64);
        ImportSprite("Assets/Art/asteroid.png", 64);
        ImportSprite("Assets/Art/background.png", 54);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFrame));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0, 0, -10);
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.02f, 0.04f, 0.1f);
        var background = new GameObject("Fundo estrelado", typeof(SpriteRenderer));
        background.GetComponent<SpriteRenderer>().sprite = Load<Sprite>("Assets/Art/background.png");
        background.GetComponent<SpriteRenderer>().sortingOrder = -10;

        var manager = new GameObject("GameManager").AddComponent<GameManager>();
        var ship = new GameObject("Nave", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D));
        ship.transform.position = new Vector3(0, -3.5f, 0);
        ship.GetComponent<SpriteRenderer>().sprite = Load<Sprite>("Assets/Art/ship.png");
        ship.GetComponent<SpriteRenderer>().sortingOrder = 3;
        var body = ship.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0;
        ship.GetComponent<BoxCollider2D>().size = new Vector2(0.6f, 0.72f);
        ship.GetComponent<BoxCollider2D>().isTrigger = true;
        var player = ship.AddComponent<PlayerController>();
        player.game = manager;
        manager.player = player;
        var spawner = new GameObject("AsteroidSpawner").AddComponent<AsteroidSpawner>();
        spawner.game = manager;
        spawner.asteroidSprite = Load<Sprite>("Assets/Art/asteroid.png");
        manager.spawner = spawner;
        var audio = new GameObject("Audio").AddComponent<GameAudio>();
        audio.music = audio.gameObject.AddComponent<AudioSource>();
        audio.music.clip = Load<AudioClip>("Assets/Audio/music.wav");
        audio.music.loop = true;
        audio.music.playOnAwake = false;
        audio.music.volume = 0.45f;
        audio.effects = audio.gameObject.AddComponent<AudioSource>();
        audio.effects.playOnAwake = false;
        audio.effects.volume = 0.65f;
        audio.startSound = Load<AudioClip>("Assets/Audio/start.wav");
        audio.hitSound = Load<AudioClip>("Assets/Audio/hit.wav");
        audio.gameOverSound = Load<AudioClip>("Assets/Audio/gameover.wav");
        manager.audioPlayer = audio;
        var ui = new GameObject("Interface", typeof(RectTransform)).AddComponent<GameUI>();
        ui.game = manager;
        manager.ui = ui;
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.productName = "Defesa Espacial";
        PlayerSettings.companyName = "emanuelbap";
        PlayerSettings.defaultScreenWidth = 960;
        PlayerSettings.defaultScreenHeight = 540;
        PlayerSettings.runInBackground = false;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.template = "APPLICATION:Default";
        // Sprites simples usam o renderer padrão, dispensando luzes e shaders URP.
        GraphicsSettings.defaultRenderPipeline = null;
        int originalQuality = QualitySettings.GetQualityLevel();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = null;
        }
        QualitySettings.SetQualityLevel(originalQuality, false);
        AssetDatabase.SaveAssets();
        Debug.Log("Defesa Espacial: cena montada.");
    }

    [MenuItem("Defesa Espacial/2 - Gerar WebGL")]
    public static void BuildWebGL()
    {
        if (!File.Exists(ScenePath)) CreateScene();
        Directory.CreateDirectory("Delivery");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { ScenePath }, locationPathName = "Delivery/WebGL",
            target = BuildTarget.WebGL, options = BuildOptions.None
        });
        File.WriteAllText("Delivery/build-result.txt", $"Resultado: {report.summary.result}\nErros: {report.summary.totalErrors}\nTamanho: {report.summary.totalSize} bytes\n");
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Falha no build WebGL. Veja o Console do Unity.");
        Debug.Log("Defesa Espacial: WebGL pronto em Delivery/WebGL.");
    }

    static T Load<T>(string path) where T : UnityEngine.Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) throw new Exception("Asset não encontrado: " + path);
        return asset;
    }

    static void ImportSprite(string path, float pixelsPerUnit)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }
}
