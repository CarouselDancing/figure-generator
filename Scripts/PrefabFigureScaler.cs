using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PrefabFigureScaler : MonoBehaviour
{

    public GameObject prefab;
    public GameObject stabilizerJointPrefab;
    public Transform root;
    public ArticulationBody figureRoot;
    public bool DestroyWhenDone = true;

    public List<RuntimeRetargeting.RetargetingMap> jointMap;
    public List<string> endEffectors;

    public bool done;
    void Start()
    {

        if(!done) Run();
    }

    public void Run()
    {
        var figure = GameObject.Instantiate(prefab);
        figure.gameObject.SetActive(false);
        figureRoot = figure.GetComponentInChildren<ArticulationBody>();
        var p =  root.position;
        var figP = figureRoot.transform.position;
        figP.x = p.x;
        figP.z = p.z;
        figureRoot.transform.position = figP;
        var  _srcTransforms  = figure.GetComponentsInChildren<Transform>().ToList();
       var  _transforms = root.parent.GetComponentsInChildren<Transform>().ToList();

        for (int i =0;i <jointMap.Count; i++)
        {
            var srcT = _srcTransforms.FirstOrDefault(x => x.name == jointMap[i].src);
            var dstT = _transforms.FirstOrDefault(x => x.name == jointMap[i].dst);
            if (srcT != null && dstT != null && dstT.name != figureRoot.name && srcT.childCount > 0 && !endEffectors.Contains(dstT.name) ){
                SetLocalScale(srcT, dstT);
            }else if(srcT == null){
                Debug.Log(jointMap[i].src+ _srcTransforms.Count.ToString());
            }
            else if (dstT== null){
                Debug.Log( jointMap[i].dst);
            }
          
        }


         if (stabilizerJointPrefab != null){
                var jointO = GameObject.Instantiate(stabilizerJointPrefab, figure.transform);
                jointO.transform.position = Vector3.zero;
                var stabilizerJoint = jointO.GetComponent<ConfigurableJoint>();
                stabilizerJoint.connectedArticulationBody = figureRoot;
            }

        AddEnvironmentComponents();
        figure.gameObject.SetActive(true);
        var retargeting = root.parent.gameObject.AddComponent<RuntimeRetargeting>();
        retargeting.retargetingMap = jointMap;
        retargeting.src = figureRoot.transform;
        retargeting.rootName = root.name;
        retargeting.mirrorVector =  Vector3.one;
        retargeting.SetTransforms();
        done = true;
        if (DestroyWhenDone) DestroyImmediate(this);
    }

    void SetLocalScale(Transform src, Transform dst){
        var srcChild = src.GetChild(0);
        var srcLen = (srcChild.position - src.position).magnitude;
        var dstChild = dst.GetChild(0);
        var dstLen =  (dstChild.position - dst.position).magnitude;
        srcLen = src.GetComponent<CapsuleCollider>().height;

        // dstr= localScale *srcLen ;
        if (dstLen != 0 && srcLen != 0){
            Debug.Log(dst.name+dstLen.ToString());
            var localScale = dstLen / srcLen;
            //src.localScale *=  localScale;
             src.GetComponent<CapsuleCollider>().height *= localScale;
        }

    }


    void AddEnvironmentComponents()
    {

        RagDollPDController controller = figureRoot.transform.parent.gameObject.AddComponent<RagDollPDController>();
       
        
    }

}
