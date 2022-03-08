#if UNITY

using System.Collections.Generic;
using Lost;
using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour
{
    private GenericController playerController;

    public List<EnemyController> Enemies { get; private set; }

    public int NumberOfEnemiesTotal { get; private set; }

    public int NumberOfEnemiesRemaining => this.Enemies.Count;

    public UnityAction<EnemyController, int> OnRemoveEnemy;

    public void RegisterEnemy(EnemyController enemy)
    {
        this.Enemies.Add(enemy);
        this.NumberOfEnemiesTotal++;
    }

    public void UnregisterEnemy(EnemyController enemyKilled)
    {
        int enemiesRemainingNotification = this.NumberOfEnemiesRemaining - 1;

        this.OnRemoveEnemy?.Invoke(enemyKilled, enemiesRemainingNotification);

        // removes the enemy from the list, so that we can keep track of how many are left on the map
        this.Enemies.Remove(enemyKilled);
    }

    private void Awake()
    {
        this.playerController = FindObjectOfType<GenericController>();
        DebugUtility.HandleErrorIfNullFindObject<GenericController, EnemyManager>(this.playerController, this);

        this.Enemies = new List<EnemyController>();
    }
}

#endif
