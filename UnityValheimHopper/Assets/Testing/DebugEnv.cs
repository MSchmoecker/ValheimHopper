using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugEnv : MonoBehaviour {
    private void Awake() {
        ZNet.SetServer(true, false, false, "Server Name", "Test", GetDevWorld());
    }

    public static World GetDevWorld() {
        World devWorld1 = World.LoadWorld("DevWorld", FileHelpers.FileSource.Local);
        if (!devWorld1.m_loadError && !devWorld1.m_versionError)
            return devWorld1;
        World devWorld2 = new World("DevWorld", "DevWorldSeed");
        devWorld2.SaveWorldMetaData();
        return devWorld2;
    }
}
