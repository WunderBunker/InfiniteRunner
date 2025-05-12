using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Cours : MonoBehaviour
{
    [SerializeField] GameObject _tree;
    [SerializeField] GameObject _plane;
    [SerializeField] int _treeNb;

    [SerializeField] Color _colorMin;
    [SerializeField] Color _colorMax;

    System.Random _random = new();

    Vector2 _planeSize2D;
    (Vector2, Vector2) _planeMinMax2D;
    float _treeSize2D;

    List<(Vector2, float)> _existingTreeSizes = new();


    void Start()
    {
        Mesh vPlaneMesh = _plane.GetComponent<MeshFilter>().mesh;
        _planeSize2D = new Vector2(vPlaneMesh.bounds.size.x, vPlaneMesh.bounds.size.z) * _plane.transform.localScale.x;
        _planeMinMax2D = (new Vector2(_plane.transform.position.x-_planeSize2D.x/2, _plane.transform.position.z-_planeSize2D.y/2), 
                          new Vector2(_plane.transform.position.x+_planeSize2D.x/2, _plane.transform.position.z+_planeSize2D.y/2));
        _treeSize2D = _tree.GetComponent<MeshFilter>().sharedMesh.bounds.size.x;

        for (int i = 0; i < _treeNb; i++)
            CreateRectangle();
    }

    void CreateRectangle()
    {
        float v2DScale = _random.Next(1, (int)_planeSize2D.x / 2);

        int vMinPosX = (int)(_planeMinMax2D.Item1.x + v2DScale * _treeSize2D);
        int vMaxPosX = (int)(_planeMinMax2D.Item2.x - v2DScale * _treeSize2D);
        int vMinPosY = (int)(_planeMinMax2D.Item1.y + v2DScale * _treeSize2D);
        int vMaxPosY = (int)(_planeMinMax2D.Item2.y - v2DScale * _treeSize2D);

        Vector2 vPosition = new Vector2(_random.Next(math.min(vMinPosX, vMaxPosX), math.max(vMinPosX, vMaxPosX)),
                                        _random.Next(math.min(vMinPosY, vMaxPosY), math.max(vMinPosY, vMaxPosY)));

        foreach ((Vector2, float) lTreeSize2D in _existingTreeSizes)
        {
            if (vPosition.x <= lTreeSize2D.Item1.x + lTreeSize2D.Item2
              && vPosition.x + v2DScale * _treeSize2D >= lTreeSize2D.Item1.x
              && vPosition.y <= lTreeSize2D.Item1.y + lTreeSize2D.Item2
              && vPosition.y + v2DScale * _treeSize2D >= lTreeSize2D.Item1.y)
            {
                CreateRectangle();
                return;
            }
        }

        _existingTreeSizes.Add((vPosition, v2DScale * _treeSize2D));
        GameObject vNewTree = Instantiate(_tree, new Vector3(vPosition.x, 0, vPosition.y), Quaternion.identity);
        vNewTree.transform.localScale = new Vector3(v2DScale, v2DScale, v2DScale);
        Color vColor = Color.Lerp(_colorMin,_colorMax, (float)_random.Next(0,1000)/1000);
        vNewTree.GetComponent<MeshRenderer>().material= new Material(_tree.GetComponent<MeshRenderer>().sharedMaterial);
        vNewTree.GetComponent<MeshRenderer>().material.SetColor("_Color", vColor);
    }
}
