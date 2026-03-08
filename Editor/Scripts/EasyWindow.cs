using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
public class EasyWindow : EditorWindow
{
    #region Asset Parameters
    // INSPECTOR PARAMETERS
    public VisualTreeAsset visualTree;
    public StyleSheet styleSheet;
    public EasyWindowProfile Profile; 
    #endregion

    #region Window Parameters
    // WINDOW PARAMETERS

    public List<ScriptableObject> MenuTargets => Profile.MenuTargets;

    public const string DefaultWindowName = "Easy Window";
    public const string DefaultWindowMenuItemName = "Easy Window";
    public string WindowName
    {
        get => _windowName;
        set
        {
            _windowName = value;
            this.name = value;
        }
    }
    private string _windowName = DefaultWindowName;

    public static Vector2 MinSize = new(1000, 600);
    public static Vector2 MaxSize = new(1920, 1080);
    public static bool HideDefaultMenu = false;
    #endregion

    #region UI Builder References
    // UI ELEMENTS NAMES
    protected const string MenuButtonGroup = "Menus";
    protected const string MenuContentGroup = "MenuContent";
    protected const string DefaultMenuButton = "DefaultMenu";
    protected const string DefaultMenuButtonLabel = "DefaultMenuLabel";
    protected const string DefaultMenuContent = "DefaultMenuContent";
    protected const string DefaultMenuContent_Title = "DefaultMenuContent_Title";
    protected const string DefaultMenuContent_Subtitle = "DefaultMenuContent_Subtitle";
    protected const string DefaultMenuContent_Label = "DefaultMenuContent_Label";
    protected const string DefaultMenuContent_TextField = "DefaultMenuContent_TextField";

    // UI CLASS NAMES
    protected const string Section = "section";
    protected const string MenuButton = "menuButton";
    protected const string MenuButtonLabel = "menuButtonLabel";
    protected const string Title = "title";
    protected const string Subtitle = "subtitle";
    protected const string Label = "label";
    protected const string Field = "field";
    #endregion

    #region Private Variables
    // PRIVATE VARIABLES
    // string is tab name, VisualElement is menuContent
    private Dictionary<string, (VisualElement button, VisualElement content)> _tabs = new();
    private string _currentMenu = DefaultWindowName; 
    #endregion

    #region Methods

