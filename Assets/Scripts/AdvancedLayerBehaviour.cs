using UnityEngine;
using System.Collections.Generic;

public class AdvancedLayerBehaviour : StateMachineBehaviour
{
    [Header("Layer Settings")]
    public string targetLayer = "Default";
    public bool affectChildren = true;
    public bool restoreOnExit = true;

    private int _originalLayer;
    private int _targetLayerID;
    private GameObject _target;
    private readonly List<GameObject> _children = new List<GameObject>();

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _target = animator.gameObject;
        CacheLayers();
        CacheChildren();
        SetLayers(_targetLayerID);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(restoreOnExit) SetLayers(_originalLayer);
    }

    private void CacheLayers()
    {
        if(_targetLayerID != 0) return;
        
        _originalLayer = _target.layer;
        _targetLayerID = LayerMask.NameToLayer(targetLayer);
        
        if(_targetLayerID == -1)
            Debug.LogError($"Layer '{targetLayer}' not configured!", _target);
    }

    private void CacheChildren()
    {
        if(_children.Count > 0 || !affectChildren) return;
        
        foreach(Transform child in _target.transform)
        {
            _children.Add(child.gameObject);
        }
    }

    private void SetLayers(int layer)
    {
        _target.layer = layer;
        
        if(!affectChildren) return;
        
        foreach(var child in _children)
        {
            if(child != null) child.layer = layer;
        }
    }
}
