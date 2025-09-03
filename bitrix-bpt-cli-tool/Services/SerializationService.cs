using System.Text.RegularExpressions;
using PhpSerializerNET;

namespace BptTool.Services;

/// <summary>
/// Сервис для работы с PHP-сериализацией данных
/// </summary>
public static class SerializationService
{
    /// <summary>
    /// Проверяет, похож ли текст на PHP-сериализованные данные
    /// </summary>
    /// <param name="text">Текст для проверки</param>
    /// <returns>True, если текст похож на PHP-сериализованные данные</returns>
    public static bool LooksLikePhpSerialized(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        // Берем первые 200 символов для анализа
        string sample = text.Length > 200 ? text[..200] : text;

        // Проверяем начало строки на типичные PHP-сериализованные префиксы
        if (sample.StartsWith("a:") || sample.StartsWith("s:") || sample.StartsWith("i:") ||
            sample.StartsWith("b:") || sample.StartsWith("N;") || sample.StartsWith("O:") ||
            sample.StartsWith("d:"))
        {
            return true;
        }

        // Проверяем регулярным выражением
        var phpSerializedPattern = new Regex(@"^(a|s|i|b|d|O):\d+[:;{]", RegexOptions.Compiled);
        return phpSerializedPattern.IsMatch(sample);
    }

    /// <summary>
    /// Десериализует PHP-данные в объект
    /// </summary>
    /// <param name="serializedData">Сериализованные данные</param>
    /// <returns>Десериализованный объект</returns>
    public static object DeserializePhp(string serializedData)
    {
        return PhpSerialization.Deserialize(serializedData) ?? throw new InvalidDataException("Не удалось десериализовать данные");
    }

    /// <summary>
    /// Выводит объект в виде дерева с отступами
    /// </summary>
    /// <param name="node">Объект для вывода</param>
    /// <param name="level">Уровень вложенности (для отступов)</param>
    /// <param name="writer">Writer для вывода</param>
    public static void DumpPretty(object node, int level, TextWriter writer)
    {
        string indent = new string(' ', level * 2);

        switch (node)
        {
            case IDictionary<object, object> dictionary:
                foreach (var kvp in dictionary)
                {
                    writer.WriteLine($"{indent}{kvp.Key}:");
                    DumpPretty(kvp.Value, level + 1, writer);
                }
                break;

            case IList<object> list:
                for (int i = 0; i < list.Count; i++)
                {
                    writer.WriteLine($"{indent}[{i}]:");
                    DumpPretty(list[i], level + 1, writer);
                }
                break;

            default:
                writer.WriteLine($"{indent}{node}");
                break;
        }
    }
}