    public static void MakeWindow<T>() where T : EasyWindow
    {
        var win = GetWindow<T>();
        win.name = GetDefaultWindowName(win);

        win.minSize = MinSize;
        win.maxSize = MaxSize;
    }
    public static string GetDefaultWindowName(EasyWindow window)
    {
        return window.GetType().GetField("DefaultWindowName").GetValue(window) as string;
    }
    /// <summary>
    /// Called when the window is created
    /// </summary>
    public void CreateGUI()
    {
        // Setup the visual tree
        if (visualTree != null)
        {
            // Add the UI layout
            VisualElement root = visualTree.CloneTree();
            rootVisualElement.Add(root);

            // Add the selected style sheet
            if (styleSheet != null)
            {
                //rootVisualElement.styleSheets.Clear();
                rootVisualElement.styleSheets.Add(styleSheet);
            }

            // Setup the default menu
            SetupDefaultMenu();
            if (HideDefaultMenu) TryHideMenu(DefaultWindowName);

            if (Profile != null)
            {
                // Add the menus based on scriptable objects
                foreach (var target in MenuTargets)
                {
                    AddMenu(target.name, target);
                }
            }

        }
    }
    private void SetupDefaultMenu()
    {
        var button = rootVisualElement.Q<Button>(DefaultMenuButton);
        button.clicked += () => EnableMenu(DefaultWindowName);
        var content = rootVisualElement.Q<VisualElement>(DefaultMenuContent);
        content.Clear();

        content.Add(MakeTitle(GetDefaultWindowName(this)));
        content.Add(MakeSubtitle("Add menus from ScriptableObjects"));
        content.Add(MakeObjectField("Menu profile", SetProfile, typeof(EasyWindowProfile), Profile));
        content.Add(Space());

        content.Add(MakeLabel("Currently added menus : "));
        foreach (var target in MenuTargets)
        {
            var temp = MakeObjectField(target.name, UnregisterMenu, typeof(ScriptableObject), target);
            content.Add(temp);
        }

        // Allow adding a new menu
        content.Add(Space());
        var objField = MakeObjectField("Select and add a new menu", RegisterMenu, typeof(ScriptableObject));
        content.Add(objField);

        if (button != null && content != null)
        {
            if (!_tabs.ContainsKey(DefaultWindowName))
                _tabs.Add(DefaultWindowName, (button, content));
            else
                _tabs[DefaultWindowName] = (button, content);
        }

        content.Add(Space());
        content.Add(MakeSubtitle("How to use"));
        content.Add(MakeLabel("<color=white>Menu Profile</color> : The menu profile stores all the menus that should get displayed in this window"));
        content.Add(MakeLabel("<color=white>Remove a menu</color> : The added menus can be deleted by selecting their field and pressing Suppr"));
        content.Add(MakeLabel("<color=white>Add a menu</color> : You can add a ScriptableObject-based menu by selecting your desired ScriptableObject in the last object field."));
        content.Add(Space());
        content.Add(MakeLabel("<color=white>How to prepare my ScriptableObject for menuing ?</color>"));
        content.Add(MakeLabel("Your scriptable object should use public fields for values that you want to expose."));
        content.Add(MakeLabel("Properties are voluntarily not supported."));
        content.Add(MakeLabel("This window supports the following types : int, float, string, bool, enum."));
        content.Add(Space());
        content.Add(MakeLabel("<color=white>How to make my own window ?</color>"));
        content.Add(MakeLabel("Have a look at the samples. You can inherit from EasyWindow, and use EasyWindow's visual tree and stylesheet."));
        content.Add(MakeLabel("Simply assign them in your mono script's default fields (in the inspector, after selecting you custom window script)"));
    }
    public void RegisterMenu(ChangeEvent<Object> evt)
    {
        if (evt.currentTarget is ObjectField elm && evt.newValue is ScriptableObject obj)
        {
            if (MenuTargets.Contains(obj)) return;
            elm.value = null;
            MenuTargets.Add(obj);
            AddMenu(obj.name, obj);
            SetupDefaultMenu();
            rootVisualElement.MarkDirtyRepaint();
        }
    }

    public void UnregisterMenu(ChangeEvent<Object> evt)
    {
        if (evt.currentTarget is ObjectField elm && evt.previousValue is ScriptableObject obj && evt.newValue == null)
        {
            Debug.Log("3");
            elm.value = null;
            MenuTargets.Remove(obj);
            RemoveMenu(obj.name);
            SetupDefaultMenu();
            rootVisualElement.MarkDirtyRepaint();
        }
    }
    public void SetProfile(ChangeEvent<Object> evt)
    {
        if (evt.newValue is EasyWindowProfile profile)
        {
            Profile = profile;
        }
    }

    /// <summary>
    /// Adds all the parameters of a SO to a specified menu
    /// </summary>
    /// <param name="menuName"></param>
    /// <param name="target"></param>
    protected void AddParametersFromScriptableObject(string menuName, ScriptableObject target)
    {
        foreach (var member in target.GetType().GetMembers())
        {
            AddParameterToMenu(menuName, member.Name, new SerializedObject(target), member);
        }
    }

