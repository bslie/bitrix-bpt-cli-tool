using BptTool.Models;

namespace BptTool.Models;

/// <summary>
/// Базовые опции командной строки
/// </summary>
public abstract class BaseCommandOptions
{
    /// <summary>
    /// Входной файл или "-" для stdin
    /// </summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Альтернативный способ указания входного файла
    /// </summary>
    public string? InPath { get; set; }

    /// <summary>
    /// Получает окончательный входной путь
    /// </summary>
    public string GetInputPath() => InPath ?? Input;
}

/// <summary>
/// Опции для команды dump
/// </summary>
public class DumpOptions : BaseCommandOptions
{
    /// <summary>
    /// Выходной файл или "-" для stdout
    /// </summary>
    public string? Output { get; set; }

    /// <summary>
    /// Альтернативный способ указания выходного файла
    /// </summary>
    public string? OutPath { get; set; }

    /// <summary>
    /// Режим красивого вывода (древо структуры)
    /// </summary>
    public bool Pretty { get; set; }

    /// <summary>
    /// Получает окончательный выходной путь
    /// </summary>
    public string GetOutputPath() => OutPath ?? Output ?? "-";
}

/// <summary>
/// Опции для команды build
/// </summary>
public class BuildOptions : BaseCommandOptions
{
    /// <summary>
    /// Выходной файл или "-" для stdout
    /// </summary>
    public string? Output { get; set; }

    /// <summary>
    /// Альтернативный способ указания выходного файла
    /// </summary>
    public string? OutPath { get; set; }

    /// <summary>
    /// Тип сжатия для создаваемого файла
    /// </summary>
    public CompressionKind CompressionKind { get; set; } = CompressionKind.GZip;

    /// <summary>
    /// Получает окончательный выходной путь
    /// </summary>
    public string GetOutputPath() => OutPath ?? Output ?? throw new ArgumentException("Не указан выходной файл");
}

/// <summary>
/// Опции для команды info
/// </summary>
public class InfoOptions : BaseCommandOptions
{
    /// <summary>
    /// Количество символов для предпросмотра содержимого
    /// </summary>
    public int? PeekLength { get; set; }
}
