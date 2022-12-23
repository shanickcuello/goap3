using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class EnemySpawner : MonoBehaviour
{
    public Transform enemiesContainer;
    public GameObject enemyPrefab;
    public Vector3 size;
    public int maxCapacity = 5;
    [SerializeField] private List<GameObject> enemies;
    public float checkTime;
    public float _currentCheckTime;
    public List<GameObject> Enemies => enemies;
    private void Start()
    {
        for (var i = 0; i < enemies.Count; i++) enemies[i].GetComponent<EnemyModel>().onDie += x => enemies.Remove(x);
    }
    private void Update()
    {
        if (_currentCheckTime < 0)
        {
            if (enemies.Count < maxCapacity)
            {
                var newEnemy = Instantiate(enemyPrefab, GetNewEnemyPosition(), Quaternion.identity, enemiesContainer);
                newEnemy.GetComponent<EnemyModel>().onDie += x => enemies.Remove(x);
                enemies.Add(newEnemy);
            }
            _currentCheckTime = checkTime;
        }
        else
        {
            _currentCheckTime -= Time.deltaTime;
        }
    }
    private Vector3 GetNewEnemyPosition()
    {
        float deltaX, deltaZ, xPos, zPos;
        deltaX = size.x / 2;
        deltaZ = size.z / 2;
        xPos = transform.position.x + Random.Range(-deltaX, deltaX);
        zPos = transform.position.z + Random.Range(-deltaZ, deltaZ);
        return new Vector3(xPos, 0, zPos);
    }
    private void OnDrawGizmosSelected()
    {
        var cubePos = transform.position + new Vector3(0, size.y / 2, 0);
        Gizmos.color = new Color(.5f, .5f, 1, .5f);
        Gizmos.DrawCube(cubePos, size);
    }
}