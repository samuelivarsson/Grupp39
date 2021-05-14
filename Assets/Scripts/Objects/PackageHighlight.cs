using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageHighlight : MonoBehaviour, IHighlight
{
    [SerializeField] Renderer[] renderers;
    [SerializeField] Renderer[] tapeRenderers;
    
    string type;
    Material standardMat;
    Material highlightedMat;
    Material standardTapeMat;
    Material highlightedTapeMat;
    
    void Awake()
    {
        standardMat = renderers[0].material;
        highlightedMat = FindHighlightedMaterial(standardMat);
        standardTapeMat = tapeRenderers[0].material;
        highlightedTapeMat = FindHighlightedMaterial(standardTapeMat);
    }

    public void Highlight(bool highlight)
    {
        foreach (Renderer rend in renderers)
        {
            rend.material = highlight ? highlightedMat : standardMat;
        }
    }

    Material FindHighlightedMaterial(Material mat)
    {
        Material matTemp = new Material(mat);
        float offset = 0.15f;
        matTemp.SetColor("_Color", new Color(matTemp.color.r+offset, matTemp.color.g+offset, matTemp.color.b+offset, matTemp.color.a));
        return matTemp;
    }
}
