using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;



public class RagDollPDController : RagDollPDControllerBase
{


    List<Rigidbody> _mocapBodyParts;
    List<ArticulationBody> _mocapBodyPartsA;
    List<ArticulationBody> _bodyParts;
    RagDoll003 _ragDollSettings;
    List<ArticulationBody> _motors;

    public int numActionDims;

    public bool _hasLazyInitialized;
    public float[] _mocapTargets;
    

    void Start(){
    }

    void FixedUpdate()
    {
    
        
        if (!_hasLazyInitialized)
        {
            OnEpisodeBegin();
            return;
        }
        if(!active)return;
        
        var vectorAction = GetMocapTargets();
    
        int i = 0;
        foreach (var m in _motors)
        {
            if (m.isRoot)
                continue;
            Vector3 targetNormalizedRotation = Vector3.zero;

            if (m.twistLock == ArticulationDofLock.LimitedMotion)
                targetNormalizedRotation.x = vectorAction[i++];
            if (m.swingYLock == ArticulationDofLock.LimitedMotion)
                targetNormalizedRotation.y = vectorAction[i++];
            if (m.swingZLock == ArticulationDofLock.LimitedMotion)
                targetNormalizedRotation.z = vectorAction[i++];
            UpdateMotor(m, targetNormalizedRotation);
        }
    }


   public override void OnEpisodeBegin()
    {

        if (!_hasLazyInitialized && animationSrc != null)
        {
         
            _mocapBodyParts = animationSrc.GetComponentsInChildren<Rigidbody>().ToList();
            _mocapBodyPartsA = animationSrc.GetComponentsInChildren<ArticulationBody>().ToList();
    
            _bodyParts = GetComponentsInChildren<ArticulationBody>().ToList();
            _ragDollSettings = GetComponent<RagDoll003>();

            foreach (var body in GetComponentsInChildren<ArticulationBody>())
            {
                body.solverIterations = 255;
                body.solverVelocityIterations = 255;
            }

            _motors = GetComponentsInChildren<ArticulationBody>()
                .Where(x => x.jointType == ArticulationJointType.SphericalJoint)
                .Where(x => !x.isRoot)
                .Distinct()
                .ToList();
            var individualMotors = new List<float>();
            numActionDims = 0;
            foreach (var m in _motors)
            {
                if (m.twistLock == ArticulationDofLock.LimitedMotion) { 
                    individualMotors.Add(0f);
                    numActionDims++;
                }
                if (m.swingYLock == ArticulationDofLock.LimitedMotion) { 
                    individualMotors.Add(0f);
                    numActionDims++;
                }
                if (m.swingZLock == ArticulationDofLock.LimitedMotion) { 
                    individualMotors.Add(0f);
                    numActionDims++;
                }
            }
            _mocapTargets = null;
            _hasLazyInitialized = true;
            animationSrc.ResetToIdle();
            animationSrc.CopyStatesTo(this.gameObject, false);
        }

     
        OnReset?.Invoke();

#if UNITY_EDITOR
        if (DebugPauseOnReset)
        {
            UnityEditor.EditorApplication.isPaused = true;
        }
#endif	      
    }

