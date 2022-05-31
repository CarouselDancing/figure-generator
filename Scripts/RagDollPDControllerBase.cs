
using UnityEngine;
using UnityEngine.Events;


public abstract class RagDollPDControllerBase : MonoBehaviour
{
    public bool DebugPauseOnReset;

     public PhysicsPoseProvider animationSrc;
     public UnityEvent OnReset;
     public bool active = true;

      public abstract void OnEpisodeBegin();
}