using BptTool.Models;

namespace BptTool.Services;

/// <summary>
/// Сервис для парсинга аргументов командной строки
/// </summary>
public static class ArgumentParser
{
    /// <summary>
    /// Парсит аргументы для команды dump
    /// </summary>
    /// <param name="args">Аргументы командной строки</param>
    /// <returns>Опции команды dump</returns>
    /// <exception cref="ArgumentException">Если аргументы некорректны</exception>
    public static DumpOptions ParseDumpArguments(string[] args)
    {
        if (args.Length < 2)
            throw new ArgumentException("dump: не указан путь к .bpt");

        var options = new DumpOptions
        {
            Input = args[1]
        };

        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-o":
                case "--out":
                    if (i + 1 >= args.Length)
                        throw new ArgumentException($"{args[i]}: отсутствует значение");
                    options.Output = args[++i];
                    break;
                    
                case "--in":
                    if (i + 1 >= args.Length)
                        throw new ArgumentException("--in: отсутствует значение");
                    options.InPath = args[++i];
                    break;
                    
                case "--pretty":
                    options.Pretty = true;
                    break;
                    
                default:
                    throw new ArgumentException($"Неизвестный параметр: {args[i]}");
            }
        }

        return options;
    }

    /// <summary>
    /// Парсит аргументы для команды build
    /// </summary>
    /// <param name="args">Аргументы командной строки</param>
    /// <returns>Опции команды build</returns>
    /// <exception cref="ArgumentException">Если аргументы некорректны</exception>
    public static BuildOptions ParseBuildArguments(string[] args)
    {
        if (args.Length < 2)
            throw new ArgumentException("build: не указан путь к .txt");

        var options = new BuildOptions
        {
            Input = args[1],
            CompressionKind = CompressionKind.GZip // по умолчанию
        };

        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-o":
                case "--out":
                    if (i + 1 >= args.Length)
                        throw new ArgumentException($"{args[i]}: отсутствует значение");
                    options.Output = args[++i];
                    break;
                    
                case "--in":
                    if (i + 1 >= args.Length)
                        throw new ArgumentException("--in: отсутствует значение");
                    options.InPath = args[++i];
                    break;
                    
                case "--gzip":
                    options.CompressionKind = CompressionKind.GZip;
                    break;
                    
                case "--zlib":
                    options.CompressionKind = CompressionKind.ZLib;
                    break;
                    
                case "--deflate":
                    options.CompressionKind = CompressionKind.Deflate;
                    break;
                    
                case "--plain":
                    options.CompressionKind = CompressionKind.Plain;
                    break;
                    
                default:
                    throw new ArgumentException($"Неизвестный параметр: {args[i]}");
            }
        }

        if (string.IsNullOrEmpty(options.Output) && string.IsNullOrEmpty(options.OutPath))
            throw new ArgumentException("build: задай выходной файл через -o out.bpt");

        return options;
    }

    /// <summary>
    /// Парсит аргументы для команды info
    /// </summary>
    /// <param name="args">Аргументы командной строки</param>
    /// <returns>Опции команды info</returns>
    /// <exception cref="ArgumentException">Если аргументы некорректны</exception>
    public static InfoOptions ParseInfoArguments(string[] args)
    {
        if (args.Length < 2)
            throw new ArgumentException("info: не указан путь к файлу");

        var options = new InfoOptions
        {
            Input = args[1]
        };

        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--peek":
                    if (i + 1 >= args.Length)
                        throw new ArgumentException("--peek: отсутствует значение");
                    if (!int.TryParse(args[++i], out int peekValue) || peekValue < 0)
                        throw new ArgumentException("--peek: укажи неотрицательное число");
                    options.PeekLength = peekValue;
                    break;
                    
                case "--in":
                    if (i + 1 >= args.Length)
                        throw new ArgumentException("--in: отсутствует значение");
                    options.InPath = args[++i];
                    break;
                    
                default:
                    throw new ArgumentException($"Неизвестный параметр: {args[i]}");
            }
        }

        return options;
    }
}
