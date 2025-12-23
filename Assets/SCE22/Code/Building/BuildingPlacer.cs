using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class BuildingPlacer : SingletonMonoBehaviour<BuildingPlacer>
{
    [SerializeField] private GameObject _previewObj;
    [SerializeField] private Building _building;
    [SerializeField] private BuildingPrefab _prefab;

    [SerializeField, Space(5), Header("Preferences")] private float _yOffset = 0.5f;
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

        if (Input.GetKeyDown(KeyCode.Mouse0))
            Complete();
        if (Input.GetKeyDown(KeyCode.Mouse1))
            Clear();
    }

    public void PlaceFrom(BuildingPrefab prefab)
    {
        Clear();

        _building = null;
        _prefab = prefab;
        _previewObj = Instantiate(prefab.Preview);
    }
    public void PlaceFrom(Building building)
    {
        Clear();

        _building = building;
        _prefab = building.BuildingPrefab;
        _previewObj = Instantiate(building.BuildingPrefab.Preview);

        building.DisableEntity();
    }

    public void Complete()
    {
        if (_previewObj == null && _prefab == null) return;

        var point = MainCamera.Singleton.TryHitGround();
        if (point != null && CanPlaceHere())
        {
            if (_building != null)
            {
                _building.SetRealPosition(point.Value);
                _building.EnableEntity();
            }
            if (_prefab != null)
            {
                var building = Map.Singleton.SpawnEntity(_prefab);
                building.SetRealPosition(point.Value);
            }
        }

        Clear();
    }
    public void Clear()
    {
        if (_previewObj != null)
            Destroy(_previewObj.gameObject);

        if (_building != null)
            _building.EnableEntity();

        _building = null;


        _previewObjColliderCached = null;
        _previewObj = null;
    }

    public bool CanPlaceHere()
    {
        if (_previewObjCollider == null) return true;

        Transform t = _previewObj.transform;
        Vector3 center = t.TransformPoint(_previewObjCollider.center);

        // Correct way to handle lossy scale for OverlapBox
        Vector3 lossyScale = t.lossyScale;
        Vector3 halfExtents = new Vector3(
            _previewObjCollider.size.x * lossyScale.x,
            _previewObjCollider.size.y * lossyScale.y,
            _previewObjCollider.size.z * lossyScale.z
        ) * 0.5f;

        return !Physics.CheckBox(
            center,
            halfExtents,
            t.rotation,
            _obstacleLayers,
            QueryTriggerInteraction.Ignore
        );
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
