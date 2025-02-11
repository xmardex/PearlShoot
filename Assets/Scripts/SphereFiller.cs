using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SphereFiller : MonoBehaviour
{
    [SerializeField] private Pearl _defaultPearlPrefab;

    [SerializeField] private float sphereRadius = 5f;
    [SerializeField] private float pearlRadius = 0.5f;
    [SerializeField] private float pearlsOffset = 0.5f;

    [SerializeField] private float clusterAffectZone = 5f;
    [SerializeField] private List<Pearl> clusterPearlPrefabs = new List<Pearl>();

    [SerializeField] private List<Pearl> allPearls = new List<Pearl>();
    [Header("Всього перлин. Розраховується автоматично.")]
    [SerializeField] private int sphereCount;

    [SerializeField] private List<PearlCluster> pearlClusters = new List<PearlCluster>();

    [SerializeField] private PearlPlanet _createdPlanet;

    private void OnValidate()
    {
        sphereCount = CalculateSphereCount(sphereRadius, pearlRadius);
    }

    void Start()
    {
        SpawnSpheres(sphereCount);
        SelectClusterCenters(out List<Pearl> clusterCenterPearls);
        PaintClusters(clusterCenterPearls);
        CalculateSameClusters();

        _createdPlanet.SetPearlClusters(pearlClusters);
    }

    private void CalculateSameClusters()
    {
        Dictionary<Vector3, Pearl> pearlMap = allPearls
            .Where(p => p != null)
            .ToDictionary(p => p.transform.position);

        HashSet<Pearl> visited = new HashSet<Pearl>();
        pearlClusters.Clear();

        foreach (Pearl pearl in allPearls)
        {
            if (pearl == null || visited.Contains(pearl)) continue;

            List<Pearl> cluster = new List<Pearl>();
            FindCluster(pearl, cluster, visited, pearlMap);

            if (cluster.Count > 0)
            {
                PearlCluster pearlsCluster = new PearlCluster();
                pearlsCluster.pearlType = pearl.Type;
                pearlsCluster.pearls = cluster;

                foreach (Pearl p in cluster)
                {
                    p.SetGroupCluster(pearlsCluster);
                }

                pearlClusters.Add(pearlsCluster);
            }
        }

        CleanupPearlClusters();
        GroupClustersInParents();


        Debug.Log($"Найдено кластеров: {pearlClusters.Count}");
    }
    private void GroupClustersInParents()
    {
        Dictionary<int, Transform> clusterParents = new Dictionary<int, Transform>();

        for (int i = 0; i < pearlClusters.Count; i++)
        {
            PearlCluster cluster = pearlClusters[i];

            if (!clusterParents.TryGetValue(i, out Transform parent))
            {
                GameObject clusterParent = new GameObject($"Cluster_{cluster.pearlType}_{i}");
                clusterParent.transform.SetParent(transform);
                clusterParents[i] = clusterParent.transform;
                cluster.clusterParent = clusterParent.transform;
                parent = clusterParent.transform;
            }

            foreach (var pearl in cluster.pearls)
            {
                if (pearl != null)
                {
                    pearl.transform.SetParent(parent);
                }
            }
        }
        foreach(PearlCluster cluster in pearlClusters)
        {
            foreach(var pearl in cluster.pearls)
                cluster.FindNeighbors(pearl,pearlRadius,pearlsOffset);
        }
    }


    private void FindCluster(Pearl start, List<Pearl> cluster, HashSet<Pearl> visited, Dictionary<Vector3, Pearl> pearlMap)
    {
        Stack<Pearl> stack = new Stack<Pearl>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            Pearl current = stack.Pop();
            if (visited.Contains(current) || current == null) continue;

            visited.Add(current);
            cluster.Add(current);

            foreach (Pearl neighbor in GetNearbyPearls(current, pearlMap))
            {
                if (neighbor.Type == start.Type)
                {
                    stack.Push(neighbor);
                }
            }
        }
    }

    private List<Pearl> GetNearbyPearls(Pearl pearl, Dictionary<Vector3, Pearl> pearlMap)
    {
        List<Pearl> neighbors = new List<Pearl>();
        foreach (var pair in pearlMap)
        {
            if (Vector3.Distance(pearl.transform.position, pair.Key) <= (pearlRadius + pearlsOffset/2) * 2f)
            {
                neighbors.Add(pair.Value);
            }
        }
        return neighbors;
    }

    private void CleanupPearlClusters()
    {
        for (int i = pearlClusters.Count - 1; i >= 0; i--)
        {
            pearlClusters[i].pearls.RemoveAll(p => p == null);
            if (pearlClusters[i].pearls.Count == 0)
            {
                pearlClusters.RemoveAt(i);
            }
        }
    }

    private int CalculateSphereCount(float bigR, float smallR)
    {
        float S_big = 4 * Mathf.PI * bigR * bigR;
        float S_small = 2 * Mathf.Sqrt(3) * smallR * smallR;
        return Mathf.RoundToInt(S_big / S_small);
    }

    void SpawnSpheres(int sphereCount)
    {
        float phi = Mathf.PI * (3 - Mathf.Sqrt(5));
        for (int i = 0; i < sphereCount; i++)
        {
            float y = 1 - (i / (float)(sphereCount - 1)) * 2;
            float radius = Mathf.Sqrt(1 - y * y);

            float theta = phi * i;
            float x = Mathf.Cos(theta) * radius;
            float z = Mathf.Sin(theta) * radius;

            Vector3 pos = new Vector3(x, y, z) * (sphereRadius - pearlRadius + pearlsOffset);
            Pearl pearl = Instantiate(_defaultPearlPrefab, pos, Quaternion.identity, transform);
            allPearls.Add(pearl);
        }
    }

    void SelectClusterCenters(out List<Pearl> selectedCenterClusterPearls)
    {
        selectedCenterClusterPearls = new List<Pearl>();
        selectedCenterClusterPearls.Add(allPearls[0]);

        while (selectedCenterClusterPearls.Count < clusterPearlPrefabs.Count)
        {
            float maxMinDistance = -1;
            Pearl nextPearl = null;

            foreach (var pearl in allPearls)
            {
                if (selectedCenterClusterPearls.Contains(pearl)) continue;

                float minDistance = float.MaxValue;
                foreach (var selected in selectedCenterClusterPearls)
                {
                    float distance = Vector3.Distance(pearl.transform.position, selected.transform.position);
                    minDistance = Mathf.Min(minDistance, distance);
                }

                if (minDistance > maxMinDistance)
                {
                    maxMinDistance = minDistance;
                    nextPearl = pearl;
                }
            }

            if (nextPearl != null)
            {
                selectedCenterClusterPearls.Add(nextPearl);
            }
        }
    }


    void PaintClusters(List<Pearl> selectedSpheres)
    {
        for (int i = 0; i < selectedSpheres.Count; i++)
        {
            Pearl sphereClusterPrefab = clusterPearlPrefabs[i];
            Pearl centerPearl = selectedSpheres[i];

            List<Pearl> nearestPearls = new List<Pearl>();

            foreach (var pearl in allPearls.ToArray())
            {
                float distance = Vector3.Distance(centerPearl.transform.position, pearl.transform.position);
                if (distance <= clusterAffectZone)
                {
                    nearestPearls.Add(pearl);
                    allPearls.Remove(pearl);
                }
            }

            PearlCluster pearlCluster = new PearlCluster();
            pearlCluster.pearls = new List<Pearl>();
            pearlCluster.pearlType = sphereClusterPrefab.Type;

            for (int j = 0; j < nearestPearls.Count; j++)
            {
                Pearl nearestPearl = nearestPearls[j];
                Pearl newPearl = Instantiate(sphereClusterPrefab, nearestPearl.transform.position, Quaternion.identity, transform);
                pearlCluster.pearls.Add(newPearl);
                allPearls.Add(newPearl);

                Destroy(nearestPearl.gameObject);
            }

            Destroy(centerPearl.gameObject);

            pearlClusters.Add(pearlCluster);
        }
    }
}
