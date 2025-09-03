// dotnet add package PhpSerializerNET
// tested on .NET 8.0

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using PhpSerializerNET;  // NuGet

namespace BptTool
{
    internal static class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0 || args[0] is "-h" or "--help")
            {
                PrintHelp();
                return 0;
            }

            try
            {
                switch (args[0])
                {
                    case "dump": return CmdDump(args);
                    case "build": return CmdBuild(args);
                    case "info": return CmdInfo(args);
                    default:
                        Console.Error.WriteLine("Неизвестная команда. Используй 'dump', 'build' или 'info'.");
                        PrintHelp();
                        return 2;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"🔥  {ex.GetType().Name}: {ex.Message}");
                return 1;
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine(@"
BptTool — утилита для Bitrix *.bpt

Использование:
  BptTool dump <input.bpt> [-o out.txt] [--pretty]
      Извлекает из .bpt строку PHP-serialized (по умолчанию) или печатает древо (--pretty).

  BptTool build <input.txt> -o out.bpt [--gzip | --zlib | --deflate | --plain]
      Собирает .bpt из текстового файла с ''сырым'' PHP-serialized содержимым.
      ВАЖНО: ничего не трогает в тексте (никакого Trim), байты идут как есть.

  BptTool info <input.bpt> [--peek N]
      Печатает информацию о файле: имя, размер, расширение, способ сжатия (gzip/zlib/deflate/plain),
      и проверяет, похоже ли содержимое на PHP-serialized строку.
");
        }

        // -------------------- I N F O --------------------

        static int CmdInfo(string[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException("info: не указан путь к файлу");

            string path = args[1];
            int? peek = null;

            for (int i = 2; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--peek":
                        if (i + 1 >= args.Length) throw new ArgumentException("--peek: отсутствует значение");
                        if (!int.TryParse(args[++i], out int n) || n < 0) throw new ArgumentException("--peek: укажи неотрицательное число");
                        peek = n;
                        break;
                    default:
                        throw new ArgumentException($"Неизвестный параметр: {args[i]}");
                }
            }

            var fi = new FileInfo(path);
            if (!fi.Exists)
                throw new FileNotFoundException("Файл не найден", path);

            byte[] raw = File.ReadAllBytes(path);

            var kind = TryDetectKindByHeader(raw);
            CompressionKind detected;
            string text = TryInflateAuto(raw, out detected);

            string trimmed = text; // НЕ трогаем, просто для LooksLike
            bool looksSerialized = LooksLikePhpSerialized(trimmed);

            Console.WriteLine($"Файл            : {fi.FullName}");
            Console.WriteLine($"Размер          : {fi.Length} байт");
            Console.WriteLine($"Расширение      : {fi.Extension}");
            Console.WriteLine($"По заголовку    : {KindToHuman(kind)}");
            Console.WriteLine($"Фактический разбор: {KindToHuman(detected)}");
            Console.WriteLine($"Контент         : {(looksSerialized ? "Похоже на PHP-serialized" : "Не похоже на PHP-serialized")}");
            Console.WriteLine($"Длина текста после распаковки: {text.Length} символов");

            if (peek is int p && p > 0)
            {
                int len = Math.Min(p, text.Length);
                Console.WriteLine();
                Console.WriteLine($"Первые {len} символов:");
                Console.WriteLine(text.Substring(0, len));
            }

            return 0;
        }

        static string KindToHuman(CompressionKind k) => k switch
        {
            CompressionKind.GZip => "GZip (1F 8B)",
            CompressionKind.ZLib => "ZLib (78 ??)",
            CompressionKind.Deflate => "Raw Deflate",
            CompressionKind.Plain => "Нет (plain UTF-8)",
            _ => "Неизвестно"
        };

        static bool LooksLikePhpSerialized(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            var head = s.Length > 200 ? s[..200] : s;

            if (head.StartsWith("a:") || head.StartsWith("s:") || head.StartsWith("i:") ||
                head.StartsWith("b:") || head.StartsWith("N;") || head.StartsWith("O:") ||
                head.StartsWith("d:"))
                return true;

            var rx = new Regex(@"^(a|s|i|b|d|O):\d+[:;{]", RegexOptions.Compiled);
            return rx.IsMatch(head);
        }

        // -------------------- D U M P --------------------

        static int CmdDump(string[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException("dump: не указан путь к .bpt");

            string input = args[1];
            string? output = null;
            bool pretty = false;

            for (int i = 2; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-o":
                        if (i + 1 >= args.Length) throw new ArgumentException("-o: отсутствует значение");
                        output = args[++i];
                        break;
                    case "--pretty":
                        pretty = true;
                        break;
                    default:
                        throw new ArgumentException($"Неизвестный параметр: {args[i]}");
                }
            }

            byte[] raw = File.ReadAllBytes(input);
            CompressionKind detected;
            string serialized = TryInflateAuto(raw, out detected);

            if (pretty)
            {
                var root = PhpSerialization.Deserialize(serialized);
                using var sw = output is null
                    ? Console.Out
                    : new StreamWriter(File.Open(output, FileMode.Create, FileAccess.Write, FileShare.None), new UTF8Encoding(false));
                DumpPretty(root, 0, sw);
            }
            else
            {
                if (output is null)
                    Console.Write(serialized);
                else
                    File.WriteAllText(output, serialized, new UTF8Encoding(false));
            }

            return 0;
        }

        // -------------------- B U I L D --------------------

        static int CmdBuild(string[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException("build: не указан путь к .txt");

            string inputTxt = args[1];
            string? outputBpt = null;
            CompressionKind kind = CompressionKind.GZip; // дефолт как раньше, но чаще подходит ZLib/Deflate — см. ниже.

            for (int i = 2; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-o":
                        if (i + 1 >= args.Length) throw new ArgumentException("-o: отсутствует значение");
                        outputBpt = args[++i];
                        break;
                    case "--gzip": kind = CompressionKind.GZip; break;
                    case "--zlib": kind = CompressionKind.ZLib; break;
                    case "--deflate": kind = CompressionKind.Deflate; break;
                    case "--plain": kind = CompressionKind.Plain; break;
                    default:
                        throw new ArgumentException($"Неизвестный параметр: {args[i]}");
                }
            }

            if (outputBpt is null)
                throw new ArgumentException("build: задай выходной файл через -o out.bpt");

            // НИЧЕГО НЕ ТРИММ: Bitrix/serialize чувствителен к байтам.
            string serialized = File.ReadAllText(inputTxt, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            byte[] bytes = BuildBytes(serialized, kind);
            File.WriteAllBytes(outputBpt, bytes);

            return 0;
        }

        enum CompressionKind { Plain, GZip, ZLib, Deflate, Unknown }

        static byte[] BuildBytes(string serialized, CompressionKind kind)
        {
            byte[] text = new UTF8Encoding(false).GetBytes(serialized);

            return kind switch
            {
                CompressionKind.Plain => text,
                CompressionKind.GZip => GzipCompress(text),
                CompressionKind.ZLib => ZlibCompress(text),
                CompressionKind.Deflate => DeflateCompress(text),
                _ => throw new NotSupportedException("Неподдерживаемый тип сжатия для сборки")
            };
        }

        // -------------------- C O R E --------------------

        static CompressionKind TryDetectKindByHeader(byte[] data)
        {
            if (data.Length > 2 && data[0] == 0x1F && data[1] == 0x8B) return CompressionKind.GZip; // gzip
            if (data.Length > 2 && data[0] == 0x78) return CompressionKind.ZLib; // zlib (0x78 0x01/9C/DA)
            return CompressionKind.Unknown; // deflate/plain не различить по сигнатуре
        }

        static string TryInflateAuto(byte[] data, out CompressionKind detected)
        {
            // 1) попробуем по заголовку
            var headerKind = TryDetectKindByHeader(data);
            try
            {
                switch (headerKind)
                {
                    case CompressionKind.GZip: detected = CompressionKind.GZip; return InflateGzip(data);
                    case CompressionKind.ZLib: detected = CompressionKind.ZLib; return InflateZlib(data);
                }
            }
            catch { /* fallthrough */ }

            // 2) попробуем как raw deflate
            try { detected = CompressionKind.Deflate; return InflateDeflate(data); } catch { }

            // 3) попробуем трактовать как UTF-8 plain
            try
            {
                string s = Encoding.UTF8.GetString(data);
                detected = CompressionKind.Plain;
                return s;
            }
            catch { }

            detected = CompressionKind.Unknown;
            throw new InvalidDataException("Не удалось распаковать содержимое файла ни как gzip, ни как zlib, ни как deflate/plain.");
        }

        static string InflateGzip(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var gz = new GZipStream(ms, CompressionMode.Decompress);
            using var sr = new StreamReader(gz, Encoding.UTF8);
            return sr.ReadToEnd();
        }

        static string InflateZlib(byte[] data)
        {
#if NET7_0_OR_GREATER
            using var ms = new MemoryStream(data);
            using var zb = new ZLibStream(ms, CompressionMode.Decompress);
            using var sr = new StreamReader(zb, Encoding.UTF8);
            return sr.ReadToEnd();
#else
            throw new NotSupportedException("ZLibStream доступен начиная с .NET 7.");
#endif
        }

        static string InflateDeflate(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var df = new DeflateStream(ms, CompressionMode.Decompress);
            using var sr = new StreamReader(df, Encoding.UTF8);
            return sr.ReadToEnd();
        }

        static byte[] GzipCompress(byte[] input)
        {
            using var ms = new MemoryStream();
            using (var gz = new GZipStream(ms, CompressionLevel.SmallestSize, leaveOpen: true))
                gz.Write(input, 0, input.Length);
            return ms.ToArray();
        }

        static byte[] ZlibCompress(byte[] input)
        {
#if NET7_0_OR_GREATER
            using var ms = new MemoryStream();
            using (var zb = new ZLibStream(ms, CompressionLevel.SmallestSize, leaveOpen: true))
                zb.Write(input, 0, input.Length);
            return ms.ToArray();
#else
            throw new NotSupportedException("ZLibStream доступен начиная с .NET 7.");
#endif
        }

        static byte[] DeflateCompress(byte[] input)
        {
            using var ms = new MemoryStream();
            using (var df = new DeflateStream(ms, CompressionLevel.SmallestSize, leaveOpen: true))
                df.Write(input, 0, input.Length);
            return ms.ToArray();
        }

        /// <summary>Грубый pretty-print PHP-массивов/объектов.</summary>
        static void DumpPretty(object node, int level, TextWriter tw)
        {
            string pad = new string(' ', level * 2);

            switch (node)
            {
                case IDictionary<object, object> map:
                    foreach (var kv in map)
                    {
                        tw.WriteLine($"{pad}{kv.Key}:");
                        DumpPretty(kv.Value, level + 1, tw);
                    }
                    break;

                case IList<object> list:
                    for (int i = 0; i < list.Count; i++)
                    {
                        tw.WriteLine($"{pad}[{i}]:");
                        DumpPretty(list[i], level + 1, tw);
                    }
                    break;

                default:
                    tw.WriteLine($"{pad}{node}");
                    break;
            }
        }
    }
}
