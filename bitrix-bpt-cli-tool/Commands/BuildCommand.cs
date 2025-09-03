using BptTool.Models;
using BptTool.Services;

namespace BptTool.Commands;

/// <summary>
/// Команда для сборки .bpt файлов из текстовых данных
/// </summary>
public static class BuildCommand
{
    /// <summary>
    /// Выполняет команду build
    /// </summary>
    /// <param name="options">Опции команды</param>
    /// <returns>Код возврата (0 - успех)</returns>
    public static int Execute(BuildOptions options)
    {
        string inputPath = FileIOService.NormalizePath(options.GetInputPath());
        string outputPath = FileIOService.NormalizePath(options.GetOutputPath(), forOutput: true);

        // Читаем сериализованные данные без какой-либо обработки
        // Битрикс чувствителен к изменениям в байтах
        string serializedData = FileIOService.ReadAllText(inputPath);

        // Сжимаем данные выбранным способом
        byte[] compressedData = CompressionService.Compress(serializedData, options.CompressionKind);

        // Записываем в выходной файл
        FileIOService.WriteAllBytes(outputPath, compressedData);

        return 0;
    }
}