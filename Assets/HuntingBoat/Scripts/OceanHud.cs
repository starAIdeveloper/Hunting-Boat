using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HuntingBoat
{
    public sealed class HeldControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public System.Action<bool> Changed;
        public void OnPointerDown(PointerEventData data) { Changed?.Invoke(true); }
        public void OnPointerUp(PointerEventData data) { Changed?.Invoke(false); }
        private void OnDisable() { Changed?.Invoke(false); }
    }

    public sealed class OceanHud : MonoBehaviour
    {
        private OceanGame game;
        private Canvas canvas;
        private Font font;
        private Text speed, objective, message, fish, economy, controls, menuText;
        private Image tensionFill, progressFill;
        private GameObject fishingPanel, menu, fullMap, driving, fighting;
        private Button castButton;
        private RawImage miniMap, largeMap;
        private Texture2D mapTexture;
        private float uiScale;
        private readonly Color panelColor = new Color(.015f, .07f, .11f, .92f);
        private readonly Color cyan = new Color(.15f, .85f, .94f);

        public void Initialize(OceanGame owner)
        {
            game = owner; font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = .5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if (!FindFirstObjectByType<EventSystem>()) { var e = new GameObject("UI Event System"); e.AddComponent<EventSystem>(); e.AddComponent<StandaloneInputModule>(); }
            Label(transform, "HUNTING  BOAT", new Vector2(25, -20), new Vector2(360, 45), 30, cyan, new Vector2(0, 1));
            Label(transform, "OCEAN EXPEDITION  /  FISH · EXPLORE · NAVIGATE", new Vector2(28, -63), new Vector2(520, 25), 12, Color.white, new Vector2(0, 1));
            var mission = Panel(transform, new Vector2(25, -103), new Vector2(330, 100), new Vector2(0, 1));
            objective = Label(mission, "", new Vector2(14, -10), new Vector2(306, 80), 16, Color.white, new Vector2(0, 1));
            var instrument = Panel(transform, new Vector2(-25, -22), new Vector2(260, 106), new Vector2(1, 1));
            speed = Label(instrument, "", new Vector2(15, -8), new Vector2(230, 88), 22, Color.white, new Vector2(0, 1));
            message = Label(transform, "", new Vector2(0, 23), new Vector2(890, 62), 17, Color.white, new Vector2(.5f, 0)); message.alignment = TextAnchor.MiddleCenter;
            var messageBg = message.gameObject.AddComponent<Outline>(); messageBg.effectColor = Color.black;
            economy = Label(transform, "", new Vector2(-25, -138), new Vector2(290, 55), 14, cyan, new Vector2(1, 1)); economy.alignment = TextAnchor.UpperRight;

            var mapPanel = Panel(transform, new Vector2(25, 90), new Vector2(170, 170), new Vector2(0, 0));
            miniMap = ImageMap(mapPanel, new Vector2(8, -8), new Vector2(154, 154), new Vector2(0, 1));
            mapTexture = new Texture2D(256, 256, TextureFormat.RGB24, false); mapTexture.filterMode = FilterMode.Bilinear; miniMap.texture = mapTexture;
            var mapButton = Button(transform, "Map [M]", new Vector2(25, 52), new Vector2(170, 32), new Vector2(0, 0), ToggleMap);
            controls = Label(transform, "WASD / arrows: navigate\nF: cast / hook    SPACE: reel\nSHIFT: pull    R: release\nC: camera   ESC: menu", new Vector2(-25, 105), new Vector2(275, 100), 14, Color.white, new Vector2(1, 0)); controls.alignment = TextAnchor.MiddleRight;
            Button(transform, "Menu / Upgrades", new Vector2(-25, 53), new Vector2(190, 35), new Vector2(1, 0), game.ToggleMenu);
            castButton = Button(transform, "Cast [F]", new Vector2(0, 100), new Vector2(160, 43), new Vector2(.5f, 0), game.CastOrHook);

            fishingPanel = Panel(transform, new Vector2(0, -22), new Vector2(430, 125), new Vector2(.5f, 1)).gameObject;
            fish = Label(fishingPanel.transform, "", new Vector2(15, -10), new Vector2(400, 28), 20, Color.white, new Vector2(0, 1));
            Bar(fishingPanel.transform, new Vector2(15, -48), new Vector2(400, 19), out tensionFill);
            Label(fishingPanel.transform, "LINE TENSION  •  REEL, THEN EASE OFF", new Vector2(15, -70), new Vector2(400, 20), 11, Color.white, new Vector2(0, 1));
            Bar(fishingPanel.transform, new Vector2(15, -95), new Vector2(400, 9), out progressFill); progressFill.color = cyan;
            fishingPanel.SetActive(false);

            driving = new GameObject("Touch helm controls", typeof(RectTransform)); driving.transform.SetParent(transform, false);
            Hold(driving.transform, "▲", new Vector2(110, 87), new Vector2(0, 0), held => game.TouchThrottle = held ? 1 : 0);
            Hold(driving.transform, "▼", new Vector2(110, 25), new Vector2(0, 0), held => game.TouchThrottle = held ? -1 : 0);
            Hold(driving.transform, "◀", new Vector2(40, 25), new Vector2(0, 0), held => game.TouchSteering = held ? -1 : 0);
            Hold(driving.transform, "▶", new Vector2(180, 25), new Vector2(0, 0), held => game.TouchSteering = held ? 1 : 0);
            fighting = new GameObject("Touch fishing controls", typeof(RectTransform)); fighting.transform.SetParent(transform, false);
            Hold(fighting.transform, "REEL", new Vector2(-160, 110), new Vector2(1, 0), held => game.TouchReel = held);
            Hold(fighting.transform, "PULL", new Vector2(-85, 110), new Vector2(1, 0), held => game.TouchPull = held);
            Button(fighting.transform, "Release", new Vector2(-25, 190), new Vector2(140, 35), new Vector2(1, 0), () => game.Fishing.Release());

            menu = Panel(transform, Vector2.zero, new Vector2(590, 560), new Vector2(.5f, .5f)).gameObject;
            Label(menu.transform, "CAPTAIN'S LOG", new Vector2(25, -20), new Vector2(530, 45), 27, cyan, new Vector2(0, 1));
            menuText = Label(menu.transform, "", new Vector2(25, -78), new Vector2(530, 260), 17, Color.white, new Vector2(0, 1));
            Button(menu.transform, "Upgrade engine", new Vector2(25, 158), new Vector2(255, 40), new Vector2(0, 0), () => game.Buy(true));
            Button(menu.transform, "Upgrade rod", new Vector2(305, 158), new Vector2(255, 40), new Vector2(0, 0), () => game.Buy(false));
            Button(menu.transform, "Save voyage", new Vector2(25, 106), new Vector2(165, 38), new Vector2(0, 0), () => game.SaveVoyage());
            Button(menu.transform, "Load voyage", new Vector2(212, 106), new Vector2(165, 38), new Vector2(0, 0), game.LoadVoyage);
            Button(menu.transform, "Weather", new Vector2(395, 106), new Vector2(165, 38), new Vector2(0, 0), game.ChangeWeather);
            Button(menu.transform, "Recover boat", new Vector2(25, 54), new Vector2(165, 38), new Vector2(0, 0), game.Recover);
            Button(menu.transform, "Camera", new Vector2(212, 54), new Vector2(165, 38), new Vector2(0, 0), game.ChangeCamera);
            Button(menu.transform, "Resume", new Vector2(395, 54), new Vector2(165, 38), new Vector2(0, 0), game.ToggleMenu);
            Label(menu.transform, "", new Vector2(25, 10), new Vector2(530, 35), 12, Color.gray, new Vector2(0, 0));
            menu.SetActive(false);
            fullMap = Panel(transform, Vector2.zero, new Vector2(650, 600), new Vector2(.5f, .5f)).gameObject;
            Label(fullMap.transform, "ARCHIPELAGO / WAYPOINTS", new Vector2(20, -15), new Vector2(560, 40), 22, cyan, new Vector2(0, 1));
            largeMap = ImageMap(fullMap.transform, new Vector2(110, -65), new Vector2(430, 430), new Vector2(0, 1)); largeMap.texture = mapTexture;
            for (int i = 0; i < 3; i++) { int spot = i; Button(fullMap.transform, game.World.SpotNames[i], new Vector2(15 + i * 210, 52), new Vector2(195, 36), new Vector2(0, 0), () => { game.SelectSpot(spot); ToggleMap(); }); }
            Button(fullMap.transform, "Close map", new Vector2(450, 8), new Vector2(180, 30), new Vector2(0, 0), ToggleMap);
            fullMap.SetActive(false); UpdateMap();
        }
        public void ToggleMap() { fullMap.SetActive(!fullMap.activeSelf); UpdateMap(); }
        public void SetMenu(bool visible) { menu.SetActive(visible); if (visible) fullMap.SetActive(false); }
        public void Refresh()
        {
            var save = game.Save; var f = game.Fishing; bool active = f.Phase == FishingPhase.Waiting || f.Phase == FishingPhase.Bite || f.Phase == FishingPhase.Fighting;
            speed.text = game.World.Boat.SpeedKmh.ToString("F0") + " km/h\nHDG " + game.World.Boat.Heading.ToString("F0") + "°    Hull " + game.Hull.ToString("F0") + "%";
            objective.text = "CURRENT OBJECTIVE\n" + (save.catches.Count >= 3 ? "✓ Catch mission complete" : "Catch 3 fish (" + save.catches.Count + "/3)") + "\n" + game.World.SpotNames[game.TargetSpot] + " · " + game.DistanceToSpot.ToString("F0") + " m";
            economy.text = save.credits + " credits  •  Rod Lv " + save.rodLevel + "\n" + ((WeatherKind)save.weather).ToString();
            message.text = game.Message; fishingPanel.SetActive(active);
            fish.text = f.Phase == FishingPhase.Waiting ? "Waiting for a bite…" : f.Phase == FishingPhase.Bite ? "BITE! SET THE HOOK" : f.Species + " · " + f.Weight.ToString("F1") + " kg";
            tensionFill.fillAmount = f.Tension; tensionFill.color = f.Tension > .82f || f.Tension < .12f ? new Color(1, .25f, .15f) : f.Tension > .65f ? new Color(1, .74f, .10f) : new Color(.22f, .8f, .35f); progressFill.fillAmount = f.Progress;
            castButton.GetComponentInChildren<Text>().text = f.Phase == FishingPhase.Bite ? "HOOK! [F]" : active ? "Line deployed" : "Cast [F]";
            castButton.interactable = !game.MenuOpen && (!active || f.Phase == FishingPhase.Bite);
            bool touch = Application.isMobilePlatform || Screen.width < 900;
            driving.SetActive(touch && !active && !game.MenuOpen); fighting.SetActive(active && !game.MenuOpen); controls.gameObject.SetActive(!touch);
            if (menu.activeSelf)
            {
                string history = save.catches.Count == 0 ? "No fish landed yet." : "LAST CATCHES\n";
                for (int i = Mathf.Max(0, save.catches.Count - 4); i < save.catches.Count; i++) history += save.catches[i].species + "  " + save.catches[i].weight.ToString("F1") + " kg\n";
                menuText.text = save.credits + " credits\nEngine Lv " + save.engineLevel + "  (upgrade " + Economy.UpgradeCost(save.engineLevel) + ")\nRod Lv " + save.rodLevel + "  (upgrade " + Economy.UpgradeCost(save.rodLevel) + ")\n\n" + history;
            }
        }
        private Transform Panel(Transform parent, Vector2 position, Vector2 size, Vector2 anchor)
        {
            var g = new GameObject("Panel", typeof(RectTransform), typeof(Image)); g.transform.SetParent(parent, false); Place(g.GetComponent<RectTransform>(), position, size, anchor); g.GetComponent<Image>().color = panelColor; return g.transform;
        }
        private Text Label(Transform parent, string text, Vector2 position, Vector2 size, int fontSize, Color color, Vector2 anchor)
        {
            var g = new GameObject("Label", typeof(RectTransform), typeof(Text)); g.transform.SetParent(parent, false); Place(g.GetComponent<RectTransform>(), position, size, anchor);
            var t = g.GetComponent<Text>(); t.font = font; t.fontSize = fontSize; t.color = color; t.text = text; t.raycastTarget = false; t.horizontalOverflow = HorizontalWrapMode.Wrap; return t;
        }
        private Button Button(Transform parent, string label, Vector2 position, Vector2 size, Vector2 anchor, UnityEngine.Events.UnityAction action)
        {
            var g = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button)); g.transform.SetParent(parent, false); Place(g.GetComponent<RectTransform>(), position, size, anchor); g.GetComponent<Image>().color = new Color(.025f, .24f, .32f, .98f);
            var b = g.GetComponent<Button>(); b.onClick.AddListener(action); var text = Label(g.transform, label, Vector2.zero, size, 15, Color.white, new Vector2(.5f, .5f)); text.alignment = TextAnchor.MiddleCenter; return b;
        }
        private void Hold(Transform parent, string label, Vector2 position, Vector2 anchor, System.Action<bool> action)
        {
            var b = Button(parent, label, position, new Vector2(65, 54), anchor, () => { }); b.gameObject.AddComponent<HeldControl>().Changed = action;
        }
        private static void Place(RectTransform rect, Vector2 position, Vector2 size, Vector2 anchor)
        {
            rect.anchorMin = rect.anchorMax = anchor; rect.pivot = anchor; rect.anchoredPosition = position; rect.sizeDelta = size;
        }
        private RawImage ImageMap(Transform parent, Vector2 pos, Vector2 size, Vector2 anchor)
        {
            var g = new GameObject("Navigation chart", typeof(RectTransform), typeof(RawImage)); g.transform.SetParent(parent, false); Place(g.GetComponent<RectTransform>(), pos, size, anchor); return g.GetComponent<RawImage>();
        }
        private void Bar(Transform parent, Vector2 pos, Vector2 size, out Image fill)
        {
            var bg = Panel(parent, pos, size, new Vector2(0, 1)); bg.GetComponent<Image>().color = new Color(.01f, .02f, .04f);
            var g = new GameObject("Fill", typeof(RectTransform), typeof(Image)); g.transform.SetParent(bg, false); Place(g.GetComponent<RectTransform>(), Vector2.zero, size, new Vector2(0, 1)); fill = g.GetComponent<Image>(); fill.sprite = WhiteSprite(); fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = 0;
        }
        private static Sprite WhiteSprite() { return Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), new Vector2(.5f, .5f)); }
        public void UpdateMap()
        {
            if (!mapTexture) return;
            var pixels = new Color[256 * 256]; var water = new Color(.015f, .14f, .20f);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = water;
            foreach (var island in game.World.Islands) Circle(pixels, Map(island), 13, new Color(.31f, .46f, .31f));
            for (int i = 0; i < game.World.Spots.Length; i++) Circle(pixels, Map(game.World.Spots[i]), i == game.TargetSpot ? 5 : 3, i == game.TargetSpot ? new Color(1, .75f, .1f) : cyan);
            Circle(pixels, Map(game.World.Boat.transform.position), 4, Color.white);
            mapTexture.SetPixels(pixels); mapTexture.Apply(false);
        }
        private static Vector2Int Map(Vector3 p) { return new Vector2Int(Mathf.RoundToInt(128 + p.x / 4), Mathf.RoundToInt(128 + p.z / 4)); }
        private static void Circle(Color[] pixels, Vector2Int center, int r, Color color)
        {
            for (int y = -r; y <= r; y++) for (int x = -r; x <= r; x++) { int a = center.x + x, b = center.y + y; if (x * x + y * y <= r * r && a >= 0 && a < 256 && b >= 0 && b < 256) pixels[b * 256 + a] = color; }
        }
    }
}
