using System.IO;

using AE.Core;

namespace ScreenBase.Data.Base;

public static class DataHelper
{
    public static void Save<T>(string path, T data)
        where T : class
    {
        File.WriteAllText(path, data.Serialize());
    }

    public static T Load<T>(string path)
        where T : class
    {
        try
        {
            var data = File.ReadAllText(path);
            return data.Deserialize<T>();
        }
        catch
        {
            return "".Deserialize<T>();
        }
    }

    public static T Clone<T>(object obj)
        where T : class
    {
        var data = obj.Serialize();
        return data.Deserialize<T>();
    }
}
