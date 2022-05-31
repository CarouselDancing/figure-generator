using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ArticulationBodyFigureGenerator : GeneratorBase
{
    
    public bool alignAnchorRotation = true;

    public bool RootImmovable = false;
    public HumanBodyBones torsoReference = HumanBodyBones.UpperChest;
    int numActionDims;
    public bool disableLimits;
    public int solverIterations = 10;//255;//255 is used to make stabilizer work

    override public void Generate()
    {
        if (generated) return;
        generated = true;
        anim = GetComponent<Animator>();
        CreateReferenceTransformMap();
        CreatePhyscisBody(root, null, false);
        SetSolverIterations();
        if (DestroyWhenDone) DestroyImmediate(this);
    }

    public void SetSolverIterations(){
        
        foreach (var body in GetComponentsInChildren<ArticulationBody>())
        {
            body.solverIterations = solverIterations;
            body.solverVelocityIterations = solverIterations;
        }
    }


    protected override void CreateHipsBody(Transform node)
    {
        ArticulationBody ab = node.gameObject.AddComponent<ArticulationBody>();
        ab.immovable = RootImmovable;

        ab.jointType = ArticulationJointType.SphericalJoint;
        var hip = anim.GetBoneTransform(HumanBodyBones.Hips);
        var ru = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        var lu = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        Vector3 delta = ru.position - lu.position;
        ArticulationBody referenceBody = GetReferenceBody(hip);

        GameObject capsuleObject = null;
        if (createColliderAsChild)
        {
            capsuleObject = CreateChildObject(node, delta, Vector3.zero, Quaternion.Euler(0, 0, 90),2*width, lengthScaleFactor, referenceBody);
        }
        else
        {
            capsuleObject = node.gameObject;
            GeneratorUtils.CreateOffsetCapsuleCollider(node, delta, Vector3.zero, Quaternion.Euler(0, 0, 90), 2*width, lengthScaleFactor);
        }

        capsuleObject.GetComponent<MeshRenderer>().material = mat;
        var capsule = capsuleObject.AddComponent<CapsuleCollider>();
        if (!createColliderAsChild) capsule.direction = 0;
        capsule.radius = 2 * width;
        capsule.height = delta.magnitude * lengthScaleFactor * 1.5f;
        if (referenceBody != null) copyReference(referenceBody, ab, !createColliderAsChild);
    }


    protected override void CreateHead(Transform node, Transform parent)
    {
        ArticulationBody ab = parent.gameObject.AddComponent<ArticulationBody>();
        ab.jointType = ArticulationJointType.FixedJoint;

        var o = GameObject.Instantiate(headPrefab);
        var colliderJoint = o.GetComponent<ArticulationBody>();
        colliderJoint.jointType = ArticulationJointType.FixedJoint;
        Vector3 pos = node.position;
        pos.y += headOffset;
        o.transform.position = pos;
        o.transform.parent = parent;
        Debug.Log("head");
    }


    protected override void CreatePrefabBodyV1(Transform node, Transform parent, GameObject prefab, Vector3 offset)
    {
        ArticulationBody ab = parent.gameObject.AddComponent<ArticulationBody>();
        ab.jointType = ArticulationJointType.SphericalJoint;
        numActionDims += 3;
        ArticulationBody referenceBody = GetReferenceBody(parent);
        if (referenceBody != null)
        {
            copyReference(referenceBody, ab, false, false);
        }
        //ArticulationBody a = parent.GetComponent<ArticulationBody>();
        var o = GameObject.Instantiate(prefab);
        var colliderJoint = o.GetComponent<ArticulationBody>();
        //copyReference(colliderJoint, a);
        colliderJoint.jointType = ArticulationJointType.FixedJoint;

        o.transform.position = parent.position + offset;
        o.transform.rotation *= root.rotation;
        o.transform.parent = parent;
    }

    protected override void CreatePrefabBodyV2(Transform node, Transform parent, GameObject prefab, Vector3 offset)
    {

        ArticulationBody ab = parent.gameObject.AddComponent<ArticulationBody>();
        ab.jointType = ArticulationJointType.SphericalJoint;
        numActionDims += 3;
        ArticulationBody referenceBody = GetReferenceBody(parent);
        if (referenceBody != null)
        {
            copyReference(referenceBody, ab, false, false);
        }


        var o = GameObject.Instantiate(prefab);
        o.transform.position = parent.position + offset;
        o.transform.rotation *= root.rotation;
        while (o.transform.childCount > 0)
        {
            o.transform.GetChild(0).parent = parent;
        }
        float mass = ab.mass;
        DestroyImmediate(o);
    }



    protected override void CreateUpperBody(Transform node, Transform parent, float scale)
    {

        var left = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
        var right = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        Vector3 widthDelta = left.position - right.position;


        var chest = anim.GetBoneTransform(HumanBodyBones.Chest);
        var uc = anim.GetBoneTransform(HumanBodyBones.UpperChest);
        var neck = anim.GetBoneTransform(HumanBodyBones.Neck);
        Vector3 offset1 = uc.position - chest.position;
        Vector3 offset2 = neck.position - uc.position;

        //lower chest 
        ArticulationBody sphereJoint = chest.gameObject.AddComponent<ArticulationBody>();
        sphereJoint.jointType = ArticulationJointType.SphericalJoint;
        numActionDims += 3;
        ArticulationBody referenceBody = GetReferenceBody(parent);
        if (referenceBody != null)
        {
            copyReference(referenceBody, sphereJoint, !createColliderAsChild);
        }


        var rotation1 = Quaternion.Euler(0, 0, 90);

        GameObject capsuleObject1 = null;
        if (createColliderAsChild)
        {
            capsuleObject1 = CreateChildObject(chest, widthDelta, offset1 / 2, rotation1, scale*width, lengthScaleFactor * 0.75f, referenceBody);

        }
        else
        {
            offset1.x = offset1.magnitude / 2;
            offset1.y = 0;
            offset1.z = 0;
            capsuleObject1 = chest.gameObject;
            GeneratorUtils.CreateOffsetCapsuleCollider(chest, widthDelta, offset1, rotation1, scale*width, lengthScaleFactor * 0.75f);
        }
        capsuleObject1.GetComponent<MeshRenderer>().material = mat;
        var capsule = capsuleObject1.AddComponent<CapsuleCollider>();
        if (!createColliderAsChild) capsule.direction = 0;
        capsule.radius = scale * width;
        capsule.height = widthDelta.magnitude * lengthScaleFactor;


        //upper chest
        ArticulationBody sphereJoint2 = uc.gameObject.AddComponent<ArticulationBody>();
        //ArticulationBody referenceBody2 = GetReferenceBody(parent);//TODO change this
        ArticulationBody referenceBody2 = GetReferenceBody(torsoReference);
        if (referenceBody2 != null)
        {
            copyReference(referenceBody2, sphereJoint2, !createColliderAsChild);
        }
        sphereJoint2.jointType = ArticulationJointType.FixedJoint;



        var rotation2 = Quaternion.Euler(0, 0, 90);
        GameObject capsuleObject2 = null;
        if (createColliderAsChild)
        {
            capsuleObject2 = CreateChildObject(uc, widthDelta, offset2 / 2, rotation2, scale*width, lengthScaleFactor * 1.25f, referenceBody2);

        }
        else
        {
            offset2.x = offset2.magnitude / 2;
            offset2.y = 0;
            offset2.z = 0;
            capsuleObject2 = uc.gameObject;
            GeneratorUtils.CreateOffsetCapsuleCollider(uc, widthDelta, offset2, rotation2, scale*width, lengthScaleFactor * 0.8f);
        }

        capsuleObject2.GetComponent<MeshRenderer>().material = mat;
        var capsule2 = uc.gameObject.AddComponent<CapsuleCollider>();
        if (!createColliderAsChild) capsule2.direction = 0;
        capsule2.radius = scale * width;
        capsule2.height = widthDelta.magnitude * lengthScaleFactor;
    }

    ArticulationBody GetReferenceBody(Transform node)
    {
        ArticulationBody referenceBody = null;
        if (node != null && bones.ContainsKey(node) && refBones.ContainsKey(bones[node]))
        {
            referenceBody = refBones[bones[node]].GetComponent<ArticulationBody>();
        }
        return referenceBody;
    }
    ArticulationBody GetReferenceBody(HumanBodyBones bone)
    {      //return anim.GetBoneTransform(bone).GetComponent<ArticulationBody>();
        ArticulationBody referenceBody = null;
        if ( refBones.ContainsKey(bone) ){
            referenceBody = refBones[bone].GetComponent<ArticulationBody>();
        }
        return referenceBody;
    }

    protected override  void CreateCapsuleFromParent(Transform node, Transform parent, float scale, bool rotate)
    {
        ArticulationBody referenceBody = null;
        ArticulationBody ab = parent.gameObject.AddComponent<ArticulationBody>();
        if (ab == null) return;
        if (parent.name.EndsWith("Hand"))
        {
            ab.jointType = ArticulationJointType.FixedJoint;
        }
        else
        {
            ab.jointType = ArticulationJointType.SphericalJoint;
            referenceBody = GetReferenceBody(parent);

            if (referenceBody != null)
            {
                copyReference(referenceBody, ab, !createColliderAsChild, true);
            }
            numActionDims += 3;
        }
        Vector3 delta = node.position - parent.position;
        var rotation = Quaternion.identity;
        if (rotate)
        {
            rotation = Quaternion.Euler(0, 0, 90);
        }
        var offset = delta / 2;
        GameObject capsuleObject = null;
        if (createColliderAsChild)
        {

            capsuleObject = CreateChildObject(parent, delta, offset, rotation, scale*width, lengthScaleFactor, referenceBody);
        }
        else
        {
            
            offset = Quaternion.Inverse(parent.rotation)* Quaternion.Inverse(rotation) * offset;
         
            capsuleObject = parent.gameObject;
            GeneratorUtils.CreateOffsetCapsuleCollider(parent, delta, offset, rotation, scale*width, lengthScaleFactor);
        }

        capsuleObject.GetComponent<MeshRenderer>().material = mat;
        var capsule = capsuleObject.AddComponent<CapsuleCollider>();
        if (!createColliderAsChild && rotate)
        {
            capsule.direction = 0;
            capsule.height = delta.magnitude * lengthScaleFactor * 1.5f;
            capsule.radius = scale * width;

        }

    }

      
    GameObject CreateChildObject(Transform parent, Vector3 delta, Vector3 offset, Quaternion rotation, float radius, float heightScale, ArticulationBody referenceBody = null)
    {
        var capsuleObject = new GameObject();
        capsuleObject.name = parent.name + "Capsule";
        ProceduralCapsule c = capsuleObject.AddComponent<ProceduralCapsule>();
        c.height = delta.magnitude * heightScale;
        c.radius = radius;
        Debug.Log(parent.name + " " + offset.ToString());
        c.CreateMesh();
        capsuleObject.transform.position = parent.position + offset;
        capsuleObject.transform.localRotation = rotation;
        capsuleObject.transform.parent = parent;
        ArticulationBody ab2 = capsuleObject.AddComponent<ArticulationBody>();
        ab2.jointType = ArticulationJointType.FixedJoint;
        if (referenceBody != null) ab2.mass = referenceBody.mass;
        return capsuleObject;
    }


    void copyReference(ArticulationBody src, ArticulationBody dst, bool copyMass = true, bool isArm=false)
    {

        if (alignAnchorRotation)
        {
            dst.anchorRotation = Quaternion.Inverse(dst.transform.rotation) * src.anchorRotation;
        }
        else
        {
            dst.anchorRotation = src.anchorRotation;
        }
        
        var x = src.xDrive;
        var y = src.yDrive;
        var z = src.zDrive;
        if (disableLimits && isArm){
            x.upperLimit = 180;
            x.lowerLimit = -180;
            y.upperLimit = 180;
            y.lowerLimit = -180;
            z.upperLimit = 180;
            z.lowerLimit = -180;
           dst.twistLock = ArticulationDofLock.LimitedMotion;
           dst.swingYLock = ArticulationDofLock.LimitedMotion;
           dst.swingZLock = ArticulationDofLock.LimitedMotion;
           
            // dst.twistLock = src.twistLock;
            // dst.swingYLock = src.swingYLock;
            // dst.swingZLock = src.swingZLock;
        }else{
            dst.twistLock = src.twistLock;
            dst.swingYLock = src.swingYLock;
            dst.swingZLock = src.swingZLock;
        }
        dst.xDrive = x;
        dst.yDrive = y;
        dst.zDrive = z;
        dst.angularDamping = src.angularDamping;
        dst.linearDamping = src.linearDamping;
        dst.jointFriction = src.jointFriction;
        if (copyMass)
        {
            dst.mass = src.mass;
        }
    }



}
