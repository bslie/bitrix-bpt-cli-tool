using BptTool.Models;
using BptTool.Services;

namespace BptTool.Commands;

/// <summary>
/// Команда для отображения информации о .bpt файле
/// </summary>
public static class InfoCommand
{
    /// <summary>
    /// Выполняет команду info
    /// </summary>
    /// <param name="options">Опции команды</param>
    /// <returns>Код возврата (0 - успех)</returns>
    public static int Execute(InfoOptions options)
    {
        string inputPath = FileIOService.NormalizePath(options.GetInputPath());
        
        if (inputPath == "-")
            throw new ArgumentException("info: команда должна читать из файла, а не из stdin");

        var fileInfo = FileIOService.GetFileInfo(inputPath);
        byte[] rawData = FileIOService.ReadAllBytes(inputPath);

        var headerKind = CompressionService.DetectCompressionByHeader(rawData);
        string decompressedText = CompressionService.DecompressAuto(rawData, out var actualKind);
        bool looksPhpSerialized = SerializationService.LooksLikePhpSerialized(decompressedText);

        PrintFileInfo(fileInfo, headerKind, actualKind, looksPhpSerialized, decompressedText.Length);

        if (options.PeekLength is int peekLength && peekLength > 0)
        {
            PrintContentPreview(decompressedText, peekLength);
        }

        return 0;
    }

    private static void PrintFileInfo(FileInfo fileInfo, CompressionKind headerKind, 
        CompressionKind actualKind, bool looksPhpSerialized, int textLength)
    {
        Console.WriteLine($"Файл                : {fileInfo.FullName}");
        Console.WriteLine($"Размер              : {fileInfo.Length} байт");
        Console.WriteLine($"Расширение          : {fileInfo.Extension}");
        Console.WriteLine($"По заголовку        : {CompressionService.GetHumanReadableName(headerKind)}");
        Console.WriteLine($"Фактический разбор  : {CompressionService.GetHumanReadableName(actualKind)}");
        Console.WriteLine($"Контент             : {(looksPhpSerialized ? "Похоже на PHP-serialized" : "Не похоже на PHP-serialized")}");
        Console.WriteLine($"Длина распакованного текста: {textLength} символов");
    }

    private static void PrintContentPreview(string text, int peekLength)
    {
        int actualLength = Math.Min(peekLength, text.Length);
        Console.WriteLine();
        Console.WriteLine($"Первые {actualLength} символов:");
        Console.WriteLine(text[..actualLength]);
    }
}
