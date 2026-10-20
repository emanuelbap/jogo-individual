using System;
using System.IO;
using System.Reflection;
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
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        const string resolutionRequest = "Delivery/request-resolution";
        if (EditorApplication.isPlaying)
        {
            if (File.Exists(resolutionRequest)) EditorApplication.isPlaying = false;
            return;
        }
        if (File.Exists(resolutionRequest))
        {
            File.Delete(resolutionRequest);
            SetGameViewSize();
        }
        const string request = "Delivery/request-build";
        if (!File.Exists(request)) return;
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
        ImportSprite("Assets/Art/enemy.png", 64);
        ImportSprite("Assets/Art/background.png", 54);
        const string materialPath = "Assets/Art/SpaceSprite.mat";
        var spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (spriteMaterial == null)
        {
            spriteMaterial = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(spriteMaterial, materialPath);
        }
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
        background.GetComponent<SpriteRenderer>().sharedMaterial = spriteMaterial;

        var manager = new GameObject("GameManager").AddComponent<GameManager>();
        var ship = new GameObject("Nave", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D));
        ship.transform.position = new Vector3(0, -3.5f, 0);
        ship.GetComponent<SpriteRenderer>().sprite = Load<Sprite>("Assets/Art/ship.png");
        ship.GetComponent<SpriteRenderer>().sortingOrder = 3;
        ship.GetComponent<SpriteRenderer>().sharedMaterial = spriteMaterial;
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
        spawner.asteroidMaterial = spriteMaterial;
        spawner.initialInterval = 1.05f;
        spawner.minimumInterval = 0.65f;
        spawner.enemySprite = Load<Sprite>("Assets/Art/enemy.png");
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
        PlayerSettings.defaultScreenWidth = 600;
        PlayerSettings.defaultScreenHeight = 480;
        PlayerSettings.defaultWebScreenWidth = 600;
        PlayerSettings.defaultWebScreenHeight = 480;
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

    [MenuItem("Defesa Espacial/3 - Tela 600 x 480")]
    public static void SetGameViewSize()
    {
        // O Unity não expõe a seleção do tamanho da aba Game por uma API pública.
        var assembly = typeof(Editor).Assembly;
        var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
        var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var sizes = singleton.GetProperty("instance").GetValue(null);
        var getGroup = sizesType.GetMethod("GetGroup");
        var groupEnum = getGroup.GetParameters()[0].ParameterType;
        var group = getGroup.Invoke(sizes, new[] { Enum.Parse(groupEnum, "Standalone") });
        var groupType = group.GetType();
        int count = (int)groupType.GetMethod("GetBuiltinCount").Invoke(group, null)
            + (int)groupType.GetMethod("GetCustomCount").Invoke(group, null);
        int selected = -1;
        for (int i = 0; i < count; i++)
        {
            var size = groupType.GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
            if ((int)size.GetType().GetProperty("width").GetValue(size) == 600
                && (int)size.GetType().GetProperty("height").GetValue(size) == 480) selected = i;
        }
        if (selected < 0)
        {
            var sizeType = assembly.GetType("UnityEditor.GameViewSize");
            var kind = assembly.GetType("UnityEditor.GameViewSizeType");
            var size = Activator.CreateInstance(sizeType, new object[] {
                Enum.Parse(kind, "FixedResolution"), 600, 480, "Defesa Espacial" });
            groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
            selected = count;
        }
        var gameViewType = assembly.GetType("UnityEditor.GameView");
        var window = EditorWindow.GetWindow(gameViewType);
        gameViewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .SetValue(window, selected);
        window.Repaint();
        Debug.Log("Defesa Espacial: aba Game em 600 x 480.");
    }

    public static void CreateAndBuild()
    {
        CreateScene();
        BuildWebGL();
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
        // O rodapé do template ocupa 38 px e cortaria o jogo no iframe 600 x 480.
        string cssPath = "Delivery/WebGL/TemplateData/style.css";
        if (File.Exists(cssPath))
            File.AppendAllText(cssPath, "\n#unity-footer { display: none; }\nbody { overflow: hidden; }\n");
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
