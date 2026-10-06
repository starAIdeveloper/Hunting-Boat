using NUnit.Framework;
using UnityEngine;

namespace HuntingBoat.Tests
{
    public sealed class GameRulesTests
    {
        private FishingSession Bite(int seed = 3)
        {
            var f = new FishingSession(seed); f.Cast(1);
            for (int i = 0; i < 1000 && f.Phase == FishingPhase.Waiting; i++) f.Tick(.02f, false, false);
            Assert.AreEqual(FishingPhase.Bite, f.Phase); return f;
        }
        [Test] public void HookOutsideBiteFails() { Assert.IsFalse(new FishingSession(1).Hook()); }
        [Test] public void MissedBiteEscapes() { var f = Bite(); for (int i = 0; i < 200; i++) f.Tick(.02f, false, false); Assert.AreEqual(FishingPhase.Escaped, f.Phase); }
        [Test] public void ContinuousReelingBreaksLine() { var f = Bite(); f.Hook(); for (int i = 0; i < 3000; i++) f.Tick(.02f, true, true); Assert.AreEqual(FishingPhase.Escaped, f.Phase); }
        [Test] public void SlackLineLosesFish() { var f = Bite(); f.Hook(); for (int i = 0; i < 3000; i++) f.Tick(.02f, false, false); Assert.AreEqual(FishingPhase.Escaped, f.Phase); }
        [Test] public void BalancedReelingCanLandFish()
        {
            var f = Bite(); f.Hook(); bool reel = true;
            for (int i = 0; i < 15000 && f.Phase == FishingPhase.Fighting; i++) { if (f.Tension > .62f) reel = false; if (f.Tension < .35f) reel = true; f.Tick(.02f, reel, false); }
            Assert.AreEqual(FishingPhase.Landed, f.Phase); Assert.AreEqual(1, f.Progress);
        }
        [Test] public void CastCannotOverwriteActiveFight() { var f = Bite(); f.Hook(); string species = f.Species; f.Cast(5); Assert.AreEqual(FishingPhase.Fighting, f.Phase); Assert.AreEqual(species, f.Species); }
        [Test] public void InvalidDeltaDoesNotCorruptTension() { var f = Bite(); f.Hook(); f.Tick(float.NaN, true, true); Assert.IsFalse(float.IsNaN(f.Tension)); }
        [Test] public void UpgradeChargesExactlyOnce() { var s = new SaveData { credits = 200 }; Assert.IsTrue(Economy.BuyEngine(s)); Assert.AreEqual(0, s.credits); Assert.AreEqual(2, s.engineLevel); Assert.IsFalse(Economy.BuyEngine(s)); }
        [Test] public void UpgradeMaximumIsEnforced() { var s = new SaveData { credits = 10000, rodLevel = 5 }; Assert.IsFalse(Economy.BuyRod(s)); Assert.AreEqual(10000, s.credits); }
        [Test] public void CorruptSaveIsRejected() { var s = new SaveData { x = float.NaN }; Assert.IsFalse(s.IsValid()); s.x = 0; s.catches = null; Assert.IsFalse(s.IsValid()); }
        [Test] public void SaveRoundtripRetainsCatch()
        {
            var s = new SaveData { credits = 550, x = 30, z = 40 }; s.catches.Add(new CatchRecord { species = "Tuna", weight = 12.5f, length = 90, caughtAt = "2026-10-06T00:00:00Z" });
            var read = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(s)); Assert.IsTrue(read.IsValid()); Assert.AreEqual(12.5f, read.catches[0].weight); Assert.AreEqual(550, read.credits);
        }
        [Test] public void RewardIsNonzero() { Assert.Greater(Economy.Reward(4), 0); }
    }
}
