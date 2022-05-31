using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RigidBodyFigureGenerator : GeneratorBase
{
    

    public bool isKinematic=true;

    protected override void CreateHipsBody(Transform node)
    {
        Rigidbody rb;
        var hip = anim.GetBoneTransform(HumanBodyBones.Hips);
        var ru = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        var lu = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        Vector3 delta = ru.position - lu.position;
        ArticulationBody referenceBody = GetReferenceBody(hip);
        GameObject capsuleObject = null;
        if (createColliderAsChild)
        {
            capsuleObject = GeneratorUtils.CreateChildObject(node, delta, Vector3.zero, Quaternion.Euler(0, 0, 90), 2*width, lengthScaleFactor, null, isKinematic);
            rb = capsuleObject.GetComponent<Rigidbody>();
        }
        else
        {
            rb = node.gameObject.AddComponent<Rigidbody>();
            Debug.Log("node" + node.name);
            rb.isKinematic = isKinematic;
            capsuleObject = node.gameObject;
            GeneratorUtils.CreateOffsetCapsuleCollider(node, delta, Vector3.zero, Quaternion.Euler(0, 0, 90), 2* width, lengthScaleFactor);
        }
        capsuleObject.GetComponent<MeshRenderer>().material = mat;
        var capsule = capsuleObject.AddComponent<CapsuleCollider>();
        if(!createColliderAsChild) capsule.direction = 0;
        capsule.radius = 2 * width;
        capsule.height = delta.magnitude * lengthScaleFactor * 1.5f;
        if (referenceBody != null) copyReference(referenceBody, rb);
    }

    protected override void CreateHead(Transform node, Transform parent)
    {

        var o = GameObject.Instantiate(headPrefab);
        var ab = o.GetComponent<ArticulationBody>();
        float mass = ab.mass;
        DestroyImmediate(ab);
        var rb = o.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.isKinematic = isKinematic;
        Vector3 pos = node.position;
        pos.y += headOffset;
        o.transform.position = pos;
        o.transform.parent = parent;
        if (verbose) Debug.Log("head");
         if(!isKinematic){
            createJoint(rb,  rb.transform.parent, true);
         }
    }


    protected override void CreatePrefabBodyV1(Transform node, Transform parent, GameObject prefab, Vector3 offset)
    {
        var o = GameObject.Instantiate(prefab);
        var ab = o.GetComponent<ArticulationBody>();
        float mass = ab.mass;
        DestroyImmediate(ab);
        var rb = parent.gameObject.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.isKinematic = isKinematic;
        o.transform.position = parent.position + offset;
        o.transform.rotation *= root.rotation;
        o.transform.parent = parent;

         if(!isKinematic){
            createJoint(rb,  rb.transform.parent, false);
         }
    }


    protected override void CreatePrefabBodyV2(Transform node, Transform parent, GameObject prefab, Vector3 offset)
    {
        var o = GameObject.Instantiate(prefab);
        o.transform.position = parent.position + offset;
        o.transform.rotation *= root.rotation;
        while (o.transform.childCount > 0)
        {
            o.transform.GetChild(0).parent = parent;
        }
        //o.transform.parent = parent;
        var ab = o.GetComponent<ArticulationBody>();
        float mass = ab.mass;

        var rb = parent.gameObject.AddComponent<Rigidbody>();
        //var rb2 = o.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.isKinematic = isKinematic;
        DestroyImmediate(o);

         if(!isKinematic){
             
            createJoint(rb,  rb.transform.parent, false);
         }
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
        Rigidbody rb = null;
        ArticulationBody referenceBody = GetReferenceBody(parent);

        var rotation1 = Quaternion.Euler(0, 0, 90);
        GameObject capsuleObject1 = null;
        if (createColliderAsChild)
        {
            capsuleObject1 = GeneratorUtils.CreateChildObject(chest, widthDelta, offset1 / 2, rotation1, scale*width, lengthScaleFactor * 0.75f, null, isKinematic);
            rb = capsuleObject1.GetComponent<Rigidbody>();
        }
        else
        {
            rb = chest.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = isKinematic;
            offset1.x = offset1.magnitude / 2;
            offset1.y = 0;
            offset1.z = 0;
            capsuleObject1 = chest.gameObject;
            GeneratorUtils.CreateOffsetCapsuleCollider(chest, widthDelta, offset1, rotation1, width*scale, lengthScaleFactor * 0.75f);
        }

         if(!isKinematic){
            createJoint(rb,  rb.transform.parent, true);
         }

        if (referenceBody != null)
        {
            copyReference(referenceBody, rb);
        }
        capsuleObject1.GetComponent<MeshRenderer>().material = mat;
        var capsule = capsuleObject1.AddComponent<CapsuleCollider>();
        if (!createColliderAsChild) capsule.direction = 0;
        capsule.radius = scale * width;
        capsule.height = widthDelta.magnitude * lengthScaleFactor;


        //upper chest
        Rigidbody rb2= null;
        ArticulationBody referenceBody2 = GetReferenceBody(parent);
        
        var rotation2 = Quaternion.Euler(0, 0, 90);
        GameObject capsuleObject2 = null;
        if (createColliderAsChild)
        {
            capsuleObject2 = GeneratorUtils.CreateChildObject(uc, widthDelta, offset2 / 2, rotation2, scale*width, lengthScaleFactor * 1.25f, null, isKinematic);
            rb2 = capsuleObject2.GetComponent<Rigidbody>();
        }
        else
        {
            rb2 = uc.gameObject.AddComponent<Rigidbody>();
            rb2.isKinematic = isKinematic;
            offset2.x = offset2.magnitude / 2;
            offset2.y = 0;
            offset2.z = 0;
            capsuleObject2 = uc.gameObject;
            GeneratorUtils.CreateOffsetCapsuleCollider(uc, widthDelta, offset2, rotation2, scale*width, lengthScaleFactor *0.8f );
        }
        if (referenceBody2 != null)
        {
            copyReference(referenceBody2, rb2);
        }

         if(!isKinematic){
             createJoint(rb2, rb.transform, true);
         }
        capsuleObject2.GetComponent<MeshRenderer>().material = mat;
        var capsule2 = capsuleObject2.AddComponent<CapsuleCollider>();
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

    ConfigurableJoint createJoint(Rigidbody rb, Transform parent, bool isFixed=false){
        var parentRB = parent.GetComponent<Rigidbody>();
        while (parentRB == null && parent.parent!=null){
            parent = parent.parent;
             parentRB = parent.GetComponent<Rigidbody>();

        }
        var joint = rb.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = parentRB;
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.enablePreprocessing = true;
        if(isFixed){
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
        }else{
            var xDrive = joint.angularXDrive;
            xDrive.positionSpring = 1000;
            xDrive.positionDamper = 0;
            xDrive.maximumForce = 3.402823e+38f;
            joint.angularXDrive= xDrive;
            var yzDrive = joint.angularYZDrive;
            yzDrive.positionSpring = 1000;
            yzDrive.positionDamper = 0;
            yzDrive.maximumForce = 3.402823e+38f;
            joint.angularYZDrive= yzDrive;
        }
        return joint;
    }

    protected override void CreateCapsuleFromParent(Transform node, Transform parent, float scale, bool rotate)
    {
        ArticulationBody referenceBody = null;
        Rigidbody rb = parent.gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = isKinematic;
        
         if(!isKinematic){
             
            createJoint(rb,  rb.transform.parent, false);
         }
        if (!parent.name.EndsWith("Hand"))
        {
         
            referenceBody = GetReferenceBody(parent);
            if (referenceBody != null)
            {
                Debug.Log("copy reference " + parent.name);
                copyReference(referenceBody, rb, !createColliderAsChild);
            }
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

            capsuleObject = GeneratorUtils.CreateChildObject(parent, delta,offset, rotation, scale*width, lengthScaleFactor, referenceBody, isKinematic);
        }
        else
        {
            capsuleObject = parent.gameObject;
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



}
