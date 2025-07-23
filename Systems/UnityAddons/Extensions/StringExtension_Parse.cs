using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using uParser;

public static class StringExtension_Parse
{
    static Dictionary<Type, ParsableType> ids;
    enum ParsableType
    {
        //Common
        String, Int, Float, Boolean, Quaternion, Vector2, Vector3, Vector4, Color,
        Vector2Int, Vector3Int, Matrix4x4,
        //Reference
        GameObject, Component, Material, Sprite, Texture2D,
        //Variants
        Char, Double, Decimal, Long, Byte, Short, Color32, SByte, UInt, ULong, UShort,
        //Others
        Scene, Rect, Hash128
    }

    public static T Parse<T>(this string input)
    {
        if (input.ToLower() == "default")
            return default;

        if (ids == null)
        {
            ids = new Dictionary<Type, ParsableType>()
            {
                { typeof(string), ParsableType.String },
                { typeof(int), ParsableType.Int },
                { typeof(float), ParsableType.Float },
                { typeof(bool), ParsableType.Boolean },
                { typeof(Quaternion), ParsableType.Quaternion },
                { typeof(Vector2), ParsableType.Vector2 },
                { typeof(Vector3), ParsableType.Vector3 },
                { typeof(Vector4), ParsableType.Vector4 },
                { typeof(Color), ParsableType.Color },
                { typeof(Vector2Int), ParsableType.Vector2Int },
                { typeof(Vector3Int), ParsableType.Vector3Int },
                { typeof(Matrix4x4), ParsableType.Matrix4x4 },
                { typeof(GameObject), ParsableType.GameObject },
                { typeof(Component), ParsableType.Component },
                { typeof(Material), ParsableType.Material },
                { typeof(Sprite), ParsableType.Sprite },
                { typeof(Texture2D), ParsableType.Texture2D },
                { typeof(char), ParsableType.Char },
                { typeof(double), ParsableType.Double },
                { typeof(decimal), ParsableType.Decimal },
                { typeof(long), ParsableType.Long },
                { typeof(byte), ParsableType.Byte },
                { typeof(short), ParsableType.Short },
                { typeof(Color32), ParsableType.Color32 },
                { typeof(sbyte), ParsableType.SByte },
                { typeof(uint), ParsableType.UInt },
                { typeof(ulong), ParsableType.ULong },
                { typeof(ushort), ParsableType.UShort },
                { typeof(Scene), ParsableType.Scene },
                { typeof(Rect), ParsableType.Rect },
                { typeof(Hash128), ParsableType.Hash128 },
            };
        }

        switch (ids[typeof(T)])
        {
            case ParsableType.String:
                return (T)(object)input;
            case ParsableType.Int:
                return (T)(object)UnityParser.ParseInteger(input);
            case ParsableType.Float:
                return (T)(object)UnityParser.ParseFloat(input);
            case ParsableType.Boolean:
                return (T)(object)UnityParser.ParseBoolean(input);
            case ParsableType.Quaternion:
                return (T)(object)UnityParser.ParseQuaternion(input);
            case ParsableType.Vector2:
                return (T)(object)UnityParser.ParseVector2(input);
            case ParsableType.Vector3:
                return (T)(object)UnityParser.ParseVector3(input);
            case ParsableType.Vector4:
                return (T)(object)UnityParser.ParseVector4(input);
            case ParsableType.Color:
                return (T)(object)UnityParser.ParseColor(input);
            case ParsableType.Vector2Int:
                return (T)(object)UnityParser.ParseVector2Int(input);
            case ParsableType.Vector3Int:
                return (T)(object)UnityParser.ParseVector3Int(input);
            case ParsableType.Matrix4x4:
                return (T)(object)UnityParser.ParseMatrix4x4(input);
            case ParsableType.GameObject:
                return (T)(object)UnityParser.ParseGameObject(input);
            case ParsableType.Component:
                return (T)(object)UnityParser.ParseComponent(input);
            case ParsableType.Material:
                return (T)(object)UnityParser.ParseMaterial(input);
            case ParsableType.Sprite:
                return (T)(object)UnityParser.ParseSprite(input);
            case ParsableType.Texture2D:
                return (T)(object)UnityParser.ParseTexture2D(input);
            case ParsableType.Char:
                return (T)(object)UnityParser.ParseChar(input);
            case ParsableType.Double:
                return (T)(object)UnityParser.ParseDouble(input);
            case ParsableType.Decimal:
                return (T)(object)UnityParser.ParseDecimal(input);
            case ParsableType.Long:
                return (T)(object)UnityParser.ParseLong(input);
            case ParsableType.Byte:
                return (T)(object)UnityParser.ParseByte(input);
            case ParsableType.Short:
                return (T)(object)UnityParser.ParseShort(input);
            case ParsableType.Color32:
                return (T)(object)UnityParser.ParseColor32(input);
            case ParsableType.SByte:
                return (T)(object)UnityParser.ParseSignedByte(input);
            case ParsableType.UInt:
                return (T)(object)UnityParser.ParseUnsignedInteger(input);
            case ParsableType.ULong:
                return (T)(object)UnityParser.ParseUnsignedLong(input);
            case ParsableType.UShort:
                return (T)(object)UnityParser.ParseUnsignedShort(input);
            case ParsableType.Scene:
                return (T)(object)UnityParser.ParseScene(input);
            case ParsableType.Rect:
                return (T)(object)UnityParser.ParseRect(input);
            case ParsableType.Hash128:
                return (T)(object)UnityParser.ParseHash128(input);
            default:
                Debug.LogError("Parser for type " + typeof(T).ToString() + " is not implemented.");
                return default;
        }
    }

