using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixPose : MonoBehaviour
{
    [Serializable]
    public class LimbDesc
    {
        public Transform t;
        public Vector3 up;
        public Vector3 side;
        public Vector3 desiredUp;
        public Vector3 desiredSide;

    }
    public List<LimbDesc> limbRoots;


    public void OrientLimbs()
    {
        foreach(var l in limbRoots)
        {
            orientJoint(l);
        }
    }

    public void orientJoint(LimbDesc l)
    {
        

        var rotation1 = Quaternion.FromToRotation(l.up, l.desiredUp).normalized;
        var dstSide = rotation1 * l.side;
        var rotation2 = Quaternion.FromToRotation(dstSide, l.desiredSide).normalized;
        var rotation = rotation2 * rotation1;
        l.t.localRotation = Quaternion.Inverse(l.t.parent.rotation) * rotation.normalized;
    }


}
