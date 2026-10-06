using System;
using UnityEngine;

namespace HuntingBoat
{
    public sealed class OceanGame : MonoBehaviour
    {
        public OceanWorld World { get; private set; }
        public SaveData Save { get; private set; }
        public FishingSession Fishing { get; private set; }
        public bool MenuOpen { get; private set; }
        public float Hull { get; private set; } = 100;
        public string Message { get; private set; } = "Welcome aboard. Follow the gold waypoint to Lighthouse Shoal.";
        public int TargetSpot { get; private set; }
        public bool Cockpit { get; private set; }
        public float TouchThrottle, TouchSteering;
        public bool TouchReel, TouchPull;
        public float DistanceToSpot { get { return Vector2.Distance(new Vector2(World.Boat.transform.position.x, World.Boat.transform.position.z), new Vector2(World.Spots[TargetSpot].x, World.Spots[TargetSpot].z)); } }
        private Camera view;
        private OceanHud hud;
        private FishingPhase lastPhase;
        private float environmentTime, mapTimer;
        private int weatherBefore = -1;
        private AudioSource engine;
        private Vector3 cameraVelocity;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!FindFirstObjectByType<OceanGame>()) new GameObject("Hunting Boat Game").AddComponent<OceanGame>();
        }
        private void Start()
        {
            QualitySettings.vSyncCount = 1; Application.targetFrameRate = 60;
            World = WorldBuilder.Build(); Save = new SaveData(); Fishing = new FishingSession(Environment.TickCount);
            World.Boat.OnImpact = speed => { Hull = Mathf.Max(0, Hull - speed * 2); Message = "Rock impact! Slow down near island shores."; if (Hull <= 0) Recover(); };
            var oldCamera = Camera.main; if (oldCamera) Destroy(oldCamera.gameObject);
            view = new GameObject("Voyage Camera").AddComponent<Camera>(); view.tag = "MainCamera"; view.nearClipPlane = .12f; view.farClipPlane = 1800; view.fieldOfView = 62;
            view.gameObject.AddComponent<AudioListener>();
            foreach (var listener in FindObjectsByType<AudioListener>(FindObjectsSortMode.None)) if (listener.gameObject != view.gameObject) Destroy(listener);
            view.transform.position = new Vector3(0, 7, -14);
            engine = World.Boat.gameObject.AddComponent<AudioSource>(); engine.clip = EngineSound(); engine.loop = true; engine.volume = .10f; engine.Play();
            hud = new GameObject("Voyage HUD").AddComponent<OceanHud>(); hud.Initialize(this);
            lastPhase = FishingPhase.Idle;
            ApplyEnvironment();
        }
        private void Update()
        {
            if (World == null) return;
            if (Input.GetKeyDown(KeyCode.Escape)) ToggleMenu();
            if (!MenuOpen)
            {
                if (Input.GetKeyDown(KeyCode.C)) Cockpit = !Cockpit;
                if (Input.GetKeyDown(KeyCode.F)) CastOrHook();
                if (Input.GetKeyDown(KeyCode.R)) Fishing.Release();
                if (Input.GetKeyDown(KeyCode.M)) hud.ToggleMap();
                if (Input.GetKeyDown(KeyCode.F5)) SaveVoyage();
                float keyboardThrottle = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
                float keyboardSteer = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
                World.Boat.Throttle = Mathf.Clamp(keyboardThrottle + TouchThrottle, -1, 1);
                World.Boat.Steering = Mathf.Clamp(keyboardSteer + TouchSteering, -1, 1);
                bool active = Fishing.Phase == FishingPhase.Waiting || Fishing.Phase == FishingPhase.Bite || Fishing.Phase == FishingPhase.Fighting;
                World.Boat.Anchored = active; World.Boat.EngineLevel = Save.engineLevel;
                Fishing.Tick(Time.deltaTime, TouchReel || Input.GetMouseButton(0) && !PointerOverUi() || Input.GetKey(KeyCode.Space), TouchPull || Input.GetMouseButton(1) && !PointerOverUi() || Input.GetKey(KeyCode.LeftShift));
                if (Fishing.Phase != lastPhase)
                {
                    if (Fishing.Phase == FishingPhase.Bite) Message = "Bite! Press F or Hook within 2.5 seconds.";
                    if (Fishing.Phase == FishingPhase.Escaped) Message = "The fish escaped. Rebalance tension next time, then cast again.";
                    if (Fishing.Phase == FishingPhase.Landed) LandFish();
                    lastPhase = Fishing.Phase;
                }
                for (int i = 0; i < World.Spots.Length; i++)
                    if (Vector3.Distance(new Vector3(World.Boat.transform.position.x, 0, World.Boat.transform.position.z), World.Spots[i]) < 32 && (Save.exploredMask & (1 << i)) == 0)
                    { Save.exploredMask |= 1 << i; Message = "Discovered " + World.SpotNames[i] + ". Stop and cast to begin fishing."; SaveVoyage(false); }
                environmentTime += Time.deltaTime;
                mapTimer -= Time.deltaTime; if (mapTimer <= 0) { hud.UpdateMap(); mapTimer = .25f; }
            }
            else { World.Boat.Throttle = World.Boat.Steering = 0; }
            var emission = World.Wake.emission; emission.rateOverTime = MenuOpen ? 0 : World.Boat.SpeedKmh * 1.2f;
            engine.volume = MenuOpen ? .02f : Mathf.Lerp(.035f, .12f, World.Boat.SpeedKmh / 50); engine.pitch = .65f + World.Boat.SpeedKmh / 60;
            if (weatherBefore != Save.weather) ApplyEnvironment();
            UpdateFishingVisual(); hud.Refresh();
        }
        private bool PointerOverUi() { return UnityEngine.EventSystems.EventSystem.current && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(); }
        private void LateUpdate()
        {
            OnLateCamera();
        }
        private void OnLateCamera()
        {
            if (World == null || view == null) return;
            Transform boat = World.Boat.transform;
            bool fishingView = Fishing.Phase == FishingPhase.Waiting || Fishing.Phase == FishingPhase.Bite || Fishing.Phase == FishingPhase.Fighting;
            Vector3 offset = fishingView ? new Vector3(1.3f, 2.25f, -.6f) : Cockpit ? new Vector3(0, 2.15f, -.6f) : new Vector3(0, 6, -13);
            Vector3 desired = boat.TransformPoint(offset);
            view.transform.position = Vector3.SmoothDamp(view.transform.position, desired, ref cameraVelocity, .25f, 100, Time.unscaledDeltaTime);
            Vector3 target = fishingView ? boat.TransformPoint(new Vector3(2, 1, 14)) : boat.TransformPoint(new Vector3(0, 1.5f, 8));
            view.transform.rotation = Quaternion.Slerp(view.transform.rotation, Quaternion.LookRotation(target - view.transform.position, Vector3.up), Time.unscaledDeltaTime * 6);
        }
        private void UpdateFishingVisual()
        {
            bool fighting = Fishing.Phase == FishingPhase.Fighting; bool active = fighting || Fishing.Phase == FishingPhase.Waiting || Fishing.Phase == FishingPhase.Bite;
            World.Rod.localRotation = Quaternion.Euler(-Fishing.Tension * 18, 0, 0);
            var line = World.Rod.GetComponent<LineRenderer>(); line.enabled = active;
            Vector3 target = World.Boat.transform.TransformPoint(new Vector3(2 + Mathf.Sin(Time.time * 1.6f) * 2, fighting ? 1.2f + Mathf.Max(0, Mathf.Sin(Time.time * 3)) * 1.7f : .1f, 12 - Fishing.Progress * 8));
            line.SetPosition(0, World.Rod.TransformPoint(new Vector3(.15f, 1.9f, 1.6f))); line.SetPosition(1, target);
            World.Fish.gameObject.SetActive(fighting); World.Fish.position = target; World.Fish.rotation = Quaternion.Euler(10 + Mathf.Sin(Time.time * 4) * 25, World.Boat.Heading + 160, Mathf.Sin(Time.time * 5) * 20);
        }
        public void CastOrHook()
        {
            if (MenuOpen) return;
            if (Fishing.Phase == FishingPhase.Bite) { Fishing.Hook(); Message = "Hook set. Reel below 60% tension; ease off during surges."; return; }
            if (Fishing.Phase == FishingPhase.Waiting || Fishing.Phase == FishingPhase.Fighting) return;
            if (DistanceToSpot > 40) { Message = "Navigate within 40 m of the selected fishing buoy first."; return; }
            if (World.Boat.SpeedKmh > 5) { Message = "Slow below 5 km/h before casting."; return; }
            Fishing.Cast(Save.rodLevel); Message = "Line cast. Watch for a bite; F or Hook sets the hook.";
        }
        private void LandFish()
        {
            if (Save.catches.Count >= 500) { Message = "Fish log is full. Catch released safely."; return; }
            var fish = new CatchRecord { species = Fishing.Species, weight = (float)Math.Round(Fishing.Weight, 2), length = (float)Math.Round(Fishing.Length, 1), caughtAt = DateTime.UtcNow.ToString("o") };
            Save.catches.Add(fish); int reward = Economy.Reward(fish.weight); Save.credits += reward;
            Message = fish.species + " landed! " + fish.weight.ToString("F1") + " kg · +" + reward + " credits."; SaveVoyage(false);
        }
        public void SelectSpot(int index) { TargetSpot = Mathf.Clamp(index, 0, 2); Message = "Waypoint set: " + World.SpotNames[TargetSpot]; }
        public void ChangeWeather() { Save.weather = (Save.weather + 1) % 4; ApplyEnvironment(); }
        public void ChangeCamera() { Cockpit = !Cockpit; }
        public void ToggleMenu()
        {
            MenuOpen = !MenuOpen; Time.timeScale = MenuOpen ? 0 : 1; TouchThrottle = TouchSteering = 0; TouchReel = TouchPull = false;
            hud.SetMenu(MenuOpen);
        }
        public void Buy(bool engineUpgrade)
        {
            bool success = engineUpgrade ? Economy.BuyEngine(Save) : Economy.BuyRod(Save);
            Message = success ? "Upgrade installed. Enjoy your next voyage." : "Not enough credits, or upgrade already maxed.";
            if (success) SaveVoyage(false);
        }
        public void SaveVoyage(bool notify = true)
        {
            Save.x = World.Boat.transform.position.x; Save.z = World.Boat.transform.position.z; Save.heading = World.Boat.Heading;
            bool ok = SaveStore.Write(Save, out string error); if (notify || !ok) Message = ok ? "Voyage saved locally." : "Save failed: " + error;
        }
        public void LoadVoyage()
        {
            if (!SaveStore.Read(out SaveData data, out string error)) { Message = error; return; }
            Save = data; World.Boat.Body.position = new Vector3(Save.x, .5f, Save.z); World.Boat.Body.rotation = Quaternion.Euler(0, Save.heading, 0); World.Boat.Body.linearVelocity = Vector3.zero; World.Boat.Body.angularVelocity = Vector3.zero;
            Fishing.Reset(); Hull = 100; ApplyEnvironment(); Message = "Saved voyage restored. Unfinished fishing sessions restart.";
        }
        public void Recover()
        {
            Fishing.Release(); World.Boat.Body.position = new Vector3(0, .5f, 0); World.Boat.Body.linearVelocity = Vector3.zero; World.Boat.Body.angularVelocity = Vector3.zero; Hull = 100; Message = "Coastguard recovery: boat returned to home waters.";
        }
        private void ApplyEnvironment()
        {
            weatherBefore = Save.weather; WeatherKind kind = (WeatherKind)Save.weather;
            bool night = kind == WeatherKind.Night, storm = kind == WeatherKind.Storm;
            World.Sun.transform.rotation = Quaternion.Euler(night ? 25 : kind == WeatherKind.Sunset ? 12 : 48, -35, 0);
            World.Sun.intensity = night ? .18f : storm ? .45f : 1.2f;
            World.Sun.color = night ? new Color(.45f, .62f, 1) : kind == WeatherKind.Sunset ? new Color(1, .69f, .39f) : new Color(.94f, .96f, 1);
            RenderSettings.ambientLight = night ? new Color(.09f, .13f, .20f) : storm ? new Color(.22f, .27f, .32f) : new Color(.39f, .45f, .47f);
            RenderSettings.fogColor = night ? new Color(.045f, .095f, .16f) : storm ? new Color(.23f, .31f, .38f) : new Color(.40f, .65f, .76f);
            RenderSettings.fogDensity = storm ? .004f : .0018f;
            RenderSettings.skybox.SetFloat("_Exposure", night ? .14f : storm ? .5f : 1.1f);
            World.Water.SetFloat("_WaveScale", storm ? 2 : .9f);
        }
        private static AudioClip EngineSound()
        {
            const int rate = 22050; var samples = new float[rate]; var rng = new System.Random(1);
            for (int i = 0; i < rate; i++) samples[i] = Mathf.Sin(i * 2 * Mathf.PI * 70 / rate) * .18f + Mathf.Sin(i * 2 * Mathf.PI * 140 / rate) * .08f + ((float)rng.NextDouble() * 2 - 1) * .035f;
            var clip = AudioClip.Create("Original synthesized marine engine", rate, 1, rate, false); clip.SetData(samples, 0); return clip;
        }
        private void OnApplicationQuit() { if (World != null) SaveVoyage(false); Time.timeScale = 1; }
        private void OnDisable() { Time.timeScale = 1; }
    }
}
