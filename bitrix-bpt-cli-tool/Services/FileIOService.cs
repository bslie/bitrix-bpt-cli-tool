using System.Text;

namespace BptTool.Services;

/// <summary>
/// Сервис для работы с файлами и потоками ввода-вывода
/// </summary>
public static class FileIOService
{
    /// <summary>
    /// Нормализует путь и создает директории при необходимости
    /// </summary>
    /// <param name="pathOrDash">Путь к файлу или "-" для stdin/stdout</param>
    /// <param name="forOutput">True, если путь используется для вывода</param>
    /// <returns>Нормализованный путь</returns>
    public static string NormalizePath(string pathOrDash, bool forOutput = false)
    {
        if (pathOrDash == "-")
            return "-";

        string fullPath = Path.GetFullPath(pathOrDash);
        
        if (forOutput)
        {
            string? directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
        }
        
        return fullPath;
    }

    /// <summary>
    /// Читает все байты из файла или stdin
    /// </summary>
    /// <param name="pathOrDash">Путь к файлу или "-" для stdin</param>
    /// <returns>Содержимое в виде массива байтов</returns>
    public static byte[] ReadAllBytes(string pathOrDash)
    {
        if (pathOrDash == "-")
        {
            using var memory = new MemoryStream();
            Console.OpenStandardInput().CopyTo(memory);
            return memory.ToArray();
        }
        
        return File.ReadAllBytes(pathOrDash);
    }

    /// <summary>
    /// Читает весь текст из файла или stdin
    /// </summary>
    /// <param name="pathOrDash">Путь к файлу или "-" для stdin</param>
    /// <returns>Содержимое в виде строки UTF-8</returns>
    public static string ReadAllText(string pathOrDash)
    {
        if (pathOrDash == "-")
        {
            using var reader = new StreamReader(
                Console.OpenStandardInput(), 
                new UTF8Encoding(false));
            return reader.ReadToEnd();
        }
        
        return File.ReadAllText(pathOrDash, new UTF8Encoding(false));
    }

    /// <summary>
    /// Записывает текст в файл или stdout
    /// </summary>
    /// <param name="pathOrDash">Путь к файлу или "-" для stdout</param>
    /// <param name="content">Содержимое для записи</param>
    public static void WriteAllText(string pathOrDash, string content)
    {
        if (pathOrDash == "-")
        {
            Console.Out.Write(content);
            return;
        }
        
        File.WriteAllText(pathOrDash, content, new UTF8Encoding(false));
    }

    /// <summary>
    /// Записывает байты в файл или stdout
    /// </summary>
    /// <param name="pathOrDash">Путь к файлу или "-" для stdout</param>
    /// <param name="bytes">Данные для записи</param>
    public static void WriteAllBytes(string pathOrDash, byte[] bytes)
    {
        if (pathOrDash == "-")
        {
            using var stdout = Console.OpenStandardOutput();
            stdout.Write(bytes, 0, bytes.Length);
            return;
        }
        
        File.WriteAllBytes(pathOrDash, bytes);
    }

    /// <summary>
    /// Создает TextWriter для записи в файл или stdout
    /// </summary>
    /// <param name="pathOrDash">Путь к файлу или "-" для stdout</param>
    /// <returns>TextWriter для записи</returns>
    public static TextWriter CreateTextWriter(string pathOrDash)
    {
        if (pathOrDash == "-")
            return Console.Out;

        return new StreamWriter(
            File.Open(pathOrDash, FileMode.Create, FileAccess.Write, FileShare.None), 
            new UTF8Encoding(false));
    }

    /// <summary>
    /// Проверяет существование файла
    /// </summary>
    /// <param name="path">Путь к файлу</param>
    /// <returns>True, если файл существует</returns>
    public static bool FileExists(string path) => File.Exists(path);

    /// <summary>
    /// Получает информацию о файле
    /// </summary>
    /// <param name="path">Путь к файлу</param>
    /// <returns>Информация о файле</returns>
    /// <exception cref="FileNotFoundException">Если файл не найден</exception>
    public static FileInfo GetFileInfo(string path)
    {
        var fileInfo = new FileInfo(path);
        if (!fileInfo.Exists)
            throw new FileNotFoundException("Файл не найден", path);
        
        return fileInfo;
    }
}
