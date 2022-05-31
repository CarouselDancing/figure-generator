﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RuntimeMirroring : PhysicsPoseProvider
{
    public enum MirrorMode
    {
        GLOBAL = 1,
        LOCAL = 2
    }

    public enum TranslationMode
    {
        ABSOLUTE = 1,
        RELATIVE = 2,
        EXTERNAL = 3,
        IGNORE = 4
    }
    [Serializable]
    public class JointMap
    {
        public string src;
        public string dst;
        public Transform srcT;
        public Transform dstT;

    }
    public Transform src;
    private List<Transform> _transforms;
    private List<Transform> _srcTransforms;
    public List<JointMap> jointMap;

    JointMap rootMap;
    public Vector3 mirrorVector;
    public Matrix4x4 mirrorMatrix;
    public Vector3 relativeRootOffset;
    public MirrorMode mode = MirrorMode.GLOBAL;
    public TranslationMode translationMode = TranslationMode.RELATIVE;
    public bool applyRootPosition = true;
    public Vector3 lastPosition;
    public bool rootPosSet = false;
    public bool groundFeet = false;
    public Transform footTip;
    public bool active = true;

    public float footTipOffset =0;
    public Vector3 externalPosition;
    public bool initialized;
  
  
    public void SetTransforms()
    {
        mirrorMatrix.m00 = mirrorVector.x;
        mirrorMatrix.m11 = mirrorVector.y;
        mirrorMatrix.m22 = mirrorVector.z;
        mirrorMatrix.m33 = 1;
        _transforms = GetComponentsInChildren<Transform>().ToList();
        _srcTransforms = src.GetComponentsInChildren<Transform>().ToList();

        for (int i = 0; i < jointMap.Count; i++)
        {
            if (jointMap[i].src == "ignore") continue;

            var srcTs = _srcTransforms.FindAll(x => x.name == jointMap[i].dst);
            if(srcTs.Count > 0){
                jointMap[i].srcT =srcTs[0];
            }else{
                Debug.Log("error: could not find"+ jointMap[i].dst);
            }
            Transform dstT;

            dstT = _transforms.First(x => x.name == jointMap[i].src);
            /*
            if (mode == MirrorMode.GLOBAL) { 
            }
            else { 
                dstT = _transforms.First(x => x.name == jointMap[i].dst);
            }*/
            jointMap[i].dstT = dstT;
            if (jointMap[i].src == rootName)
            {
                rootMap = jointMap[i];
            }
            
        }
        initialized = true;

    }



    override public void UpdatePose(){
        if(!initialized && active)SetTransforms();
        MirrorPose();
    }

    public void MirrorPose()
    { 
        if(!active) return;
        if (mode == MirrorMode.GLOBAL)
        {
            if(translationMode == TranslationMode.ABSOLUTE){
                MirrorPoseGlobal();
           }else if(translationMode == TranslationMode.EXTERNAL) {
                MirrorPoseGlobalWithExternalTranslation();
            }else{
                MirrorPoseGlobalWithRelativeTranslation();
            }
        }
        else
        {
            MirrorPoseLocal();
        }
    }
    public void MirrorPoseGlobal()
    {
        if (rootMap == null) return;
        var srcRootRotation = rootMap.srcT.rotation;
        var dstRootRotation = rootMap.dstT.rotation;
        srcRootRotation = Quaternion.Euler(0, srcRootRotation.eulerAngles.y, 0);
        var worldToLocal = Quaternion.Inverse(srcRootRotation);
        var _relativeRootOffset = srcRootRotation * relativeRootOffset;
        //_relativeRootOffset.y = 0;

        foreach (var m in jointMap)
        {
            if (m.src == "ignore") continue;

            var srcT = m.srcT;
            var dstT = m.dstT;
            var relativePos = worldToLocal * (srcT.position - rootMap.srcT.position);
            var matrix = mirrorMatrix * Matrix4x4.TRS(relativePos, worldToLocal * srcT.rotation, Vector3.one) * mirrorMatrix;

           
            dstT.rotation = srcRootRotation * matrix.rotation;
            dstT.localRotation *= Quaternion.Euler(0, 180, 0);

             Vector3 delta = matrix.GetColumn(3);
            dstT.position = _relativeRootOffset + rootMap.srcT.position + srcRootRotation * delta;
        }
        ApplyFootGrounding();
    }

    public void MirrorPoseGlobalWithRelativeTranslation()
    {
        if (rootMap == null) return;
        var srcRootRotation = rootMap.srcT.rotation;
        var dstRootRotation = rootMap.dstT.rotation;
        srcRootRotation = Quaternion.Euler(0, srcRootRotation.eulerAngles.y, 0);
        var worldToLocal = Quaternion.Inverse(srcRootRotation);
        var _relativeRootOffset = srcRootRotation * relativeRootOffset;
        //_relativeRootOffset.y = 0;

        Matrix4x4 rootMatrix = mirrorMatrix * Matrix4x4.TRS(Vector3.zero, worldToLocal * srcRootRotation, Vector3.one) * mirrorMatrix;
        rootMap.dstT.rotation = srcRootRotation * rootMatrix.rotation;
        rootMap.dstT.localRotation *= Quaternion.Euler(0, 180, 0);
        var absoluteMirroredRootPos = _relativeRootOffset + rootMap.srcT.position;
        if (applyRootPosition)
        {
            switch (translationMode)
            {
                case TranslationMode.ABSOLUTE:
                    rootMap.dstT.position =absoluteMirroredRootPos;
                    break;
                case TranslationMode.RELATIVE:
                    if (!rootPosSet)
                    {
                        rootMap.dstT.position = absoluteMirroredRootPos;
                    }
                    else
                    {
                        var delta = rootMap.srcT.position - lastPosition;
                        delta = Quaternion.Inverse(rootMap.dstT.rotation)  * -delta;
                        rootMatrix = mirrorMatrix * Matrix4x4.TRS(delta, Quaternion.identity, Vector3.one) * mirrorMatrix;
                        delta = rootMatrix.GetColumn(3);
                        delta.y = 0; //HACK TO prevent moving up and down
                        rootMap.dstT.position = rootMap.dstT.position + rootMap.dstT.rotation  * delta;
                    }
                    lastPosition = rootMap.srcT.position;
                    rootPosSet = true;
                    break;
            };
        }
        Vector3 deltaToAbs = Vector3.zero;
        if(translationMode == TranslationMode.RELATIVE){
            deltaToAbs = rootMap.dstT.position-absoluteMirroredRootPos;
        }
        
        foreach (var m in jointMap)
        {
            if (m.src == "ignore" || m.src == rootName) continue;

            var srcT = m.srcT;
            var dstT = m.dstT;
            var relativePos = worldToLocal * (srcT.position - rootMap.srcT.position);
            var matrix = mirrorMatrix * Matrix4x4.TRS(relativePos, worldToLocal * srcT.rotation, Vector3.one) * mirrorMatrix;

           
            dstT.rotation = srcRootRotation * matrix.rotation;
            dstT.localRotation *= Quaternion.Euler(0, 180, 0);

             Vector3 delta = matrix.GetColumn(3);
            dstT.position = _relativeRootOffset + rootMap.srcT.position + srcRootRotation * delta;
            dstT.position+= deltaToAbs;
        }
        ApplyFootGrounding();

    }
    
    
    public void MirrorPoseGlobalWithExternalTranslation()
    {
    
        if (rootMap == null) return;
        var srcRootRotation = rootMap.srcT.rotation;
        var dstRootRotation = rootMap.dstT.rotation;
        srcRootRotation = Quaternion.Euler(0, srcRootRotation.eulerAngles.y, 0);
        var worldToLocal = Quaternion.Inverse(srcRootRotation);
        var _relativeRootOffset = srcRootRotation * relativeRootOffset;

        Matrix4x4 rootMatrix = mirrorMatrix * Matrix4x4.TRS(Vector3.zero, worldToLocal * srcRootRotation, Vector3.one) * mirrorMatrix;
        rootMap.dstT.rotation = srcRootRotation * rootMatrix.rotation;
        rootMap.dstT.localRotation *= Quaternion.Euler(0, 180, 0);
        var absoluteMirroredRootPos = _relativeRootOffset + rootMap.srcT.position;
        Vector3 deltaToAbs = Vector3.zero;
          
        deltaToAbs = externalPosition-absoluteMirroredRootPos;
        deltaToAbs.y = 0;
        foreach (var m in jointMap)
        {
            if (m.src == "ignore" ) continue;

            var srcT = m.srcT;
            var dstT = m.dstT;
            var relativePos = worldToLocal * (srcT.position - rootMap.srcT.position);
            var matrix = mirrorMatrix * Matrix4x4.TRS(relativePos, worldToLocal * srcT.rotation, Vector3.one) * mirrorMatrix;

           
            dstT.rotation = srcRootRotation * matrix.rotation;
            dstT.localRotation *= Quaternion.Euler(0, 180, 0);

            Vector3 delta = matrix.GetColumn(3);
            dstT.position = _relativeRootOffset + rootMap.srcT.position + srcRootRotation * delta;
            dstT.position += deltaToAbs;
        }
        ApplyFootGroundingV2();
    }
    

    public void ApplyFootGrounding(){
          if (groundFeet && footTip != null && footTip.position.y != 0)
        {

            rootMap.dstT.position += new Vector3(0,-footTip.position.y+footTipOffset,0);
        }
    }
    public void ApplyFootGroundingV2(){
        if (groundFeet || footTip != null) return;
        float deltaToZero = footTip.position.y+footTipOffset;
        if (deltaToZero == 0) return;
        foreach (var m in jointMap)
        {
            if (m.src == "ignore" ) continue;
                m.dstT.position += new Vector3(0,-deltaToZero,0);
        }
    }


    public void MirrorPoseLocal()
    {

        var srcRootRotation = rootMap.srcT.rotation;
        var dstRootRotation = rootMap.dstT.rotation;
        srcRootRotation = Quaternion.Euler(0, srcRootRotation.eulerAngles.y, 0);
        var worldToLocal = Quaternion.Inverse(srcRootRotation);
        var _relativeRootOffset = srcRootRotation * relativeRootOffset;
        Matrix4x4 matrix = mirrorMatrix * Matrix4x4.TRS(Vector3.zero, worldToLocal * srcRootRotation, Vector3.one) * mirrorMatrix;
        rootMap.dstT.rotation = srcRootRotation * matrix.rotation;
        rootMap.dstT.localRotation *= Quaternion.Euler(0, 180, 0);
        if (applyRootPosition)
        {
            switch (translationMode)
            {
                case TranslationMode.ABSOLUTE:
                    rootMap.dstT.position = _relativeRootOffset + rootMap.srcT.position;
                    break;
                case TranslationMode.RELATIVE:
                    if (!rootPosSet)
                    {
                        rootMap.dstT.position = _relativeRootOffset + rootMap.srcT.position;
                    }
                    else
                    {
                        var delta = rootMap.srcT.position - lastPosition;
                        matrix = mirrorMatrix * Matrix4x4.TRS(delta, Quaternion.identity, Vector3.one) * mirrorMatrix;
                        delta = matrix.GetColumn(3);
                        rootMap.dstT.position = rootMap.dstT.position + delta;
                    }
                    lastPosition = rootMap.srcT.position;
                    rootPosSet = true;
                    break;
            };
        }
        ApplyFootGrounding();
        foreach (var m in jointMap)
        {
            if (m.dst == rootName) continue;
            
            var srcT = m.srcT;
            var dstT = m.dstT;
            if(srcT == null){
                Debug.Log(m.src);
            }
            matrix = mirrorMatrix * Matrix4x4.TRS(Vector3.zero, worldToLocal * srcT.rotation, Vector3.one) * mirrorMatrix;
            dstT.rotation = srcRootRotation * matrix.rotation;
            dstT.localRotation *= Quaternion.Euler(0, 180, 0);
        }
    }


    public void SnapTo(Vector3 snapPosition)
    {
        gameObject.SetActive(false);
        transform.position = snapPosition;
        gameObject.SetActive(true);
    }

    static public void ApplyMirroring(Transform src, Transform dst, MirrorSettings settings, string rootName)
    {
        Matrix4x4 mirrorMatrix = new Matrix4x4();
        mirrorMatrix.m00 = settings.mirrorVector.x;
        mirrorMatrix.m11 = settings.mirrorVector.y;
        mirrorMatrix.m22 = settings.mirrorVector.z;
        mirrorMatrix.m33 = 1;

        var _transforms = dst.GetComponentsInChildren<Transform>().ToList();
        var _srcTransforms = src.GetComponentsInChildren<Transform>().ToList();

        var rootT = _srcTransforms.First(x => x.name == rootName);
        var rootRotation = Quaternion.Euler(0, rootT.rotation.eulerAngles.y, 0);
        var worldToLocal = Quaternion.Inverse(rootRotation);
        var _relativeRootOffset = rootRotation * settings.mirrorRootOffset;
        foreach (var m in settings.jointMap)
        {

            var srcT = _srcTransforms.First(x => x.name == m.refName);
            var dstT = _transforms.First(x => x.name == m.name);
            var relativePos = worldToLocal * (srcT.position - rootT.position);
            var matrix = mirrorMatrix * Matrix4x4.TRS(relativePos, worldToLocal * srcT.rotation, Vector3.one) * mirrorMatrix;


            dstT.rotation = rootRotation * matrix.rotation;
            dstT.localRotation *= Quaternion.Euler(0, 180, 0);

            Vector3 delta = matrix.GetColumn(3);
            dstT.position = _relativeRootOffset + rootT.position + rootRotation * delta;
        }
    }
    
    public void SetExternalTargetPosition(Vector3 t){
        externalPosition = t;
    }


    override public void ResetToIdle(){

            rootPosSet = false;
            MirrorPose();
    }

}
