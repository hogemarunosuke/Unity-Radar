using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Radar : BaseBehaviour
{
    public GameObject enemyIconPrefab;
    public float destroyUnusedIconInterval = 5.0f;

    private GameObject player;
    private GameObject radarViewAngle;
    private GameObject freeLookCameraRig;
    private float detectionRange = 100.0f;
    private float radarRadius;

    // Collection for Object Pooling.
    private List<GameObject> enemyIconCollection;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    private void Start()
    {
        // Set the Player object.
        player = GameObject.FindGameObjectWithTag("Player");

        // Set the RadarViewAngle object.
        radarViewAngle = GameObject.Find("RadarViewAngle");

        // Set the FreeLookCameraRig. (or another Camera object that you can get eulerAngles.)
        freeLookCameraRig = GameObject.Find("FreeLookCameraRig");

        // Get the radius of radar.
        radarRadius = GetComponent<RectTransform>().sizeDelta.x / 2.0f;

        // Start Object Pooling.
        enemyIconCollection = new List<GameObject>();
        StartCoroutine(DestroyUnusedIcons(destroyUnusedIconInterval));
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    private void Update()
    {
        // Update the angle.
        radarViewAngle.transform.rotation = Quaternion.Euler(0, 0, -freeLookCameraRig.transform.eulerAngles.y);

        // Hide all icons.
        HideAllIcons();

        // Plot all enemies within range.
        var enemys = GameObject.FindGameObjectsWithTag("Enemy")
            .Where(enemy => Vector3.Distance(player.transform.position, enemy.transform.position) < detectionRange);
        foreach (var enemy in enemys)
        {
            PlotEnemyIcons(enemy.transform.position);
        }
    }

    /// <summary>
    /// Hide all icons.
    /// </summary>
    private void HideAllIcons()
    {
        foreach (var enemyIcon in enemyIconCollection)
        {
            enemyIcon.SetActive(false);
        }
    }

    /// <summary>
    /// Plot all enemies within range.
    /// </summary>
    /// <param name="enemyPos">The position of enemy.</param>
    private void PlotEnemyIcons(Vector3 enemyPos)
    {
        Vector3 plotPos = (enemyPos - player.transform.position) * (radarRadius / detectionRange);
        RectTransform iconRect = GetEnemyIcon().GetComponent<RectTransform>();
        iconRect.anchoredPosition = new Vector2(plotPos.x, plotPos.z);
    }

    /// <summary>
    /// Get enemy icon.
    /// </summary>
    /// <returns>The enemy object.</returns>
    private GameObject GetEnemyIcon()
    {
        foreach (var oldEnemyIcon in enemyIconCollection)
        {
            if (oldEnemyIcon.activeSelf == false)
            {
                oldEnemyIcon.SetActive(true);
                return oldEnemyIcon;
            }
        }

        var newEnemyIcon = GameObject.Instantiate(enemyIconPrefab) as GameObject;
        newEnemyIcon.transform.SetParent(transform, false);
        enemyIconCollection.Add(newEnemyIcon);

        return newEnemyIcon;
    }

    /// <summary>
    /// Destroy unused icons.
    /// </summary>
    /// <param name="interval">The interval.</param>
    /// <returns></returns>
    private IEnumerator DestroyUnusedIcons(float interval)
    {
        while (true)
        {
            var enemyIconRemoveList = new List<GameObject>();
            foreach (var enemyIcon in enemyIconCollection)
            {
                enemyIconRemoveList.Add(enemyIcon);
            }
            foreach (var enemyIcon in enemyIconRemoveList)
            {
                if (enemyIcon.activeSelf == false)
                {
                    enemyIconCollection.Remove(enemyIcon);
                    Destroy(enemyIcon);
                }
            }

            yield return new WaitForSeconds(interval);
        }
    }

    /// <summary>
    /// Set the detection range.
    /// </summary>
    /// <param name="detectionRange">The detection range.</param>
    public void SetDetectionRange(float detectionRange)
    {
        this.detectionRange = detectionRange;
    }

    /// <summary>
    /// Get the detection range.
    /// </summary>
    /// <returns>The detection range.</returns>
    public float GetDetectionRange()
    {
        return this.detectionRange;
    }
}