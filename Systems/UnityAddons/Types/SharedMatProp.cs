using System;
using System.Collections.Generic;
using UnityEngine;

public struct SharedMatProp : IEquatable<SharedMatProp>
{
    static Dictionary<Component, SharedMatProp> holders;
    static Dictionary<SharedMatProp, Component> priority;
    Material material;
    string property;

    public SharedMatProp(Material material, string property)
    {
        this.material = material;
        this.property = property;
    }

    public override bool Equals(object other)
    {
        if (!(other is SharedMatProp)) return false;
        return Equals((SharedMatProp)other);
    }

    public bool Equals(SharedMatProp other)
    {
        return (material == other.material)
            && (property == other.property);
    }

    public override int GetHashCode()
    {
        return material.GetHashCode() * 31 + property.GetHashCode();
    }

    public static bool operator ==(SharedMatProp o1, SharedMatProp o2)
    {
        return o1.Equals(o2);
    }

    public static bool operator !=(SharedMatProp o1, SharedMatProp o2)
    {
        return !o1.Equals(o2);
    }

    public void PrioritizeComponent(Component component, bool replace = true)
    {
        priority = priority.CreateIfNull();
        if (!priority.ContainsKey(this))
            priority.Add(this, component);
        else if (replace) priority[this] = component;
    }
    
    public void DeprioritizeComponent(Component component)
    {
        if (priority.SmartGetValue(this, out Component recorded) && (recorded == component))
            priority.Remove(this);
    }

    public bool ComponentCanAct(Component component)
    {
        if (priority.SmartGetValue(this, out Component recorded) && (recorded != component))
            return false;
        else return true;
    }

    public static void ProcessPriority(Component component, Material material, string property,
        bool prioritize, bool replace = true)
    {
        if (prioritize) Prioritize(component, material, property, replace);
        else Deprioritize(component);
    }

    public static void Prioritize(Component component, Material material, string property, bool replace = true)
    {
        holders = holders.CreateIfNull();
        SharedMatProp matProp = new SharedMatProp(material, property);
        holders.Set(component, matProp);
        matProp.PrioritizeComponent(component, replace);
    }

    public static void Deprioritize(Component component)
    {
        if (holders.SmartGetValue(component, out SharedMatProp matProp))
            matProp.DeprioritizeComponent(component);
    }

    public static bool CanAct(Component component)
    {
        if (holders.SmartGetValue(component, out SharedMatProp matProp))
            return matProp.ComponentCanAct(component);
        else return true;
    }
}
