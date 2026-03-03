using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "UIBinder", menuName = "Scriptable Objects/UIBinder")]
public class UIBinder : ScriptableObject
{
    public List<UIBinding> bindings;

    public void BindAll(UIDocument target)
    {
        if (target == null)
            return;
        VisualElement root = target.rootVisualElement;
        if (root == null)
            return;

        foreach (var binding in bindings)
        {
            if (string.IsNullOrEmpty(binding.visualElementName))
                { continue; }

            VisualElement elm = root.Q(binding.visualElementName);
            TryBind(elm, GetActionFromBindingOption(binding.actionType));
        }
    }
    public bool TryBind(VisualElement elm, Action action)
    {
        if (elm == null)
            return false;

        if (elm is Button)
        {
            TryBindButton(elm as Button, action);
        }
        if (elm is Slider)
        {
            TryBindSlider(elm as Slider,evt => action());
        }

        return true;
    }
    void TryBindButton(Button elm, Action callback)
    {
        if (elm == null) return;
        elm.RegisterCallback<ClickEvent>(evt => callback());
    }
    void TryBindSlider(Slider elm, Action<float> callback)
    {
        if (elm == null) return;
        elm.RegisterValueChangedCallback<float>(evt => callback(evt.newValue));
    }


    public Action GetActionFromBindingOption(UIBindingOptions option)
    {
        if (option == UIBindingOptions.None)
            { return null; }

        return () => EventBus.Invoke(option.ToString());
    }
}



[System.Serializable]
public struct UIBinding
{
    public string visualElementName;
    public UIBindingOptions actionType;
}
