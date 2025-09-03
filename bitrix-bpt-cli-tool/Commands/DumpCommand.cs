using BptTool.Models;
using BptTool.Services;

namespace BptTool.Commands;

/// <summary>
/// Команда для извлечения содержимого из .bpt файлов
/// </summary>
public static class DumpCommand
{
    /// <summary>
    /// Выполняет команду dump
    /// </summary>
    /// <param name="options">Опции команды</param>
    /// <returns>Код возврата (0 - успех)</returns>
    public static int Execute(DumpOptions options)
    {
        string inputPath = FileIOService.NormalizePath(options.GetInputPath());
        byte[] rawData = FileIOService.ReadAllBytes(inputPath);
        
        string serializedData = CompressionService.DecompressAuto(rawData, out _);

        if (options.Pretty)
        {
            ExecutePrettyDump(options, serializedData);
        }
        else
        {
            ExecuteRawDump(options, serializedData);
        }

        return 0;
    }

    /// <summary>
    /// Выполняет обычную выгрузку (сырые PHP-сериализованные данные)
    /// </summary>
    private static void ExecuteRawDump(DumpOptions options, string serializedData)
    {
        string outputPath = FileIOService.NormalizePath(options.GetOutputPath(), forOutput: true);
        FileIOService.WriteAllText(outputPath, serializedData);
    }

    /// <summary>
    /// Выполняет красивую выгрузку (древо структуры)
    /// </summary>
    private static void ExecutePrettyDump(DumpOptions options, string serializedData)
    {
        var deserializedObject = SerializationService.DeserializePhp(serializedData);
        string outputPath = FileIOService.NormalizePath(options.GetOutputPath(), forOutput: true);

        using var writer = FileIOService.CreateTextWriter(outputPath);
        SerializationService.DumpPretty(deserializedObject, 0, writer);
        writer.Flush();
    }
}
