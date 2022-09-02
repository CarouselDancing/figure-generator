using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Carousel{
    
namespace FigureGenerator{

    

public abstract class GeneratorBase : MonoBehaviour
{
    [Serializable]
    public class RefBodyMapping
    {
        public string name;
        public string refName;
    }
    public Transform root;
    [Range(0.02f, 2f)]
    public float width;
    public Material mat;
    public GameObject leftFoot;
    public GameObject rightFoot;
    public GameObject headPrefab;
    public GameObject reference;
    public float footOffset;
    public float lengthScaleFactor = 0.80f;
    public float headOffset = 0;
    [SerializeField]
    public List<RefBodyMapping> referenceBodies;
    protected Animator anim;
    [SerializeField]
    public Dictionary<Transform, HumanBodyBones> bones;
    [SerializeField]
    public Dictionary<HumanBodyBones, Transform> refBones;
    public bool createColliderAsChild = false;
    public List<string> endEffectors;
    public bool generated = false;
    public bool DestroyWhenDone = true;
    public bool verbose = false;
    public int version = 1;
    public int figureType =0;
    public List<string> IgnoreList;
    public bool disableLimits;
    public int solverIterations = 255;//255 is used to make stabilizer work
    public float handRadius = 0.05f;
    public bool useHandBalls = true;
    public Vector3 handCenterOffset = new Vector3(0,0,0);

    // Start is called before the first frame update
    void Start()
    {

        if (!generated)
        {
            Debug.Log("generate figure from start" + name + generated.ToString());
            Generate();
        }
    }

    virtual public void Generate()
    {
        if (generated) return;
        generated = true;
        anim = GetComponent<Animator>();
        CreateReferenceTransformMap();
        CreatePhyscisBody(root, null, false);
        if (DestroyWhenDone) DestroyImmediate(this);
    }

    protected void CreateReferenceTransformMap()
    {
        bones = new Dictionary<Transform, HumanBodyBones>();
        refBones = new Dictionary<HumanBodyBones, Transform>();
        var refRoot = reference.GetComponent<Transform>();
        foreach (HumanBodyBones v in HumanBodyBones.GetValues(typeof(HumanBodyBones)))
        {
            if (v == HumanBodyBones.LastBone) continue;
            var t = anim.GetBoneTransform(v);
            if (t != null)
            {
                bones[t] = v;
                var mapping = referenceBodies
                .Where(x => x.name == t.name)
                .FirstOrDefault();
                if (mapping != null)
                {
                    Transform refT;
                    if (verbose) Debug.Log("found " + mapping.refName);
                    if (GeneratorUtils.FindChild(refRoot, mapping.refName, out refT))
                    {
                        if (verbose) Debug.Log("add " + refT.name);
                        refBones[v] = refT;
                    }
                }
            }
        }
    }

    public void CreatePhyscisBody(Transform node, Transform parent, bool ignore)
    {
        if(node == null)return;
        if(IgnoreList.Count > 0 && IgnoreList.Contains(node.name))return;
        bool rotate = false;
        float scale = 1;
        int nChildren = node.childCount;
        if (nChildren == 0) // && !isRightFoot && !isLeftFoot && !isHead
        {
            if(verbose)Debug.Log("leave" + node.name);
            ignore = true;
        }
        bool ignoreChild = false;
        if (bones.ContainsKey(node))
        {
            switch (bones[node])
            {
                case HumanBodyBones.Hips:
                    ignoreChild = true;
                    CreateHipsBody(node);
                    ignore = true;
                    break;
                case HumanBodyBones.LeftToes:
                    if (version == 2)
                        CreatePrefabBodyV2(node, parent, leftFoot, new Vector3(0, footOffset, 0));
                    else
                        CreatePrefabBodyV1(node, parent, leftFoot, new Vector3(0, footOffset, 0));
                    ignore = true;
                    break;
                case HumanBodyBones.RightToes:
                    if (version == 2)
                        CreatePrefabBodyV2(node, parent, rightFoot, new Vector3(0, footOffset, 0));
                    else
                        CreatePrefabBodyV1(node, parent, rightFoot, new Vector3(0, footOffset, 0));
                    ignore = true;
                    break;
                case HumanBodyBones.Spine:
                    scale = 1.5f;
                    rotate = false;
                    break;
                case HumanBodyBones.UpperChest:
                    ignoreChild = true;
                    scale = 1.5f;
                    CreateUpperBody(node, parent, scale);
                    ignore = true;
                    break;
                case HumanBodyBones.Chest:
                    scale = 1.5f;
                    rotate = false;
                    break;
                case HumanBodyBones.Head:
                    CreateHead(node, parent);
                    ignore = true;
                    break;
                case HumanBodyBones.Neck:
                    ignoreChild = true;
                    break;
                case HumanBodyBones.RightLowerArm:
                    rotate = createColliderAsChild || figureType == 1;
                    break;
                case HumanBodyBones.RightHand:
                    rotate = createColliderAsChild || figureType == 1;
                    break;
                case HumanBodyBones.LeftLowerArm:
                    rotate =createColliderAsChild || figureType == 1;
                    break;
                case HumanBodyBones.LeftHand:
                    rotate = createColliderAsChild || figureType == 1;
                    break;
            }

        }

        if (parent != null)
        {
            ignore |= parent.name.EndsWith("HipJoint");
            ignore |= parent.name.EndsWith("Shoulder");
        }

        if (parent != null && !ignore)
        {
            CreateCapsuleFromParent(node, parent, scale, rotate);
        }
        if (endEffectors.Contains(node.name))
        {
            
            if(useHandBalls)CreateHandBall(node, parent, handRadius);
            return;
        }
        for (int i = 0; i < nChildren; i++)
        {
            var child = node.GetChild(i);
            CreatePhyscisBody(child, node, ignoreChild);
        }
    }

   protected abstract void CreateHipsBody(Transform node);


    protected abstract void CreateHead(Transform node, Transform parent);
    protected abstract void CreateHandBall(Transform node, Transform parent, float radisu);

    protected abstract void CreateUpperBody(Transform node, Transform parent, float scale);

    protected abstract void CreateCapsuleFromParent(Transform node, Transform parent, float scale, bool rotate);

    protected abstract void CreatePrefabBodyV1(Transform node, Transform parent, GameObject prefab, Vector3 offset);
    protected abstract void CreatePrefabBodyV2(Transform node, Transform parent, GameObject prefab, Vector3 offset);

    protected void copyReference(ArticulationBody src, Rigidbody dst, bool copyMass = true)
    {
        if (copyMass)
        {
            dst.mass = src.mass;
        }
    }
}

}

}