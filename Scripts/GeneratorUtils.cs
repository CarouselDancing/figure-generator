using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorUtils : MonoBehaviour
{


    //https://forum.unity.com/threads/change-gameobject-layer-at-run-time-wont-apply-to-child.10091/
    public static void SetLayerRecursively(Transform t, int newLayer)
    {
        t.gameObject.layer = newLayer;

        for (int i = 0; i < t.childCount; i++)
        {
            SetLayerRecursively(t.GetChild(i), newLayer);
        }
    }

    public static Transform FindChildWithBodyComponent(Transform t)
    {

        Transform result = null;
        if (t.GetComponent<ArticulationBody>() != null)
        {
            result = t;
        }
        else
        {
            Transform tempResult;
            for (int i = 0; i < t.childCount; i++)
            {
                tempResult = FindChildWithBodyComponent(t.GetChild(i));
                if (tempResult != null)
                {
                    result = tempResult;
                    break;
                }
            }
        }
        return result;
    }

    public static void SetMaterialRecursively(Transform t, Material m)
    {
        var renderer = t.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = m;
        }

        var skinRenderer = t.GetComponent<SkinnedMeshRenderer>();
        if (skinRenderer != null)
        {
            skinRenderer.material = m;
        }

        for (int i = 0; i < t.childCount; i++)
        {
            SetMaterialRecursively(t.GetChild(i), m);
        }
    }

    
    public static GameObject CreateChildObject(Transform parent, Vector3 delta, Vector3 offset, Quaternion rotation, float radius, float heightScale, ArticulationBody referenceBody = null, bool isKinematic=true)
    {
        var capsuleObject = new GameObject();
        capsuleObject.name = parent.name + "Capsule";
        ProceduralCapsule c = capsuleObject.AddComponent<ProceduralCapsule>();
        c.height = delta.magnitude * heightScale;
        c.radius = radius;//widthScale * width;
        Debug.Log(parent.name + " " + offset.ToString());
        c.CreateMesh();
        capsuleObject.transform.position = parent.position + offset;
        capsuleObject.transform.localRotation = rotation;
        capsuleObject.transform.parent = parent;
        Rigidbody rb = capsuleObject.AddComponent<Rigidbody>();
        rb.isKinematic = isKinematic;
        if (referenceBody != null) rb.mass = referenceBody.mass;
        return capsuleObject;
    }

    public static void CreateOffsetCapsuleCollider(Transform t, Vector3 delta, Vector3 offset, Quaternion rotation, float radius, float heightScale)
    {
        ProceduralCapsule c = t.gameObject.AddComponent<ProceduralCapsule>();
        c.height = delta.magnitude * heightScale;
        c.radius = radius;//widthScale * width;
        c.CreateMeshWithOffset(offset, rotation);
    }

    public static bool FindChild(Transform node, string name, out Transform childTransform)
    {
        bool found = false;
        childTransform = null;
        if (node.name == name)
        {
            found = true;
            childTransform = node;
        }
        if (!found)
        {
            for (int i = 0; i < node.childCount; i++)
            {
                Transform _ct;
                if (FindChild(node.GetChild(i), name, out _ct))
                {
                    childTransform = _ct;
                    found = true;
                    break;
                }
            }
        }
        return found;
    }

}
