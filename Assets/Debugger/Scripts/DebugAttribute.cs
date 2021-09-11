using System;

[AttributeUsage(AttributeTargets.Class)]
public class DebugClassAttribute : Attribute
{
    public int order { get; private set; }

    public DebugClassAttribute(int order = 0)
    {
        this.order = order;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class DebugMethodAttribute : Attribute
{
    public enum ParamType
    {
        None,
        Int,
        Float,
        String,
    }

    public string catalog { get; private set; }
    public string altName { get; private set; }
    public int order { get; private set; }
    public ParamType paramType { get; private set; }
    public string paramValue { get; private set; }

    public DebugMethodAttribute(string altName = null, string catalog = null, int order = 0)
    {
        Init(ParamType.None, "", altName, catalog, order);
    }

    public DebugMethodAttribute(ParamType paramType, string paramValue, string altName = null, string catalog = null, int order = 0)
    {
        Init(paramType, paramValue, altName, catalog, order);
    }

    public void Init(ParamType paramType = ParamType.None, string paramValue = "", string altName = null, string catalog = null, int order = 0)
    {
        this.paramType = paramType;
        this.paramValue = paramValue;
        this.altName = altName;
        this.catalog = catalog;
        this.order = order;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class DebugCheckerAttribute : Attribute
{
    public string catalog { get; private set; }
    public DebugCheckerAttribute(string catalog = null)
    {
        this.catalog = catalog;
    }
}