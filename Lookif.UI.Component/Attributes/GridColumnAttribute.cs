using System;

namespace Lookif.UI.Component.Attributes;
[AttributeUsage(AttributeTargets.Property)]
public class GridColumnAttribute: Attribute
{
    public string Width { get; set; }

    public GridColumnAttribute(string width)
    {
        Width = width;
    }
}
