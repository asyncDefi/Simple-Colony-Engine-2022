using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class BuildingPlacer : SingletonMonoBehaviour<BuildingPlacer>
{
    [SerializeField] private GameObject _previewObj;
    [SerializeField] private float _yOffset = 0.5f;
    [SerializeField] private LayerMask _obstacleLayers;

    private BoxCollider _previewObjColliderCached;
    private BoxCollider _previewObjCollider
    {
        get
        {
            if (_previewObjColliderCached == null)
                _previewObjColliderCached = _previewObj.GetComponent<BoxCollider>();
            return _previewObjColliderCached;
        }
    }

    private void Update()
    {
        if (_previewObj == null) return;

        var camera = MainCamera.Singleton;

        var hit = camera.TryHitGround();
        if (hit != null)
        {
            Vector3 pos = new Vector3(hit.Value.x, hit.Value.y + _yOffset, hit.Value.z);
            _previewObj.transform.position = pos;
        }
    }

    public bool CanPlaceHere()
    {
        if (_previewObjCollider == null) return true;

        // Calculate overlap box based on the collider's bounds
        Vector3 center = _previewObj.transform.position + _previewObjCollider.center;
        Vector3 halfExtents = _previewObjCollider.size * 0.5f * _previewObj.transform.localScale.x;

        // Check for any colliders in the obstacle layers
        Collider[] colliders = Physics.OverlapBox(
            center,
            halfExtents,
            _previewObj.transform.rotation,
            _obstacleLayers,
            QueryTriggerInteraction.Ignore
        );

        return colliders.Length == 0;
    }

    private void OnDrawGizmos()
    {
        if (_previewObj == null || _previewObjCollider == null) return;

        Gizmos.color = CanPlaceHere() ? Color.green : Color.red;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(_previewObj.transform.position, _previewObj.transform.rotation, _previewObj.transform.localScale);
        Gizmos.matrix = rotationMatrix;

        Gizmos.DrawWireCube(_previewObjCollider.center, _previewObjCollider.size);
    }
}
