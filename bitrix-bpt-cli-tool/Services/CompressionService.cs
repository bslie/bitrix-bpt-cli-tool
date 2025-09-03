using System.IO.Compression;
using System.Text;
using BptTool.Models;

namespace BptTool.Services;

/// <summary>
/// Сервис для работы с различными типами сжатия данных
/// </summary>
public static class CompressionService
{
    /// <summary>
    /// Определяет тип сжатия по заголовку файла
    /// </summary>
    /// <param name="data">Данные для анализа</param>
    /// <returns>Тип сжатия или Unknown, если не удалось определить</returns>
    public static CompressionKind DetectCompressionByHeader(byte[] data)
    {
        if (data.Length < 2)
            return CompressionKind.Unknown;

        // GZip magic bytes: 1F 8B
        if (data[0] == 0x1F && data[1] == 0x8B)
            return CompressionKind.GZip;

        // ZLib magic bytes: 78 (первый байт), второй может быть разный
        if (data[0] == 0x78)
            return CompressionKind.ZLib;

        // Deflate и Plain не имеют четкой сигнатуры
        return CompressionKind.Unknown;
    }

    /// <summary>
    /// Автоматически определяет тип сжатия и распаковывает данные
    /// </summary>
    /// <param name="data">Сжатые данные</param>
    /// <param name="detectedKind">Определенный тип сжатия</param>
    /// <returns>Распакованная строка в UTF-8</returns>
    /// <exception cref="InvalidDataException">Если не удалось распаковать никаким способом</exception>
    public static string DecompressAuto(byte[] data, out CompressionKind detectedKind)
    {
        var headerKind = DetectCompressionByHeader(data);

        // Пытаемся по заголовку
        try
        {
            switch (headerKind)
            {
                case CompressionKind.GZip:
                    detectedKind = CompressionKind.GZip;
                    return DecompressGZip(data);
                case CompressionKind.ZLib:
                    detectedKind = CompressionKind.ZLib;
                    return DecompressZLib(data);
            }
        }
        catch { /* fallthrough to other methods */ }

        // Пытаемся Deflate
        try
        {
            detectedKind = CompressionKind.Deflate;
            return DecompressDeflate(data);
        }
        catch { /* fallthrough */ }

        // Пытаемся как обычный текст
        try
        {
            string text = Encoding.UTF8.GetString(data);
            detectedKind = CompressionKind.Plain;
            return text;
        }
        catch { /* fallthrough */ }

        detectedKind = CompressionKind.Unknown;
        throw new InvalidDataException("Не удалось распаковать содержимое файла ни одним из поддерживаемых способов (gzip, zlib, deflate, plain).");
    }

    /// <summary>
    /// Сжимает данные указанным способом
    /// </summary>
    /// <param name="text">Текст для сжатия</param>
    /// <param name="kind">Тип сжатия</param>
    /// <returns>Сжатые данные</returns>
    /// <exception cref="NotSupportedException">Если тип сжатия не поддерживается</exception>
    public static byte[] Compress(string text, CompressionKind kind)
    {
        byte[] textBytes = Encoding.UTF8.GetBytes(text);

        return kind switch
        {
            CompressionKind.Plain => textBytes,
            CompressionKind.GZip => CompressGZip(textBytes),
            CompressionKind.ZLib => CompressZLib(textBytes),
            CompressionKind.Deflate => CompressDeflate(textBytes),
            _ => throw new NotSupportedException($"Неподдерживаемый тип сжатия: {kind}")
        };
    }

    /// <summary>
    /// Возвращает человекочитаемое название типа сжатия
    /// </summary>
    public static string GetHumanReadableName(CompressionKind kind) => kind switch
    {
        CompressionKind.GZip => "GZip (1F 8B)",
        CompressionKind.ZLib => "ZLib (78 ??)",
        CompressionKind.Deflate => "Raw Deflate",
        CompressionKind.Plain => "Нет (plain UTF-8)",
        _ => "Неизвестно"
    };

    #region Private compression methods

    private static string DecompressGZip(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var gz = new GZipStream(ms, CompressionMode.Decompress);
        using var sr = new StreamReader(gz, Encoding.UTF8);
        return sr.ReadToEnd();
    }

    private static string DecompressZLib(byte[] data)
    {
#if NET7_0_OR_GREATER
        using var ms = new MemoryStream(data);
        using var zlib = new ZLibStream(ms, CompressionMode.Decompress);
        using var sr = new StreamReader(zlib, Encoding.UTF8);
        return sr.ReadToEnd();
#else
        throw new NotSupportedException("ZLibStream доступен начиная с .NET 7.0");
#endif
    }

    private static string DecompressDeflate(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var deflate = new DeflateStream(ms, CompressionMode.Decompress);
        using var sr = new StreamReader(deflate, Encoding.UTF8);
        return sr.ReadToEnd();
    }

    private static byte[] CompressGZip(byte[] input)
    {
        using var ms = new MemoryStream();
        using (var gz = new GZipStream(ms, CompressionLevel.SmallestSize, leaveOpen: true))
            gz.Write(input, 0, input.Length);
        return ms.ToArray();
    }

    private static byte[] CompressZLib(byte[] input)
    {
#if NET7_0_OR_GREATER
        using var ms = new MemoryStream();
        using (var zlib = new ZLibStream(ms, CompressionLevel.SmallestSize, leaveOpen: true))
            zlib.Write(input, 0, input.Length);
        return ms.ToArray();
#else
        throw new NotSupportedException("ZLibStream доступен начиная с .NET 7.0");
#endif
    }

    private static byte[] CompressDeflate(byte[] input)
    {
        using var ms = new MemoryStream();
        using (var deflate = new DeflateStream(ms, CompressionLevel.SmallestSize, leaveOpen: true))
            deflate.Write(input, 0, input.Length);
        return ms.ToArray();
    }

    #endregion
}
