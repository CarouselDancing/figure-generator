using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.PhysicsModule;

namespace Carousel{
    
namespace FigureGenerator{

[System.Serializable]
public class HumanoidMusclePower
{
    public HumanBodyBones Muscle;
    public Vector3 PowerVector;

    public RagDoll003.MusclePower MapToTransform(Animator anim)
    {
        return new RagDoll003.MusclePower() { Muscle = anim.GetBoneTransform(Muscle).name, PowerVector = PowerVector };
    }
}

[Serializable]
public class MirrorSettings
{

    public Vector3 mirrorVector;
    public Vector3 mirrorRootOffset;

    public List<FigureGeneratorSettings.RefBodyMapping> jointMap;
    public RuntimeMirroring.MirrorMode mode;
    public bool groundFeet = false;
    public string footTipName;
    public RuntimeMirroring.TranslationMode translationMode;
};
public class FigureGeneratorSettings : MonoBehaviour
{
    [Serializable]
    public class RefBodyMapping
    {
        public string name;
        public string refName;
    }
    public string rootName;
    public string headName;
    [Range(0.02f, 2f)]
    public float width;
    public Material mat;
    public Material tansluscentMat;
    public GameObject leftFoot;
    public GameObject rightFoot;
    public GameObject headPrefab;
    public GameObject reference;
    public float footOffset;
    public float lengthScaleFactor = 0.80f;
    public float headOffset = 0;
    [SerializeField]
    public List<RefBodyMapping> referenceBodies;
    public bool createColliderAsChild = false;
    public List<string> endEffectors;
    public bool alignAnchorRotation = true;
    public List<RagDoll003.MusclePower> MusclePowers;
    public List<HumanoidMusclePower> HumanoidMusclePowers;
    public List<RagDoll003.IgnoreStruct> ignoreList;
    public float MotorScale = 1f;
    public float Stiffness = 100f;
    public float Damping = 100f;
    public string modelLayer;
    public string referenceLayer;
    public int figureType =0;

    public bool disableLimits;
   // public PhysicsMaterial actorMaterial;
}


}
}