    float[] GetMocapTargets()
    {
        if (_mocapTargets == null)
        {
            _mocapTargets = _motors
                .Where(x => !x.isRoot)
                .SelectMany(x => {
                    List<float> list = new List<float>();
                    if (x.twistLock == ArticulationDofLock.LimitedMotion)
                        list.Add(0f);
                    if (x.swingYLock == ArticulationDofLock.LimitedMotion)
                        list.Add(0f);
                    if (x.swingZLock == ArticulationDofLock.LimitedMotion)
                        list.Add(0f);
                    return list.ToArray();
                })
                .ToArray();
        }
        int i = 0;
        foreach (var joint in _motors)
        {
            if (joint.isRoot)
                continue;
            Quaternion localRot;
            Transform mocapBodyTransform;
             Vector3 targetRotationInJointSpace;
            if (_mocapBodyParts.Count > 0) {
                Rigidbody mocapBody = _mocapBodyParts.First(x => x.name == joint.name);
                mocapBodyTransform = mocapBody.transform;
                localRot = mocapBodyTransform.localRotation;
                if (mocapBodyTransform.parent.GetComponent<Rigidbody>() == null)
                {
                    localRot = mocapBodyTransform.parent.localRotation * localRot;
                }
            }
            else
            {
                ArticulationBody mocapBody = _mocapBodyPartsA.First(x => x.name == joint.name);
                mocapBodyTransform = mocapBody.transform;
                localRot = mocapBodyTransform.localRotation;
            }
           
            targetRotationInJointSpace = -(Quaternion.Inverse(joint.anchorRotation) * Quaternion.Inverse(localRot) * joint.parentAnchorRotation).eulerAngles;
            targetRotationInJointSpace = new Vector3(
                Mathf.DeltaAngle(0, targetRotationInJointSpace.x),
                Mathf.DeltaAngle(0, targetRotationInJointSpace.y),
                Mathf.DeltaAngle(0, targetRotationInJointSpace.z));
            if (joint.twistLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = joint.xDrive;
                var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                var midpoint = drive.lowerLimit + scale;
                var target = (targetRotationInJointSpace.x - midpoint) / scale;
                _mocapTargets[i] = target;
                i++;
            }
            if (joint.swingYLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = joint.yDrive;
                var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                var midpoint = drive.lowerLimit + scale;
                var target = (targetRotationInJointSpace.y - midpoint) / scale;
                _mocapTargets[i] = target;
                i++;
            }
            if (joint.swingZLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = joint.zDrive;
                var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                var midpoint = drive.lowerLimit + scale;
                var target = (targetRotationInJointSpace.z - midpoint) / scale;
                _mocapTargets[i] = target;
                i++;
            }
        }
        return _mocapTargets;
    }  
    
    void UpdateMotor(ArticulationBody joint, Vector3 targetNormalizedRotation)
    {
        Vector3 power = _ragDollSettings.MusclePowers.First(x => x.Muscle == joint.name).PowerVector;
        power *= _ragDollSettings.Stiffness;
        float damping = _ragDollSettings.Damping;

        if (joint.twistLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.xDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            var target = midpoint + (targetNormalizedRotation.x * scale);
            drive.target = target;
            drive.stiffness = power.x;
            drive.damping = damping;
            joint.xDrive = drive;
        }

        if (joint.swingYLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.yDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            var target = midpoint + (targetNormalizedRotation.y * scale);
            drive.target = target;
            drive.stiffness = power.y;
            drive.damping = damping;
            joint.yDrive = drive;
        }

        if (joint.swingZLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.zDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            var target = midpoint + (targetNormalizedRotation.z * scale);
            drive.target = target;
            drive.stiffness = power.z;
            drive.damping = damping;
            joint.zDrive = drive;
        }
    }

    void DeactivateMotor(ArticulationBody joint)
    {
        Vector3 power = _ragDollSettings.MusclePowers.First(x => x.Muscle == joint.name).PowerVector;
        power *= _ragDollSettings.Stiffness;
        float damping = _ragDollSettings.Damping;

        if (joint.twistLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.xDrive;
            drive.stiffness =0;
            drive.damping = damping;
            joint.xDrive = drive;
        }

        if (joint.swingYLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.yDrive;
            drive.stiffness = 0;
            drive.damping = damping;
            joint.yDrive = drive;
        }

        if (joint.swingZLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.zDrive;
            drive.stiffness = 0;
            drive.damping = damping;
            joint.zDrive = drive;
        }
    }

    public void Deactivate(){
        active = false;
        foreach (var m in _motors)
        {
            if (m.isRoot)
                continue;
            Vector3 targetNormalizedRotation = Vector3.zero;
            DeactivateMotor(m);
        }
    }

     public void Activate(){
        active = true;
    }


}
