using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIDocumentBinder : MonoBehaviour
{
    #region Fields

    public List<UIDocumentBindings> bindings;

    #endregion
    void Start()
    {
        Bind();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Bind()
    {
        foreach(UIDocumentBindings binding in bindings)
        {
            binding.Binder?.BindAll(binding.TargetDocument);
        }
    }

}

[System.Serializable]
public struct UIDocumentBindings
{
    public UIDocument TargetDocument;
    public UIBinder Binder;
}