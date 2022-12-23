using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ObstacleAvoidance : ISteering
{
    private Transform _from;
    private float _radius;
    private LayerMask _mask;
    private Transform _target;
    private float _avoidWeight;
    private Vector3 _toPos;
    private bool vectorInsteadTransform;
    public ObstacleAvoidance(Transform from, Transform target, float radius, float avoidWeight, LayerMask mask)
    {
        _avoidWeight = avoidWeight;
        _target = target;
        _radius = radius;
        _mask = mask;
        _from = from;
        vectorInsteadTransform = false;
    }
    public ObstacleAvoidance(Transform from, Vector3 to, float radius, float avoidWeight, LayerMask mask)
    {
        _from = from;
        _toPos = to;
        _radius = radius;
        _avoidWeight = avoidWeight;
        _mask = mask;
        vectorInsteadTransform = true;
    }
    public Vector3 GetDir()
    {
        Vector3 target;
        if (vectorInsteadTransform)
            target = _toPos;
        else
            target = _target.position;
        var dir = (target - _from.position).normalized;
        var obstacles = Physics.OverlapSphere(_from.position, _radius, _mask);
        if (obstacles.Length > 0)
        {
            var distance = Vector3.Distance(obstacles[0].transform.position, _from.position);
            var indexSave = 0;
            for (var i = 1; i < obstacles.Length; i++)
            {
                var currDistance = Vector3.Distance(obstacles[i].transform.position, _from.position);
                if (currDistance < distance)
                {
                    distance = currDistance;
                    indexSave = i;
                }
            }
            var dirFromObs = (_from.position - obstacles[indexSave].transform.position).normalized *
                             ((_radius - distance) / _radius) * _avoidWeight;
            dir += dirFromObs;
        }
        return dir.normalized;
    }
}