    public bool TryHideMenu(string menuName)
    {
        if (!TryGetButton(menuName, out var button)) return false;
        if (!TryGetContent(menuName, out var content)) return false;

        button.style.display = DisplayStyle.None;
        content.style.display = DisplayStyle.None;
        return true;
    }
    public void RemoveMenu(string menuName)
    {
        if (_tabs.ContainsKey(menuName))
        {
            if (!TryGetButton(menuName, out var button)) return;
            if (!TryGetContent(menuName, out var content)) return;

            // Get the roots on which to add the menu
            VisualElement menuButtonGroup = rootVisualElement.Q(MenuButtonGroup);
            if (menuButtonGroup == null) return;
            VisualElement menuContentGroup = rootVisualElement.Q(MenuContentGroup);
            if (menuContentGroup == null) return;

            // Add the menu to the UI
            menuButtonGroup.Remove(button);
            menuContentGroup.Remove(content);
            _tabs.Remove(menuName);
        }
    }
    public void AddMenu(string menuName, ScriptableObject linkedObject = null)
    {
        // Generate the menu's content
        VisualElement content;
        if (linkedObject == null)
        {
            if (!TryMakeMenuContent(menuName, out content)) return;
        }
        else
        {
            if (!TryMakeMenuContent(menuName, linkedObject, out content)) return;
        }
        content.style.display = DisplayStyle.None;

        // Generate the menu's button
        if (!TryMakeMenuButton(menuName, out var button)) return;
        button.clicked += () => EnableMenu(menuName);

        // Get the roots on which to add the menu
        VisualElement menuButtonGroup = rootVisualElement.Q(MenuButtonGroup);
        if (menuButtonGroup == null) return;
        VisualElement menuContentGroup = rootVisualElement.Q(MenuContentGroup);
        if (menuContentGroup == null) return;

        // Add the menu to the UI
        menuButtonGroup.Add(button);
        menuContentGroup.Add(content);
        _tabs.Add(menuName, (button, content));
    }
    protected void EnableMenu(string menuName)
    {
        if (!TryGetContent(menuName, out var content)) return;

        DisableCurrentMenu();

        content.style.display = DisplayStyle.Flex;
        _currentMenu = menuName;
    }

    private void DisableCurrentMenu()
    {
        if (!string.IsNullOrEmpty(_currentMenu))
            if (_tabs.ContainsKey(_currentMenu))
                _tabs[_currentMenu].content.style.display = DisplayStyle.None;
    }

    protected bool TryMakeMenuButton(string menuName, out Button result)
    {
        result = null;
        Button referenceMenuButton = rootVisualElement.Q<Button>(DefaultMenuButton);
        if (referenceMenuButton == null) return false;

        Label referenceMenuLabel = rootVisualElement.Q<Label>(DefaultMenuButtonLabel);
        if (referenceMenuButton == null) return false;

        Button newButton = new Button();
        ReplicateStyle(newButton, referenceMenuButton);

        Label newLabel = new Label();
        ReplicateStyle(newLabel, referenceMenuLabel);

        newLabel.text = menuName;
        newButton.Add(newLabel);

        result = newButton;
        return true;
    }

    protected bool TryMakeMenuContent(string menuName, out VisualElement result)
    {
        result = new();
        result.AddToClassList(Section);
        result.style.flexGrow = 1;

        Label title = new();
        title.AddToClassList(Title);
        title.text = menuName;

        result.Add(title);
        return true;
    }

    protected bool TryMakeMenuContent(string menuName, ScriptableObject content, out VisualElement result)
    {
        if (TryMakeMenuContent(menuName, out result))
        {
            var obj = MakeObjectField("Target ScriptableObject", null, null, content);
            obj.SetEnabled(false);
            result.Add(obj);
            result.Add(Space());
            foreach (var member in content.GetType().GetMembers())
            {
                var param = MakeParameter(member.Name, new SerializedObject(content), member);
                if (param != null) result.Add(param);
            }
        }
        return true;
    }


    protected bool TryGetContent(string menuName, out VisualElement menuContent)
    {
        if (_tabs.ContainsKey(menuName))
        {
            menuContent = _tabs[menuName].content;
            return true;
        }
        menuContent = null;
        return false;
    }
    protected bool TryGetButton(string menuName, out VisualElement menuButton)
    {
        if (_tabs.ContainsKey(menuName))
        {
            menuButton = _tabs[menuName].button;
            return true;
        }
        menuButton = null;
        return false;
    }