    public static bool TryParse<T>(this string input, out T result)
    {
        if (input.ToLower() == "default")
        {
            result = default;
            return true;
        }

        if (ids == null)
        {
            ids = new Dictionary<Type, ParsableType>()
            {
                { typeof(string), ParsableType.String },
                { typeof(int), ParsableType.Int },
                { typeof(float), ParsableType.Float },
                { typeof(bool), ParsableType.Boolean },
                { typeof(Quaternion), ParsableType.Quaternion },
                { typeof(Vector2), ParsableType.Vector2 },
                { typeof(Vector3), ParsableType.Vector3 },
                { typeof(Vector4), ParsableType.Vector4 },
                { typeof(Color), ParsableType.Color },
                { typeof(Vector2Int), ParsableType.Vector2Int },
                { typeof(Vector3Int), ParsableType.Vector3Int },
                { typeof(Matrix4x4), ParsableType.Matrix4x4 },
                { typeof(GameObject), ParsableType.GameObject },
                { typeof(Component), ParsableType.Component },
                { typeof(Material), ParsableType.Material },
                { typeof(Sprite), ParsableType.Sprite },
                { typeof(Texture2D), ParsableType.Texture2D },
                { typeof(char), ParsableType.Char },
                { typeof(double), ParsableType.Double },
                { typeof(decimal), ParsableType.Decimal },
                { typeof(long), ParsableType.Long },
                { typeof(byte), ParsableType.Byte },
                { typeof(short), ParsableType.Short },
                { typeof(Color32), ParsableType.Color32 },
                { typeof(sbyte), ParsableType.SByte },
                { typeof(uint), ParsableType.UInt },
                { typeof(ulong), ParsableType.ULong },
                { typeof(ushort), ParsableType.UShort },
                { typeof(Scene), ParsableType.Scene },
                { typeof(Rect), ParsableType.Rect },
                { typeof(Hash128), ParsableType.Hash128 },
            };
        }

        bool success = false;
        switch (ids[typeof(T)])
        {
            case ParsableType.String:
                result = (T)(object)input;
                return true;
            case ParsableType.Int:
                result = (T)(object)UnityParser.ParseInteger(input);
                return true;
            case ParsableType.Float:
                result = (T)(object)UnityParser.ParseFloat(input);
                return true;
            case ParsableType.Boolean:
                success = UnityParser.TryParseBoolean(input, out bool n0);
                result = (T)(object)n0;
                return success;
            case ParsableType.Quaternion:
                result = (T)(object)UnityParser.ParseQuaternion(input);
                return true;
            case ParsableType.Vector2:
                success = UnityParser.TryParseVector2(input, out Vector2 n1);
                result = (T)(object)n1;
                return success;
            case ParsableType.Vector3:
                success = UnityParser.TryParseVector3(input, out Vector3 n2);
                result = (T)(object)n2;
                return success;
            case ParsableType.Vector4:
                result = (T)(object)UnityParser.ParseVector4(input);
                return true;
            case ParsableType.Color:
                result = (T)(object)UnityParser.ParseColor(input);
                return true;
            case ParsableType.Vector2Int:
                result = (T)(object)UnityParser.ParseVector2Int(input);
                return true;
            case ParsableType.Vector3Int:
                result = (T)(object)UnityParser.ParseVector3Int(input);
                return true;
            case ParsableType.Matrix4x4:
                result = (T)(object)UnityParser.ParseMatrix4x4(input);
                return true;
            case ParsableType.GameObject:
                result = (T)(object)UnityParser.ParseGameObject(input);
                return true;
            case ParsableType.Component:
                result = (T)(object)UnityParser.ParseComponent(input);
                return true;
            case ParsableType.Material:
                result = (T)(object)UnityParser.ParseMaterial(input);
                return true;
            case ParsableType.Sprite:
                result = (T)(object)UnityParser.ParseSprite(input);
                return true;
            case ParsableType.Texture2D:
                result = (T)(object)UnityParser.ParseTexture2D(input);
                return true;
            case ParsableType.Char:
                result = (T)(object)UnityParser.ParseChar(input);
                return true;
            case ParsableType.Double:
                result = (T)(object)UnityParser.ParseDouble(input);
                return true;
            case ParsableType.Decimal:
                result = (T)(object)UnityParser.ParseDecimal(input);
                return true;
            case ParsableType.Long:
                result = (T)(object)UnityParser.ParseLong(input);
                return true;
            case ParsableType.Byte:
                result = (T)(object)UnityParser.ParseByte(input);
                return true;
            case ParsableType.Short:
                result = (T)(object)UnityParser.ParseShort(input);
                return true;
            case ParsableType.Color32:
                result = (T)(object)UnityParser.ParseColor32(input);
                return true;
            case ParsableType.SByte:
                result = (T)(object)UnityParser.ParseSignedByte(input);
                return true;
            case ParsableType.UInt:
                result = (T)(object)UnityParser.ParseUnsignedInteger(input);
                return true;
            case ParsableType.ULong:
                result = (T)(object)UnityParser.ParseUnsignedLong(input);
                return true;
            case ParsableType.UShort:
                result = (T)(object)UnityParser.ParseUnsignedShort(input);
                return true;
            case ParsableType.Scene:
                result = (T)(object)UnityParser.ParseScene(input);
                return true;
            case ParsableType.Rect:
                result = (T)(object)UnityParser.ParseRect(input);
                return true;
            case ParsableType.Hash128:
                result = (T)(object)UnityParser.ParseHash128(input);
                return true;
            default:
                result = default;
                return false;
        }
    }
}
