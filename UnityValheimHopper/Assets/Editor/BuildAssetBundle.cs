using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundle : MonoBehaviour {
    [MenuItem("Assets/Build AssetBundles")]
    private static void BuildAllAssetBundles() {
        const string assetBundleOutputPath = "AssetBundles/StandaloneWindows";
        string hopperAssetBundlePath = Path.Combine(assetBundleOutputPath, "ValheimHopper_AssetBundle");

        BuildPipeline.BuildAssetBundles(assetBundleOutputPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        FileUtil.ReplaceFile(hopperAssetBundlePath, "../ValheimHopper/ValheimHopper_AssetBundle");
    }

    [MenuItem("Assets/Create Procedural Mesh")]
    static void CreateMesh() {
        string filePath = EditorUtility.SaveFilePanelInProject("Create Procedural Mesh", "Mesh", "asset", "");
        if (filePath == "") return;
        AssetDatabase.CreateAsset(new Mesh(), filePath);
    }
}
