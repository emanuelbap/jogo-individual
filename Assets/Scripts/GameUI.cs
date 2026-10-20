using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

// A interface é montada com UI padrão, sem pacotes ou fontes externas.
public class GameUI : MonoBehaviour
{
    public GameManager game;
    Text timeLabel, livesLabel, bestLabel, title, description, finalTime, soundLabel;
    GameObject overlay, hud;
    Button primaryButton, menuButton, soundButton, pauseButton;
    Font font;
    readonly Color cyan = new Color(0.25f, 0.85f, 1);

    void Awake()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 5;
        canvas.sortingOrder = 100;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960, 540);
        gameObject.AddComponent<GraphicRaycaster>();
        var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

        hud = Panel("Informações", transform, Color.clear, Vector2.zero, new Vector2(960, 540));
        timeLabel = Label(hud.transform, "TEMPO  0,0 s", new Vector2(-330, 220), 24, new Vector2(270, 40));
        livesLabel = Label(hud.transform, "VIDAS  3 / 3", new Vector2(335, 220), 24, new Vector2(250, 40));
        livesLabel.color = new Color(1, 0.55f, 0.4f);
        pauseButton = MakeButton(hud.transform, "II", new Vector2(0, 220), new Vector2(54, 36), game.TogglePause);
        bestLabel = Label(transform, "", new Vector2(0, -245), 16, new Vector2(900, 28));
        bestLabel.color = new Color(0.55f, 0.65f, 0.8f);

        overlay = Panel("Painel", transform, new Color(0.015f, 0.025f, 0.07f, 0.94f), Vector2.zero, new Vector2(960, 540));
        Label(overlay.transform, "ARCADE  /  SOBREVIVÊNCIA", new Vector2(0, 205), 16, new Vector2(800, 30)).color = cyan;
        title = Label(overlay.transform, "DEFESA ESPACIAL", new Vector2(0, 142), 48, new Vector2(900, 75));
        description = Label(overlay.transform, "", new Vector2(0, 60), 21, new Vector2(850, 75));
        finalTime = Label(overlay.transform, "", new Vector2(0, -3), 28, new Vector2(850, 40));
        finalTime.color = cyan;
        primaryButton = MakeButton(overlay.transform, "INICIAR", new Vector2(0, -73), new Vector2(290, 52), PrimaryAction);
        menuButton = MakeButton(overlay.transform, "MENU PRINCIPAL", new Vector2(0, -135), new Vector2(290, 42), game.ReturnToMenu);
        soundButton = MakeButton(overlay.transform, "", new Vector2(0, -188), new Vector2(180, 34), ToggleSound);
        soundLabel = soundButton.GetComponentInChildren<Text>();
        Label(overlay.transform, "Mover: W A S D ou setas  •  Controle: analógico / direcional\nEnter / Espaço / A: iniciar  •  Esc / Start: pausar", new Vector2(0, -239), 16, new Vector2(900, 50));
    }

    void PrimaryAction()
    {
        if (game.Paused) game.TogglePause();
        else game.StartGame();
    }

    void ToggleSound()
    {
        game.audioPlayer.ToggleMute();
        soundLabel.text = game.audioPlayer.Muted ? "SOM: DESLIGADO" : "SOM: LIGADO";
    }

    public void UpdateHUD()
    {
        timeLabel.text = $"TEMPO  {game.SurvivalTime:0.0} s";
        livesLabel.text = $"VIDAS  {game.Lives} / 3";
    }

    public void Refresh()
    {
        bool menu = game.State == GameManager.GameState.Menu;
        bool over = game.State == GameManager.GameState.GameOver;
        overlay.SetActive(menu || over || game.Paused);
        hud.SetActive(!menu && !over && !game.Paused);
        bestLabel.gameObject.SetActive(!menu && !over && !game.Paused);
        title.text = menu ? "DEFESA ESPACIAL" : over ? "FIM DE JOGO" : "PAUSADO";
        description.text = menu ? "Desvie dos asteroides e das naves inimigas.\nVocê tem 3 vidas. Os inimigos seguem você na aproximação."
            : over ? "Sua nave foi atingida. Vamos tentar mais uma vez?" : "Respire um pouco. Sua partida está guardada aqui.";
        finalTime.text = over ? $"VOCÊ SOBREVIVEU {game.SurvivalTime:0.0} s" : $"RECORDE  {game.BestTime:0.0} s";
        primaryButton.GetComponentInChildren<Text>().text = menu ? "INICIAR" : over ? "TENTAR NOVAMENTE" : "CONTINUAR";
        menuButton.gameObject.SetActive(!menu);
        soundLabel.text = game.audioPlayer.Muted ? "SOM: DESLIGADO" : "SOM: LIGADO";
        bestLabel.text = $"RECORDE  {game.BestTime:0.0} s   •   Quanto mais tempo, maior o desafio";
        EventSystem.current.SetSelectedGameObject(null);
        if (overlay.activeSelf) EventSystem.current.SetSelectedGameObject(primaryButton.gameObject);
        UpdateHUD();
    }

    GameObject Panel(string name, Transform parent, Color color, Vector2 position, Vector2 size)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        obj.GetComponent<Image>().color = color;
        obj.GetComponent<Image>().raycastTarget = color.a > 0;
        return obj;
    }

    Text Label(Transform parent, string value, Vector2 position, int size, Vector2 dimensions)
    {
        var obj = new GameObject("Texto", typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = position;
        var label = obj.GetComponent<Text>();
        label.font = font;
        label.text = value;
        label.fontSize = size;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
        return label;
    }

    Button MakeButton(Transform parent, string text, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        var obj = Panel(text, parent, new Color(0.055f, 0.22f, 0.32f), position, size);
        var button = obj.AddComponent<Button>();
        button.targetGraphic = obj.GetComponent<Image>();
        var colors = button.colors;
        colors.highlightedColor = new Color(0.4f, 0.9f, 1);
        colors.selectedColor = new Color(0.4f, 0.9f, 1);
        button.colors = colors;
        button.onClick.AddListener(action);
        Label(obj.transform, text, Vector2.zero, 20, size);
        return button;
    }
}