    protected void ReplicateStyle(VisualElement target, VisualElement reference)
    {
        if (reference == null || target == null) return;
        foreach (var newClass in reference.GetClasses())
        {
            target.AddToClassList(newClass);
        }
    }

    protected bool AddParameterToMenu(string menuName, string parameterName, SerializedObject target, MemberInfo path)
    {
        if (!TryGetContent(menuName, out VisualElement content))
            return false;

        VisualElement parameter = MakeParameter(parameterName, target, path);

        if (parameter != null)
        {
            content.Add(parameter);
        }
        return false;
    }

    private VisualElement MakeParameter(string parameterName, SerializedObject target, MemberInfo path)
    {
        VisualElement parameter = null;
        if (path.MemberType == MemberTypes.Field)
        {
            FieldInfo info = (FieldInfo)path;
            if (info.FieldType == typeof(int)) parameter = MakeIntField(parameterName, target, info.Name);
            if (info.FieldType == typeof(string)) parameter = MakeTextField(parameterName, target, info.Name);
            if (info.FieldType == typeof(float)) parameter = MakeFloatField(parameterName, target, info.Name);
            if (info.FieldType == typeof(bool)) parameter = MakeBoolField(parameterName, target, info.Name);
            if (typeof(System.Enum).IsAssignableFrom(info.FieldType)) parameter = MakeEnumField(parameterName, target, info.Name);
        }

        return parameter;
    }

    protected Label Space() => MakeLabel("", Label);
    protected Label MakeTitle(string parameterName) => MakeLabel(parameterName, Title);
    protected Label MakeSubtitle(string parameterName) => MakeLabel(parameterName, Subtitle);
    protected Label MakeLabel(string parameterName = "", string classStyle=Label)
    {
        var elm = new Label(parameterName);
        elm.AddToClassList(classStyle);
        return elm;
    }
    protected T MakeBindableField<T>(T elm, SerializedObject target, string path) where T : BindableElement
    {
        elm.bindingPath = path;
        elm.Bind(target);
        elm.AddToClassList(Label);
        foreach (var child in elm.Children())
        {
            child.AddToClassList(Label);
            if (!child.GetClasses().Contains("unity-label"))
                child.AddToClassList(Field);
        }
        return elm;
    }
    
    protected EnumField MakeEnumField(string parameterName, SerializedObject target, string path) => MakeBindableField(new EnumField(parameterName), target, path);
    protected SliderInt MakeIntSlider(string parameterName, SerializedObject target, string path) => MakeBindableField(new SliderInt(parameterName, 0, 100), target, path);
    protected IntegerField MakeIntField(string parameterName, SerializedObject target, string path) => MakeBindableField(new IntegerField(parameterName), target, path);
    protected TextField MakeTextField(string parameterName, SerializedObject target, string path) => MakeBindableField(new TextField(parameterName), target, path);
    protected Toggle MakeBoolField(string parameterName, SerializedObject target, string path) => MakeBindableField(new Toggle(parameterName), target, path);
    protected FloatField MakeFloatField(string parameterName, SerializedObject target, string path) => MakeBindableField(new FloatField(parameterName), target, path);
    protected ObjectField MakeObjectField(string parameterName, EventCallback<ChangeEvent<Object>> callback = null, System.Type targetType = null, Object defaultValue = null)
    {
        var elm = new ObjectField(parameterName);
        if (targetType != null) elm.objectType = targetType;
        if (defaultValue != null) elm.value = defaultValue;
        if (callback != null) elm.RegisterValueChangedCallback(callback);
        ApplyStyle(elm);

        return elm;
    }
    protected void ApplyStyle(VisualElement elm)
    {
        if (!elm.GetClasses().Contains("unity-image"))
            elm.AddToClassList(Label);
        if (elm.GetClasses().Contains("unity-object-field__input"))
            elm.AddToClassList(Field);
        if (elm.GetClasses().Contains("unity-object-field-display__label"))
        {
            elm.RemoveFromClassList("unity-object-field-display__label");
        }
        foreach (var child in elm.Children())
        {
            ApplyStyle(child);
        }
    }
    #endregion
}
