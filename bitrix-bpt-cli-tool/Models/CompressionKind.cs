namespace BptTool.Models;

/// <summary>
/// Типы сжатия данных для .bpt файлов
/// </summary>
public enum CompressionKind
{
    /// <summary>
    /// Без сжатия (обычный UTF-8 текст)
    /// </summary>
    Plain,
    
    /// <summary>
    /// GZip сжатие (gzencode в PHP)
    /// </summary>
    GZip,
    
    /// <summary>
    /// ZLib сжатие (gzcompress в PHP)
    /// </summary>
    ZLib,
    
    /// <summary>
    /// Deflate сжатие (gzdeflate в PHP)
    /// </summary>
    Deflate,
    
    /// <summary>
    /// Неопознанный тип сжатия
    /// </summary>
    Unknown
}
