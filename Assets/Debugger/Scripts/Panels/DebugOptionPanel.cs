using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[Serializable]
public class DebugOptionPanel : IDebugPanel
{
    [SerializeField]
    Text m_prefabTitle;

    [SerializeField]
    Button m_prefabButton;

    [SerializeField]
    GameObject m_prefabInput;

    [SerializeField]
    RectTransform m_prefabCatalog;

    [SerializeField]
    RectTransform m_rootContainer;

    class MethodInfoData
    {
        public int catalogOrder;
        public string catalog;
        public string name;
        public int order;
        public MethodInfo info;
        public DebugMethodAttribute.ParamType paramType;
        public string paramValue;
        public PropertyInfo tag;
    }

    class ClassInfoData
    {
        public MethodInfo checker;
        public GameObject title;
        public GameObject container;
    }

    List<MethodInfoData> m_debugMethodList;
    Dictionary<string, ClassInfoData> m_debugClassList;

    public IEnumerator ProcessInit()
    {
        m_debugMethodList = new List<MethodInfoData>();
        m_debugClassList = new Dictionary<string, ClassInfoData>();

        var attMethod = typeof(DebugMethodAttribute);
        var attChecker = typeof(DebugCheckerAttribute);
        var attClass = typeof(DebugClassAttribute);
        Assembly asm = Assembly.GetAssembly(attMethod);
        var types = asm.GetTypes();

        yield return null;

        foreach (var t in types)
        {
            var methods = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var classOrder = 0;
            if (t.IsDefined(attClass))
            {
                var a = t.GetCustomAttribute<DebugClassAttribute>();
                classOrder = a.order;
            }

            foreach (var m in methods)
            {
                if (m.IsDefined(attChecker))
                {
                    var a = m.GetCustomAttribute<DebugCheckerAttribute>();
                    var catalog = string.IsNullOrEmpty(a.catalog) ? t.ToString() : a.catalog;
                    m_debugClassList[catalog] = new ClassInfoData() { checker = m };
                }
            }

            foreach (var m in methods)
            {
                if (m.IsDefined(attMethod))
                {
                    var data = new MethodInfoData();
                    var a = m.GetCustomAttribute<DebugMethodAttribute>();
                    data.catalogOrder = classOrder;
                    data.catalog = string.IsNullOrEmpty(a.catalog) ? t.ToString() : a.catalog;
                    data.name = string.IsNullOrEmpty(a.altName) ? AutoName(m.Name) : a.altName;
                    data.order = a.order;
                    data.info = m;
                    data.paramType = a.paramType;
                    data.paramValue = a.paramValue;
                    data.tag = t.GetProperty(m.Name + "Tag");
                    m_debugMethodList.Add(data);
                }
            }
        }

        yield return null;

        m_debugMethodList.Sort((a, b) =>
        {
            if (a.catalogOrder != b.catalogOrder) return a.catalogOrder - b.catalogOrder;

            var c = string.CompareOrdinal(a.catalog, b.catalog);
            if (c != 0) return c;

            if (a.order != b.order) return a.order - b.order;

            return string.CompareOrdinal(a.name, b.name);
        });

        yield return null;
    }

    public void RefreshUI()
    {
        foreach (var data in m_debugClassList.Values)
        {
            var show = (bool)data.checker.Invoke(null, null);
            data.title.SetActive(show);
            data.container.SetActive(show);
        }
    }

    public void CreateUI()
    {
        var catalog = "";
        RectTransform catalogContainer = null;
        foreach (var data in m_debugMethodList)
        {
            if (catalog != data.catalog)
            {
                catalog = data.catalog;
                var title = Object.Instantiate(m_prefabTitle, m_rootContainer, false);
                title.gameObject.SetActive(true);
                title.name = "Title_" + catalog;
                title.text = catalog;

                catalogContainer = Object.Instantiate(m_prefabCatalog, m_rootContainer, false);
                catalogContainer.gameObject.SetActive(true);
                catalogContainer.name = "Content_" + catalog;

                if (m_debugClassList.ContainsKey(catalog))
                {
                    m_debugClassList[catalog].title = title.gameObject;
                    m_debugClassList[catalog].container = catalogContainer.gameObject;
                }
            }

            var btn = Object.Instantiate(m_prefabButton, catalogContainer, false);
            btn.gameObject.SetActive(true);
            btn.name = "Btn_" + data.name;

            var btnTxt = btn.GetComponentInChildren<Text>(true);
            var btnName = data.name;
            var tag = data.tag;
            RefreshButtonText(btnTxt, btnName, tag);

            if (data.paramType != DebugMethodAttribute.ParamType.None)
            {
                var input = Object.Instantiate(m_prefabInput, catalogContainer, false);
                input.gameObject.SetActive(true);
                input.name = "Ipu_" + data.name;
                var ipt = input.GetComponentInChildren<InputField>();
                if (ipt)
                {
                    ipt.text = data.paramValue;
                    btn.onClick.AddListener(() =>
                    {
                        data.info.Invoke(null, new object[] { AutoValue(data.paramType, ipt.text) });
                        RefreshButtonText(btnTxt, btnName, tag);
                    });
                }
                else
                {
                    btn.onClick.AddListener(() =>
                    {
                        data.info.Invoke(null, new object[] { AutoValue(data.paramType, data.paramValue) });
                        RefreshButtonText(btnTxt, btnName, tag);
                    });
                }
            }
            else
            {
                btn.onClick.AddListener(() =>
                {
                    data.info.Invoke(null, null);
                    RefreshButtonText(btnTxt, btnName, data.tag);
                });
            }
        }
    }

    void RefreshButtonText(Text btnTxt, string btnName, PropertyInfo btnTag)
    {
        if (btnTxt)
        {
            var tag = "";
            if (btnTag != null)
            {
                var v = btnTag.GetValue(null);
                if (v is bool)
                {
                    var tmp = ((bool)v) ? "on" : "off";
                    tag = $" <b>[{tmp}]</b>";
                }
                else
                {
                    tag = $" <b>[{v}]</b>";
                }

            }
            btnTxt.text = btnName + tag;
        }
    }


    string AutoName(string src)
    {
        src = Regex.Replace(src, "[A-Z]", " $0").Trim();
        if (src.StartsWith("Debug ") || src.StartsWith("debug ")) src = src.Remove(0, 6);
        return src;
    }

    object AutoValue(DebugMethodAttribute.ParamType type, string src)
    {
        switch (type)
        {
            case DebugMethodAttribute.ParamType.Float:
                {
                    float.TryParse(src, out float v);
                    return v;
                }
            case DebugMethodAttribute.ParamType.Int:
                {
                    int.TryParse(src, out int v);
                    return v;
                }
        }

        return src;
    }

    public IEnumerator ProcessAutoStart()
    {
        yield return null;
    }
}
