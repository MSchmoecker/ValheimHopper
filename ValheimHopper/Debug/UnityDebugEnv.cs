using System;
using UnityEngine;

namespace ValheimHopper.Debug {
    // [DefaultExecutionOrder(-97)]
    public class UnityDebugEnv : MonoBehaviour {
        private void Awake() {
            ZNet.SetServer(true, false, false, "Server Name", "Test", World.GetDevWorld());
            ZNet.instance.m_players.Add(new ZNet.PlayerInfo());
            Game.instance.m_playerProfile = new PlayerProfile("Developer", FileHelpers.FileSource.Local);
            Game.instance.m_playerProfile.SetName("Odev");
            Game.instance.m_playerProfile.Load();
        }
    }
}
