using System;
using System.IO;
using System.Reflection;

internal class ReflectionHelper
{
    public static void PrintPropertiesAndMethods(Type type, string filePath, int depth = 0)
    {
        using (var writer = new StreamWriter(filePath, true))
        {
            PrintPropertiesAndMethods(type, writer, depth);
        }
    }

    private static void PrintPropertiesAndMethods(Type type, StreamWriter writer, int depth)
    {
        if (depth > 3) // Limit recursion depth to avoid excessive output
            return;

        writer.WriteLine($"{new string(' ', depth * 2)}Properties and Methods of {type.Name}:");

        writer.WriteLine($"{new string(' ', depth * 2)}\nProperties:");
        foreach (var prop in type.GetProperties())
        {
            writer.WriteLine($"{new string(' ', depth * 2)}- {prop.Name} ({prop.PropertyType.Name})");
            if (!prop.PropertyType.IsPrimitive && prop.PropertyType != typeof(string))
            {
                PrintPropertiesAndMethods(prop.PropertyType, writer, depth + 1);
            }
        }

        writer.WriteLine($"{new string(' ', depth * 2)}\nMethods:");
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            writer.WriteLine($"{new string(' ', depth * 2)}- {method.Name} ({method.ReturnType.Name})");
            if (!method.ReturnType.IsPrimitive && method.ReturnType != typeof(string))
            {
                PrintPropertiesAndMethods(method.ReturnType, writer, depth + 1);
            }
        }
    }
}