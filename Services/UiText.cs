using System.Globalization;
using System.Resources;

namespace UM_Project.Services;

/// <summary>Loads UI strings from Resources/SharedResource.resx (EN + sq satellite).</summary>
public interface IUiText
{
    string this[string key] { get; }
    string Get(string key);
    string Format(string key, params object[] args);
}

public class UiText : IUiText
{
    private const string BaseName = "UM_Project.Resources.SharedResource";
    private static readonly ResourceManager ResourceManager = new(BaseName, typeof(UiText).Assembly);

    public string this[string key] => Get(key);

    public string Get(string key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;

        var culture = CultureInfo.CurrentUICulture;
        var value = ResourceManager.GetString(key, culture);
        if (value != null) return value;

        if (!culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase))
            value = ResourceManager.GetString(key, CultureInfo.GetCultureInfo("en"));

        return value ?? key;
    }

    public string Format(string key, params object[] args) =>
        string.Format(CultureInfo.CurrentUICulture, Get(key), args);
}
