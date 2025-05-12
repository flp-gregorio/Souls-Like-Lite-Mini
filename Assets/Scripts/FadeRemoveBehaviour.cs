using UnityEngine;

public class FadeRemoveBehaviour : StateMachineBehaviour
{
    public float fadeTime = 0.5f;
    private float _timeElapsed = 0;
    SpriteRenderer _spriteRenderer;
    GameObject _objToRemove;
    Color _startColor;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _timeElapsed = 0;
        _spriteRenderer = animator.GetComponent<SpriteRenderer>();
        _startColor = _spriteRenderer.color;
        _objToRemove = animator.gameObject;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _timeElapsed += Time.deltaTime;

        _spriteRenderer.color = new Color(_startColor.r, _startColor.g, _startColor.b, _startColor.a * (1 - _timeElapsed / fadeTime));
        if (_timeElapsed >= fadeTime)
        {
            Destroy(_objToRemove);
        }
    }


}
