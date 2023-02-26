using System;
using UnityEngine;

namespace ValheimHopper.Debug {
    // [DefaultExecutionOrder(-97)]
    public class UnityDebugEnv : MonoBehaviour {
        private void Awake() {
            ZNet.SetServer(true, false, false, "Server Name", "Test", GetDevWorld());
            ZNet.instance.m_players.Add(new ZNet.PlayerInfo());
            Game.instance.m_playerProfile = new PlayerProfile("Developer", FileHelpers.FileSource.Local);
            Game.instance.m_playerProfile.SetName("Odev");
            Game.instance.m_playerProfile.Load();
        }

        public static World GetDevWorld() {
            World devWorld1 = World.LoadWorld("DevWorld", FileHelpers.FileSource.Local);
            if (!devWorld1.m_loadError && !devWorld1.m_versionError)
                return devWorld1;
            World devWorld2 = new World("DevWorld", "DevWorldSeed");
            devWorld2.SaveWorldMetaData(DateTime.Now);
            return devWorld2;
        }

    }
}
