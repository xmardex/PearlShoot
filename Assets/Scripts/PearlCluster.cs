﻿using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]

public class PearlCluster
{
    public PearlType pearlType;
    public List<Pearl> pearls;
    public Transform clusterParent;

    public void DestroyCluster(Pearl targetPearl)
    {
        Queue<Pearl> pearlsToDestroy = new Queue<Pearl>();
        pearlsToDestroy.Enqueue(targetPearl);

        HashSet<Pearl> destroyedPearls = new HashSet<Pearl>();

        // Animation parameters
        float delay = 0.01f;

        Sequence destructionSequence = DOTween.Sequence();

        while (pearlsToDestroy.Count > 0)
        {
            Pearl currentPearl = pearlsToDestroy.Dequeue();
            Transform currentPearlTransform = currentPearl.transform;
            if (destroyedPearls.Contains(currentPearl))
                continue;

            destroyedPearls.Add(currentPearl);

            //add some tween to destructionSequence

            destructionSequence.AppendCallback(() => {
                currentPearl.SetKinematic(false);
                currentPearl.gameObject.layer = LayerMask.NameToLayer("CollectedPearl");
                currentPearl.CollectPearl();

            });

            // Enqueue all un-destroyed neighbors of the current pearl
            foreach (Pearl neighbor in currentPearl.NeighborsPearls)
            {
                if (!destroyedPearls.Contains(neighbor))
                {
                    pearlsToDestroy.Enqueue(neighbor);
                }
            }

            // Add a delay between each pearl's destruction
            destructionSequence.AppendInterval(delay);
        }

        destructionSequence.Play();
    }





    public void FindNeighbors(Pearl pearl, float pearlRadius, float offset)
    {
        List<Pearl> neighbors = new List<Pearl>();
        float neighborRadius = (pearlRadius+ offset/2) * 2;  // The radius within which pearls are considered neighbors

        foreach (var otherPearl in pearls)
        {
            // Skip the pearl itself
            if (otherPearl == pearl)
                continue;

            // Check the distance between pearls
            float distance = Vector3.Distance(pearl.transform.position, otherPearl.transform.position);
            if (distance <= neighborRadius)
            {
                neighbors.Add(otherPearl);
            }
        }

        // Cache the neighbors for this pearl
        pearl.NeighborsPearls = neighbors;
    }


    public void ShakeCluster()
    {
        Sequence pulseSequence = DOTween.Sequence();

        pulseSequence.Append(clusterParent.DOScale(1.1f, 0.15f).SetEase(Ease.OutQuad))
            .Append(clusterParent.DOScale(0.95f, 0.1f).SetEase(Ease.InOutQuad))
            .Append(clusterParent.DOScale(1f, 0.15f).SetEase(Ease.OutElastic));

    }
}