using UnityEngine;
using UnityEngine.Serialization;

namespace Combat
{
    public class CombatStateBehaviorBase : StateMachineBehaviour
    {
        [FormerlySerializedAs("StateName")]
        [SerializeField]
        protected string stateName;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // No longer setting StateName here; it's done in the specific behaviors.
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (CombatManager.instance != null)
            {
                CombatManager.instance.HandleCombatState(stateName);
            }
            else
            {
                Debug.LogError("CombatManager Instance is null!");
            }
        }
    }

    public class IdleBehavior : CombatStateBehaviorBase
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            stateName = "Idle";
        }
    }

    public class Attack1Behavior : CombatStateBehaviorBase
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            stateName = "Attack1";
        }
    }

    public class Attack2Behavior : CombatStateBehaviorBase
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            stateName = "Attack2";
        }
    }

    public class TransitionBehavior : CombatStateBehaviorBase
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            stateName = "Transition";
        }
    }

    public class DodgeBehavior : CombatStateBehaviorBase
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            stateName = "Dodge";
        }
    }

    public class DashBehavior : CombatStateBehaviorBase
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            stateName = "Dash";
        }
    }
}
