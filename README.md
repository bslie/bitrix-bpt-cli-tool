# Bitrix BPT Tool

Утилита командной строки для работы с файлами **`.bpt`** (шаблоны Bitrix).  
Позволяет:

- извлекать содержимое (`dump`);
- собирать обратно из текста (`build`);
- смотреть информацию о файле (`info`).

Поддерживаются форматы сжатия: **gzip**, **zlib (gzcompress)**, **deflate (gzdeflate)**, а также несжатый текст.

## Установка и сборка

```bash
git clone https://github.com/ВАШ_АККАУНТ/bitrix-bpt-tool.git
cd bitrix-bpt-tool
dotnet build
```
(Требуется .NET 8.0 SDK или новее.)

Использование
Выгрузка содержимого файла
```bash
dotnet run -- dump bp-179.bpt -o bp-179.txt
```
Человекочитаемый вывод:

```bash
dotnet run -- dump bp-179.bpt --pretty
```
Сборка обратно в .bpt
```bash
# gzip (gzencode)
dotnet run -- build bp-179.txt -o bp-179.bpt --gzip

# zlib (gzcompress)
dotnet run -- build bp-179.txt -o bp-179.bpt --zlib

# deflate (gzdeflate)
dotnet run -- build bp-179.txt -o bp-179.bpt --deflate

# без сжатия
dotnet run -- build bp-179.txt -o bp-179.bpt --plain
```
⚠️ Важно: bp-179.txt должен содержать «сырой» PHP-serialized текст (результат dump без --pretty).

Просмотр информации о файле
```bash
dotnet run -- info bp-179.bpt
```
С «подсмотром» первых 200 символов содержимого:

```bash
dotnet run -- info bp-179.bpt --peek 200
```
## Лицензия
MIT
