/*
 * Created by jiadong chen
 * https://jiadong-chen.medium.com/
 * 用来烘焙动作贴图。烘焙对象使用Animation组件，并且在导入时设置Rig为Legacy
 */
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct AnimationData
{
    #region Variables
    public int vertexCount;
    public int animationTextureWidth;
    public readonly List<AnimationState> animationClips;
    public string animationSetName;

    public  Animation animation;
    public SkinnedMeshRenderer skinnedMeshRenderer;
    #endregion

    public AnimationData(Animation animation, SkinnedMeshRenderer skinnedMeshRenderer, string gameObjectName)
    {
        vertexCount = skinnedMeshRenderer.sharedMesh.vertexCount;
        animationTextureWidth = Mathf.NextPowerOfTwo(vertexCount);
        animationClips = new List<AnimationState>(animation.Cast<AnimationState>());
        this.animation = animation;
        this.skinnedMeshRenderer = skinnedMeshRenderer;
        animationSetName = gameObjectName;
    }

    #region Methods

    public void PlayAnimation(string animationName)
    {
        animation.Play(animationName);
    }

    public void SampleAnimationAndBakeMesh(ref Mesh mesh)
    {
        SampleAnimation();
        BakeMesh(ref mesh);
    }

    private void SampleAnimation()
    {
        if (animation == null) { Debug.LogError("Animation is null, please select a model with generic animations and try again."); return; }
        animation.Sample();
    }

    private void BakeMesh(ref Mesh mesh)
    {
        if (skinnedMeshRenderer == null) { Debug.LogError("Skinned Mesh Renderer is null, please select a Skinned model with generic animations and try again."); return; }
        skinnedMeshRenderer.BakeMesh(mesh);
    }
    #endregion
}

public struct BakedData
{
    #region Variables
    public string name;
    public float animationDuration;
    public byte[] rawAnimationTexture;
    public int animationTextureWidth;
    public int animationTextureHeight;

    #endregion

    public BakedData(string name, float animationDuration, Texture2D animationTexture)
    {
        this.name = name;
        this.animationDuration = animationDuration;
        animationTextureHeight = animationTexture.height;
        animationTextureWidth = animationTexture.width;
        rawAnimationTexture = animationTexture.GetRawTextureData();
    }
}

public class AnimationTextureBaker
{
    #region Variables
    private AnimationData? animationData = null;
    private Mesh bakedMesh;
    private readonly List<BakedData> bakedDataList = new List<BakedData>();
    #endregion

    #region Methods
    public void SetAnimationData(GameObject gameObject)
    {
        if(gameObject == null) { Debug.LogError("No GameObject selected!"); return; }

        Animation animation = gameObject.GetComponent<Animation>();
        SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();

        if(animation == null || skinnedMeshRenderer == null)
        { Debug.LogError("Both animation and Skinned Mesh Renderer components should be present in the object!"); return; }
        bakedMesh = new Mesh();
        animationData = new AnimationData(animation, skinnedMeshRenderer, gameObject.name);
    }

    public List<BakedData> Bake()
    {
        if(animationData == null) { Debug.LogError("No Baked Data found"); return bakedDataList; }

        for (int i = 0; i < animationData.Value.animationClips.Count; i++)
        {
            AnimationState animationState = animationData.Value.animationClips[i];
            if (!animationState.clip.legacy) { Debug.LogError(string.Format($"Animation clips must be legacy, change the Rig of {animationState.clip.name} to Generic.")); continue; }
            BakeAnimationClip(animationState);
        }

        return bakedDataList;
    }

    private void BakeAnimationClip(AnimationState currentAnimation)
    {
        int currentFrame = 0;
        float sampleTime = 0;
        float timePerFrame = 0;

        currentFrame = Mathf.ClosestPowerOfTwo((int)(currentAnimation.clip.frameRate * currentAnimation.length));
        timePerFrame = currentAnimation.length / currentFrame; ;

        Texture2D animationTexture = new Texture2D(animationData.Value.animationTextureWidth, currentFrame, TextureFormat.RGBAHalf, true);
        animationTexture.name = string.Format($"{animationData.Value.animationSetName}_{currentAnimation.name}.animMap");
        animationTexture.name = animationTexture.name.Replace("|", "_");
        animationData.Value.PlayAnimation(currentAnimation.name);

        for (var i = 0; i < currentFrame; i++)
        {
            currentAnimation.time = sampleTime;

            animationData.Value.SampleAnimationAndBakeMesh(ref bakedMesh);

            for(int j = 0; j < bakedMesh.vertexCount; j++)
            {
                Vector3 vertex = bakedMesh.vertices[j];
                animationTexture.SetPixel(j, i, new Color(vertex.x, vertex.y, vertex.z));
            }

            sampleTime += timePerFrame;
        }
        animationTexture.Apply();

        bakedDataList.Add(new BakedData(animationTexture.name, currentAnimation.clip.length, animationTexture));
    }
    #endregion
}
