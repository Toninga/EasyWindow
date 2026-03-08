# EasyWindow



## Description

Modify your ScriptableObjects with custom Windows providing an authentic Unity-feel.



## Sample

Window > Easy Window Sample
Open this window and get a look at how Unity-like it feels !

## Installation


Make sure you have \[Git](https://git-scm.com/install/windows) (not a third party interface) installed on your computer.
Open your package manager and select "Install from git URL", then paste the Following link :

```
https://github.com/Toninga/UIToolkit\\\_Template.git
```

Or simply clone the git repository to your project.



## Usage

You can make your new custom window by following these steps :

* Create a new script in an editor folder
* Paste the following code inside your script

```
using UnityEditor;

public class YOURCLASSNAME : EasyWindow
{
	new public const string DefaultWindowName = "YOUR WINDOW NAME";
	\[MenuItem("Window/YOUR WINDOW MENU ITEM")]
	public static void NewWindow()
	{
		MakeWindow<YOURCLASSNAME>();
	}
}
```

* Right-click in your Project tab and go to Create > ScriptableObjects > EasyWindowProfile
* Select your script asset, and assign the EasyWindow VisualTree asset, then either the DarkTheme or Sakura USS, and finally your WindowProfile
