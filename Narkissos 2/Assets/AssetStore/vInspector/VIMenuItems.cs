#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static VInspector.Libs.VUtils;
using static VInspector.Libs.VGUI;


namespace VInspector
{
    class VIMenuItems
    {
        public static bool scriptInspectorEnabled { get => !ScriptHasDefineDisabled(typeof(VIScriptComponentEditor)); set => SetDefineDisabledInScript(typeof(VIScriptComponentEditor), !value); }
        public static bool soInspectorEnabled { get => !ScriptHasDefineDisabled(typeof(VIScriptableObjectEditor)); set => SetDefineDisabledInScript(typeof(VIScriptableObjectEditor), !value); }
        public static bool staticInspectorEnabled { get => !ScriptHasDefineDisabled(typeof(VIScriptAssetEditor)); set => SetDefineDisabledInScript(typeof(VIScriptAssetEditor), !value); }
        public static bool resettableVariablesEnabled { get => !ScriptHasDefineDisabled(typeof(VIResettablePropDrawer)); set => SetDefineDisabledInScript(typeof(VIResettablePropDrawer), !value); }
        public static bool hideScriptFieldEnabled { get => EditorPrefs.GetBool("vInspector-hideScriptField", true); set => EditorPrefs.SetBool("vInspector-hideScriptField", value); }

        public static bool vInspectorDisabled { get => !scriptInspectorEnabled && !soInspectorEnabled && !staticInspectorEnabled && !resettableVariablesEnabled; set => scriptInspectorEnabled = soInspectorEnabled = staticInspectorEnabled = resettableVariablesEnabled = !value; }


        const string menuDir = "Tools/vInspector/";

        const string hideScriptField = menuDir + "Hide script field";
        const string resettableVariables = menuDir + "Resettable variables";
        const string staticInspector = menuDir + "Static inspector";

        const string disable = menuDir + "Disable vInspector";



        [MenuItem(resettableVariables, false, 1)] static void dadsaadsdadsas() => resettableVariablesEnabled = !resettableVariablesEnabled;
        [MenuItem(resettableVariables, true, 1)] static bool dadsadadsdasadsas() { UnityEditor.Menu.SetChecked(resettableVariables, resettableVariablesEnabled); return true; }

        [MenuItem(hideScriptField, false, 2)] static void dadsadadsas() => hideScriptFieldEnabled = !hideScriptFieldEnabled;
        [MenuItem(hideScriptField, true, 2)] static bool dadsaddasadsas() { UnityEditor.Menu.SetChecked(hideScriptField, hideScriptFieldEnabled); return true; }

        [MenuItem(staticInspector, false, 3)] static void dadsaadsdadsdasas() => staticInspectorEnabled = !staticInspectorEnabled;
        [MenuItem(staticInspector, true, 3)] static bool dadsadadsddsaasadsas() { UnityEditor.Menu.SetChecked(staticInspector, staticInspectorEnabled); return true; }


        [MenuItem(menuDir + "Join our Discord", false, 101)]
        static void dadsas() => Application.OpenURL("https://discord.gg/4dG9KsbspG");

        [MenuItem(menuDir + "Get the rest of our Editor Ehnancers with a discount", false, 102)]
        static void dadsadsas() => Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/editor-enhancers-bundle-251318?aid=1100lGLBn&pubref=menu");


        [MenuItem(disable, false, 1001)] static void dadsaaadsdsdadsdasas() => vInspectorDisabled = !vInspectorDisabled;
        [MenuItem(disable, true, 1001)] static bool dadsadaadsdsddsaasadsas() { UnityEditor.Menu.SetChecked(disable, vInspectorDisabled); return true; }



    }
}
#endif