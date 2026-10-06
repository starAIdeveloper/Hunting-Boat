using System;
using System.Collections.Generic;

namespace HuntingBoat
{
    public enum FishingPhase { Idle, Waiting, Bite, Fighting, Landed, Escaped }
    public enum WeatherKind { Sunset, Clear, Storm, Night }

    [Serializable]
    public sealed class CatchRecord
    {
        public string species;
        public float weight;
        public float length;
        public string caughtAt;
    }

    [Serializable]
    public sealed class SaveData
    {
        public int version = 1;
        public int credits = 0;
        public int engineLevel = 1;
        public int rodLevel = 1;
        public int exploredMask = 0;
        public float x, z, heading;
        public int weather = 0;
        public List<CatchRecord> catches = new List<CatchRecord>();

        public bool IsValid()
        {
            if (version != 1 || credits < 0 || credits > 100000000 || engineLevel < 1 || engineLevel > 5 || rodLevel < 1 || rodLevel > 5 || exploredMask < 0 || exploredMask > 7 || weather < 0 || weather > 3 || catches == null || catches.Count > 500) return false;
            if (!Finite(x) || !Finite(z) || !Finite(heading) || Math.Abs(x) > 850 || Math.Abs(z) > 850) return false;
            foreach (var fish in catches)
                if (fish == null || string.IsNullOrEmpty(fish.species) || fish.species.Length > 60 || !Finite(fish.weight) || fish.weight <= 0 || fish.weight > 100 || !Finite(fish.length) || fish.length <= 0 || fish.length > 400 || (fish.caughtAt != null && fish.caughtAt.Length > 50)) return false;
            return true;
        }
        private static bool Finite(float v) { return !float.IsNaN(v) && !float.IsInfinity(v); }
    }

    public static class Economy
    {
        public static int UpgradeCost(int level) { return level >= 5 ? 0 : 200 * level * level; }
        public static int Reward(float weight) { return Math.Max(25, (int)Math.Round(weight * 30)); }
        public static bool BuyEngine(SaveData save) { return Buy(save, true); }
        public static bool BuyRod(SaveData save) { return Buy(save, false); }
        private static bool Buy(SaveData save, bool engine)
        {
            int level = engine ? save.engineLevel : save.rodLevel;
            int cost = UpgradeCost(level);
            if (cost == 0 || save.credits < cost) return false;
            save.credits -= cost;
            if (engine) save.engineLevel++; else save.rodLevel++;
            return true;
        }
    }

    public sealed class FishingSession
    {
        private readonly Random random;
        private float timer, slackTime, overloadTime, forceTimer, force;
        private int rodLevel;
        public FishingPhase Phase { get; private set; }
        public float Tension { get; private set; }
        public float Progress { get; private set; }
        public float Weight { get; private set; }
        public float Length { get; private set; }
        public string Species { get; private set; }
        public float FightTime { get; private set; }
        public FishingSession(int seed) { random = new Random(seed); Reset(); }
        public void Reset() { Phase = FishingPhase.Idle; Tension = .45f; Progress = 0; FightTime = 0; }
        public void Cast(int level)
        {
            if (Phase != FishingPhase.Idle && Phase != FishingPhase.Landed && Phase != FishingPhase.Escaped) return;
            rodLevel = Math.Max(1, Math.Min(5, level)); timer = 2f + (float)random.NextDouble() * 4f;
            Species = new[] { "Yellowfin Tuna", "Mahi-Mahi", "Albacore" }[random.Next(3)];
            Weight = 4f + (float)random.NextDouble() * 18f; Length = 45f + Weight * 3.6f;
            Tension = .45f; Progress = 0; FightTime = 0; slackTime = overloadTime = forceTimer = 0;
            Phase = FishingPhase.Waiting;
        }
        public bool Hook()
        {
            if (Phase != FishingPhase.Bite) return false;
            Phase = FishingPhase.Fighting; forceTimer = 0; return true;
        }
        public void Release() { if (Phase == FishingPhase.Waiting || Phase == FishingPhase.Bite || Phase == FishingPhase.Fighting) Phase = FishingPhase.Escaped; }
        public void Tick(float dt, bool reel, bool pull)
        {
            if (dt <= 0 || float.IsNaN(dt) || float.IsInfinity(dt)) return;
            dt = Math.Min(dt, .1f);
            if (Phase == FishingPhase.Waiting)
            {
                timer -= dt; if (timer <= 0) { Phase = FishingPhase.Bite; timer = 2.5f; }
                return;
            }
            if (Phase == FishingPhase.Bite) { timer -= dt; if (timer <= 0) Phase = FishingPhase.Escaped; return; }
            if (Phase != FishingPhase.Fighting) return;
            FightTime += dt; forceTimer -= dt;
            if (forceTimer <= 0) { force = .04f + (float)random.NextDouble() * .18f; forceTimer = .7f + (float)random.NextDouble() * 1.1f; }
            Tension = Clamp(Tension + dt * (force + (reel ? .23f : -.27f) + (pull ? .16f : 0f)));
            bool safe = Tension >= .18f && Tension <= .82f;
            Progress = Clamp(Progress + dt * (reel && safe ? .065f + .007f * rodLevel : -.008f));
            slackTime = Tension < .06f ? slackTime + dt : 0;
            overloadTime = Tension > .96f ? overloadTime + dt : 0;
            if (slackTime > 2f || overloadTime > .65f) Phase = FishingPhase.Escaped;
            else if (Progress >= 1) Phase = FishingPhase.Landed;
        }
        private static float Clamp(float v) { return Math.Max(0, Math.Min(1, v)); }
    }
}
