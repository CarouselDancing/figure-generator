using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace Carousel{
    
public class PhysicsPoseProvider : PoseProviderBase
{
    public ConfigurableJoint stabilizer;

    public string rootName;

    // Update is called once per frame
    void FixedUpdate()
    {
      UpdatePose();
      
    }

   virtual public void UpdatePose(){

   }

    override public void ResetToIdle(){

   }

    public void CopyStatesToRB(GameObject target)
    {
        var targets = target.GetComponentsInChildren<Rigidbody>().ToList();
        foreach (var targetRb in targets)
        {
            var stat = GetComponentsInChildren<Transform>().First(x => x.name == targetRb.name);
            targetRb.transform.position = stat.position;
            targetRb.transform.rotation = stat.rotation;
            targetRb.velocity = Vector3.zero;
            targetRb.angularVelocity = Vector3.zero;

        }
    }


    public void CopyStatesTo(GameObject target, bool resetJoints)
    {
       Debug.Log("CopyStatesTo"+name+target.name);
       gameObject.SetActive(false);
        UpdatePose();
        var targets = target.GetComponentsInChildren<ArticulationBody>().ToList();
        var root = targets.FirstOrDefault(x => x.isRoot);
        if(root == null){
            gameObject.SetActive(true);
            return;
        }
        //deactivate
       // root.gameObject.SetActive(false);
        //change game object transform
        target.transform.position = transform.position;
        target.transform.rotation = transform.rotation;

        var src = GetComponentsInChildren<Transform>().First(x => x.name == rootName);
        root.transform.localPosition = src.localPosition;
        root.transform.localRotation = src.localRotation;
        //teleport root
        root.GetComponent<ArticulationBody>().TeleportRoot(root.transform.position, root.transform.rotation);
        if (resetJoints)ResetJoints(targets);

       // root.gameObject.SetActive(true);
        if(stabilizer != null) stabilizer.connectedArticulationBody = root;

        gameObject.SetActive(true);

    }

    void ResetJoints(List<ArticulationBody> targets){

         foreach (var targetRb in targets)
        {
            var stat = GetComponentsInChildren<Transform>().First(x => x.name == targetRb.name);
            targetRb.transform.localPosition = stat.localPosition;
            targetRb.transform.localRotation= stat.localRotation;
            /*if (targetRb.isRoot)
            {
                targetRb.TeleportRoot(stat.position, stat.rotation);
            }*/
            float stiffness = 0f;
            float damping = 10000f;
            if (targetRb.twistLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = targetRb.xDrive;
                drive.stiffness = stiffness;
                drive.damping = damping;
                targetRb.xDrive = drive;
            }
            if (targetRb.swingYLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = targetRb.yDrive;
                drive.stiffness = stiffness;
                drive.damping = damping;
                targetRb.yDrive = drive;
            }
            if (targetRb.swingZLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = targetRb.zDrive;
                drive.stiffness = stiffness;
                drive.damping = damping;
                targetRb.zDrive = drive;
            }
        }
    }

    
    public void CopyStatesFrom(GameObject srcObject)
    {
        //GetComponent<Animator>().applyRootMotion = false;
       Debug.Log("CopyStatesFrom"+name+srcObject.name);
        var srcs = srcObject.GetComponentsInChildren<Transform>().ToList();
        var srcRoot = srcs.FirstOrDefault(x => x.name == rootName);
        var dstRoot = GetComponentsInChildren<Transform>().First(x => x.name == rootName);
        if(srcRoot == null){
            return;
        }
        transform.position = srcObject.transform.position;
        transform.rotation = srcObject.transform.rotation;
        dstRoot.transform.localPosition = srcRoot.localPosition;
        dstRoot.transform.localRotation = srcRoot.localRotation;
        //GetComponent<Animator>().applyRootMotion = true;
    }

}
} 
