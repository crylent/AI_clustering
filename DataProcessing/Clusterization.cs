using System;
using System.Collections.Generic;
using System.Linq;

namespace AI_clusterization.DataProcessing;

public static class Clusterization
{
    public static List<Cluster> Run(Dataset dataset, int numberOfClusters)
    {
        var datasetCopy = new Dataset(dataset);
        
        var clusters = new List<Cluster>(numberOfClusters);
        var centers = new List<double[]>(numberOfClusters);
        
        // First step: random centers
        for (var i = 0; i < numberOfClusters; i++)
        {
            var center = PopRandomFromDataset(datasetCopy);
            clusters.Add(new Cluster { center });
            centers.Add(center.Power.ToArray());
        }
        
        // First distribution into clusters
        DistributeIntoClusters(datasetCopy, clusters, centers);

        while (true)
        {
            var newCenters = CalculateCenters(clusters);
            if (CentersAreEqual(centers, newCenters))
            {
                return clusters;
            }

            centers = newCenters;
            ClearClusters(clusters);
            DistributeIntoClusters(dataset, clusters, centers);
        }
    }

    private static void DistributeIntoClusters(Dataset dataset, List<Cluster> clusters, IReadOnlyList<double[]> centers)
    {
        var datasetCopy = new Dataset(dataset);
        while (datasetCopy.Count > 0)
        {
            var item = PopNextFromDataset(datasetCopy);
            clusters[FindBestCluster(centers, item)].Add(item);
        }
    }

    private static List<double[]> CalculateCenters(IEnumerable<Cluster> clusters)
        => clusters.Select(cluster => cluster.CenterOfMass()).ToList();

    private const double Tolerance = 0.0001;

    private static bool CentersAreEqual(IReadOnlyList<double[]> a, IReadOnlyList<double[]> b)
    {
        for (var i = 0; i < a.Count; i++)
        {
            var centerA = a[i];
            var centerB = b[i];
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var j = 0; j < centerA.Length; j++)
            {
                if (Math.Abs(centerA[j] - centerB[j]) > Tolerance) return false;
            }
        }

        return true;
    }


    private static void ClearClusters(List<Cluster> clusters)
    {
        foreach (var cluster in clusters)
        {
            cluster.Clear();
        }
    }

    private static int FindBestCluster(IReadOnlyList<double[]> centers, Spectrum item)
    {
        var bestCluster = -1;
        var bestDist = double.PositiveInfinity;
        for (var i = 0; i < centers.Count; i++)
        {
            var dist = item.DistanceTo(centers[i]);
            if (dist >= bestDist) continue;
            bestCluster = i;
            bestDist = dist;
        }
        return bestCluster;
    }

    private static Spectrum PopRandomFromDataset(Dataset dataset)
    {
        var rand = Random.Shared.Next(dataset.Count);
        return PopFromDataset(dataset, rand);
    }

    private static Spectrum PopNextFromDataset(Dataset dataset) => PopFromDataset(dataset, 0);

    private static Spectrum PopFromDataset(Dataset dataset, int index)
    {
        var item = dataset[index];
        dataset.RemoveAt(index);
        return item;
    }
}