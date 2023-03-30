/*
 * Created by jiadong chen
 * https://jiadong-chen.medium.com/
 */
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class AnimationTextureBakerUI : EditorWindow
{
    #region Variables

    private const string builtInShader = "chenjd/BuiltIn/AnimMapShader";
    private static GameObject targetGameObject;
    private static AnimationTextureBaker animationBaker;
    private static string animationSavePath = "Animations/Vertex";
    private static string modelName = "SubPath";
    #endregion

    #region Methods

    [MenuItem("Window/AnimMapBaker")]
    public static void ShowWindow()
    {
        GetWindow(typeof(AnimationTextureBakerUI));
        animationBaker = new AnimationTextureBaker();
    }

    private void OnGUI()
    {
        targetGameObject = (GameObject)EditorGUILayout.ObjectField(targetGameObject, typeof(GameObject), true);
        modelName = targetGameObject == null ? modelName : targetGameObject.name;
        EditorGUILayout.LabelField(string.Format($"Output Path: {Path.Combine(animationSavePath, modelName)}"));
        animationSavePath = EditorGUILayout.TextField(animationSavePath);
        modelName = EditorGUILayout.TextField(modelName);


        if (!GUILayout.Button("Bake")) return;

        if (targetGameObject == null) { EditorUtility.DisplayDialog("Error", "Please select a model with Generic Animations.", "Ok"); return; }

        if (animationBaker == null) { animationBaker = new AnimationTextureBaker(); }

        animationBaker.SetAnimationData(targetGameObject);

        List<BakedData> bakedData = animationBaker.Bake();

        if (bakedData == null) return;
        for (int i = 0; i < bakedData.Count; i++)
        {
            BakedData bakedDataInstance = bakedData[i];
            SaveBakedDataToDisk(ref bakedDataInstance);
        }
    }

    private void SaveBakedDataToDisk(ref BakedData bakedData)
    {
        SaveAsAsset(ref bakedData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static Texture2D SaveAsAsset(ref BakedData bakedData)
    {
        string animationSavePath = CreateFolder();
        Texture2D animationTexture = new Texture2D(bakedData.animationTextureWidth, bakedData.animationTextureHeight, TextureFormat.RGBAHalf, false);
        animationTexture.LoadRawTextureData(bakedData.rawAnimationTexture);
        AssetDatabase.CreateAsset(animationTexture, Path.Combine(animationSavePath, bakedData.name + ".asset"));
        return animationTexture;
    }

    private static string CreateFolder()
    {
        string outputPath = Path.Combine("Assets/" + animationSavePath, modelName);
        if (!Directory.Exists(outputPath)) Directory.CreateDirectory(Path.Combine("Assets/" + animationSavePath, modelName));
        return outputPath;
    }

    #endregion


}
