using System.Collections;
using System.Collections.Generic;
using UnityEngine;



class HumanoidSettingsGenerator
{

    public static List<FigureGeneratorSettings.RefBodyMapping> CreateHumanoidReferenceBodyMap(GameObject model)
    {
        var anim = model.GetComponent<Animator>();
        var rightLeg = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).name;
        var leftLeg = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).name;
        var rightLowerLeg = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg).name;
        var leftLowerLeg = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).name;
        var leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot).name;
        var rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot).name;


        var hips = anim.GetBoneTransform(HumanBodyBones.Hips).name;
        var spine = anim.GetBoneTransform(HumanBodyBones.Spine).name;
        var chest = anim.GetBoneTransform(HumanBodyBones.Chest).name;
        var upperChest = anim.GetBoneTransform(HumanBodyBones.UpperChest).name;
        var head = anim.GetBoneTransform(HumanBodyBones.Head).name;
        var rightArm = anim.GetBoneTransform(HumanBodyBones.RightUpperArm).name;
        var leftArm = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).name;
        var rightLowerArm = anim.GetBoneTransform(HumanBodyBones.RightLowerArm).name;
        var leftLowerArm = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm).name;
        var referenceBodies = new List<FigureGeneratorSettings.RefBodyMapping>();

        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = hips, refName = "butt" });
        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = spine, refName = "lower_waist" });
        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = chest, refName = "upper_waist" });
        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = upperChest, refName = "torso" });
        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = head, refName = "head" });

        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = leftLeg, refName = "left_thigh" });
        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = leftLowerLeg, refName = "left_shin" });
        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = leftFoot, refName = "left_left_foot" });
        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = rightLeg, refName = "right_thigh" });
        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = rightLowerLeg, refName = "right_shin" });
        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = rightFoot, refName = "right_right_foot" });


        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = leftArm, refName = "left_upper_arm" });
        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = leftLowerArm, refName = "left_larm" });

        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = rightArm, refName = "right_upper_arm" });
        referenceBodies.Add(new FigureGeneratorSettings.RefBodyMapping { name = rightLowerArm, refName = "right_larm" });

        return referenceBodies;
    }


    public static List<RuntimeMirroring.JointMap> CreateHumanoidMirrorMap(GameObject model)
    {
        var anim = model.GetComponent<Animator>();
        var jointMap = new List<RuntimeMirroring.JointMap>();
        var leftLeg = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).name;
        var rightLeg = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = leftLeg, dst = rightLeg });
        jointMap.Add(new RuntimeMirroring.JointMap { src = rightLeg, dst = leftLeg });
        var leftLowerLeg = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg).name;
        var rightLowerLeg = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = leftLowerLeg, dst = rightLowerLeg });
        jointMap.Add(new RuntimeMirroring.JointMap { src = rightLowerLeg, dst = leftLowerLeg });
        var leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot).name;
        var rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = leftFoot, dst = rightFoot });
        jointMap.Add(new RuntimeMirroring.JointMap { src = rightFoot, dst = leftFoot });


        var hips = anim.GetBoneTransform(HumanBodyBones.Hips).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = hips, dst = hips });
        var spine = anim.GetBoneTransform(HumanBodyBones.Spine).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = spine, dst = spine });
        var chest = anim.GetBoneTransform(HumanBodyBones.Chest).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = chest, dst = chest });
        var upperChest = anim.GetBoneTransform(HumanBodyBones.Chest).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = upperChest, dst = upperChest });
        var head = anim.GetBoneTransform(HumanBodyBones.Head).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = head, dst = head });
        var leftArm = anim.GetBoneTransform(HumanBodyBones.RightUpperArm).name;
        var rightArm = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = leftArm, dst = rightArm });
        jointMap.Add(new RuntimeMirroring.JointMap { src = rightArm, dst = leftArm });
        var leftLowerArm = anim.GetBoneTransform(HumanBodyBones.RightLowerArm).name;
        var rightLowerArm = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = leftLowerArm, dst = rightLowerArm });
        jointMap.Add(new RuntimeMirroring.JointMap { src = rightLowerArm, dst = leftLowerArm });

        return jointMap;
    }

    public static List<HumanoidMusclePower> CreateHumanoidMusclePowers()
    {
        var musclePowers = new List<HumanoidMusclePower>();
        musclePowers.Add(new HumanoidMusclePower()
        {
            Muscle = HumanBodyBones.Spine,
            PowerVector = new Vector3(40, 40, 40)
        });
        musclePowers.Add(new HumanoidMusclePower()
        {
            Muscle = HumanBodyBones.Chest,
            PowerVector = new Vector3(40, 40, 40)
        });
        musclePowers.Add(new HumanoidMusclePower()
        {
            Muscle = HumanBodyBones.RightUpperLeg,
            PowerVector = new Vector3(40, 120, 40)
        });

        musclePowers.Add(new HumanoidMusclePower()
        {
            Muscle = HumanBodyBones.RightLowerLeg,
            PowerVector = new Vector3(80, 80, 80)
        });
        musclePowers.Add(new HumanoidMusclePower()
        {
            Muscle = HumanBodyBones.RightFoot,
            PowerVector = new Vector3(40, 20, 20)
        }); musclePowers.Add(new HumanoidMusclePower()
        {
            Muscle = HumanBodyBones.LeftUpperLeg,
            PowerVector = new Vector3(40, 120, 40)
        });

        musclePowers.Add(new HumanoidMusclePower()
        {
            Muscle = HumanBodyBones.LeftLowerLeg,
            PowerVector = new Vector3(80, 80, 80)
        });
        musclePowers.Add(new HumanoidMusclePower()
        {
            Muscle = HumanBodyBones.LeftFoot,
            PowerVector = new Vector3(40, 20, 20)
        });

        musclePowers.Add(new HumanoidMusclePower()
        {
            Muscle = HumanBodyBones.LeftUpperArm,
            PowerVector = new Vector3(40, 40, 40)//20
        });
        musclePowers.Add(new HumanoidMusclePower()
        {
            Muscle = HumanBodyBones.LeftLowerArm,
            PowerVector = new Vector3(40, 40, 40)
        });

        musclePowers.Add(new HumanoidMusclePower()
        {
            Muscle = HumanBodyBones.RightUpperArm,
            PowerVector = new Vector3(40, 40, 40)//20
        });
        musclePowers.Add(new HumanoidMusclePower()
        {
            Muscle = HumanBodyBones.RightLowerArm,
            PowerVector = new Vector3(40, 40, 40)
        });
        return musclePowers;
    }

    public static void CreateSettingsFromHumanoidModel(GameObject model, ref FigureGeneratorSettings settings)
    {

        var anim = model.GetComponent<Animator>();
        settings.rootName = anim.GetBoneTransform(HumanBodyBones.Hips).name;
        settings.headName = anim.GetBoneTransform(HumanBodyBones.Head).name;
        settings.referenceBodies = CreateHumanoidReferenceBodyMap(model);
        if (settings.HumanoidMusclePowers.Count == 0)
        {
            settings.HumanoidMusclePowers = CreateHumanoidMusclePowers();
        }
        settings.MusclePowers = new List<RagDoll003.MusclePower>();
        foreach (var m in settings.HumanoidMusclePowers)
        {
            settings.MusclePowers.Add(m.MapToTransform(anim));
        }
        settings.endEffectors = new List<string>();
        settings.endEffectors.Add(anim.GetBoneTransform(HumanBodyBones.RightHand).name);
        settings.endEffectors.Add(anim.GetBoneTransform(HumanBodyBones.LeftHand).name);
    }

}