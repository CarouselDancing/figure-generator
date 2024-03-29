using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RagDoll003 : MonoBehaviour {

    [System.Serializable]
    public class MusclePower
    {
        public string Muscle;
        public Vector3 PowerVector;
    }

    [System.Serializable]
    public class IgnoreStruct
    {
        public string name;
        public List<string> ignoreList;
    }

    public List<IgnoreStruct> ignoreList;

    public List<MusclePower> MusclePowers;

    public float MotorScale = 1f;
    public float Stiffness = 100f;
    public float Damping = 100f;

	// Use this for initialization
	void Start () {
		Setup();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void Setup () {
        // handle collision overlaps
        if (ignoreList.Count == 0) {
            foreach (var i in ignoreList) { 
                foreach (var other in i.ignoreList)
                {
                    IgnoreCollision(i.name, other);
                }
            }
        }
        else
        {
            IgnoreCollision("torso", new[] { "left_upper_arm", "right_upper_arm" });
            IgnoreCollision("butt", new[] { "left_thigh", "right_thigh" });

            IgnoreCollision("left_larm", new[] { "left_upper_arm" });
            IgnoreCollision("right_larm", new[] { "right_upper_arm" });
            IgnoreCollision("left_shin", new[] { "left_thigh" });
            IgnoreCollision("right_shin", new[] { "right_thigh" });

            IgnoreCollision("right_shin", new[] { "right_right_foot" });
            IgnoreCollision("left_shin", new[] { "left_left_foot" });

        }

        //
        var joints = GetComponentsInChildren<Joint>().ToList();
        foreach (var joint in joints)
            joint.enablePreprocessing = false;
	}
	void IgnoreCollision(string first, string[] seconds)
    {
        foreach (var second in seconds)
        {
            IgnoreCollision(first, second);
        }
    }
    void IgnoreCollision(string first, string second)
    {
        var rigidbodies = GetComponentsInChildren<Rigidbody>().ToList();
        var colliderOnes = rigidbodies.FirstOrDefault(x=>x.name.Contains(first))?.GetComponents<Collider>();
        var colliderTwos = rigidbodies.FirstOrDefault(x=>x.name.Contains(second))?.GetComponents<Collider>();
        if (colliderOnes == null || colliderTwos == null)
            return;
        foreach (var c1 in colliderOnes)
            foreach (var c2 in colliderTwos)
                Physics.IgnoreCollision(c1, c2);
    }
}
