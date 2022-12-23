using System;
using UnityEngine;
public class GridEntity : MonoBehaviour
{
    public event Action<GridEntity> OnMove = delegate { };
    private SpatialGrid _spatialGrid;
    public bool onGrid;
    protected virtual void Awake()
    {
        _spatialGrid = FindObjectOfType<SpatialGrid>();
    }
    protected virtual void Start()
    {
        _spatialGrid.SubscribeGridEntity(this);
    }
    private void OnDestroy()
    {
        _spatialGrid.UnsubscribeGridEntity(this);
    }
    public virtual void Move(Vector3 dir)
    {
        OnMove(this);
    }
}