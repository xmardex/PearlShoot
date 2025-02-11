using System;
using System.Collections.Generic;
using UnityEngine;

public enum PearlType { none, red, green, blue };

public partial class Pearl : MonoBehaviour
{
    [SerializeField] private PearlType _type;
    [SerializeField] private Rigidbody _rb;

    [SerializeField] private PearlCluster _pearlCluster;

    [SerializeField] public List<Pearl> NeighborsPearls;

    public PearlType Type => _type;

    private void Update()
    {

        if (transform.position.y < -30)
            Destroy(gameObject);
    }

    public void SetGroupCluster(PearlCluster pearlCluster)
    {
        _pearlCluster = pearlCluster;
    }

    public void RightPearl()
    {
        _pearlCluster.DestroyCluster(this);
    }

    public void WrongPearl()
    {
        _pearlCluster.ShakeCluster();
    }

    public void SetKinematic(bool isKinematic)
    {
        _rb.isKinematic = isKinematic;
    }

    public void CollectPearl()
    {
        //add score and etc 
    }

}
