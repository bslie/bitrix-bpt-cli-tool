// Bitrix BPT CLI Tool
// Requires .NET 9.0 SDK or later

using BptTool.Commands;
using BptTool.Services;

namespace BptTool;

/// <summary>
/// Основная точка входа в приложение
/// </summary>
internal static class Program
{
    /// <summary>
    /// Точка входа приложения
    /// </summary>
    /// <param name="args">Аргументы командной строки</param>
    /// <returns>Код возврата (0 - успех, 1 - ошибка, 2 - неверная команда)</returns>
    static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            PrintHelp();
            return 0;
        }

        try
        {
            return args[0] switch
            {
                "dump" => ExecuteDumpCommand(args),
                "build" => ExecuteBuildCommand(args),
                "info" => ExecuteInfoCommand(args),
                _ => HandleUnknownCommand()
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"🔥 {ex.GetType().Name}: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Выполняет команду dump
    /// </summary>
    private static int ExecuteDumpCommand(string[] args)
    {
        var options = ArgumentParser.ParseDumpArguments(args);
        return DumpCommand.Execute(options);
    }

    /// <summary>
    /// Выполняет команду build
    /// </summary>
    private static int ExecuteBuildCommand(string[] args)
    {
        var options = ArgumentParser.ParseBuildArguments(args);
        return BuildCommand.Execute(options);
    }

    /// <summary>
    /// Выполняет команду info
    /// </summary>
    private static int ExecuteInfoCommand(string[] args)
    {
        var options = ArgumentParser.ParseInfoArguments(args);
        return InfoCommand.Execute(options);
    }

    /// <summary>
    /// Обрабатывает неизвестную команду
    /// </summary>
    private static int HandleUnknownCommand()
    {
        Console.Error.WriteLine("Неизвестная команда. Используй 'dump', 'build' или 'info'.");
        PrintHelp();
        return 2;
    }

    /// <summary>
    /// Выводит справочную информацию о программе
    /// </summary>
    private static void PrintHelp()
    {
        Console.WriteLine(@"
BptTool — утилита для Bitrix *.bpt

Использование:
  BptTool dump <input.bpt|-> [-o out.txt|'-'] [--pretty] [--in <path>] [--out <path>]
      Извлекает из .bpt строку PHP-serialized или печатает древо (--pretty).

  BptTool build <input.txt|-> -o out.bpt|'-' [--gzip | --zlib | --deflate | --plain] [--in <path>] [--out <path>]
      Собирает .bpt из текстового файла с 'сырым' PHP-serialized содержимым.
      ВАЖНО: ничего не трогает в тексте (никакого Trim), байты идут как есть.

  BptTool info <input.bpt> [--peek N] [--in <path>]
      Печатает информацию о файле: имя, размер, способ сжатия (gzip/zlib/deflate/plain),
      и проверяет, похоже ли содержимое на PHP-serialized строку.

Примечания:
  '-' вместо пути означает stdin (для входа) или stdout (для выхода).
  Пути могут быть абсолютными или относительными; недостающие директории для -o создаются автоматически.
");
    }
}
