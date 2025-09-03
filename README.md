# 🛠️ Bitrix BPT CLI Tool

<p align="center">
  <strong>Инструмент командной строки для работы с файлами .bpt (шаблоны бизнес-процессов Bitrix)</strong>
</p>

<p align="center">
  <a href="#english-version">🇺🇸 English</a> •
  <a href="#русская-версия">🇷🇺 Русский</a>
</p>

---

## 🇷🇺 Русская версия

### ✨ Возможности

CLI-утилита для работы с файлами **`.bpt`** (шаблоны бизнес-процессов Bitrix):

- 📤 **Извлечение** содержимого (`dump`)
- 📦 **Сборка** обратно из текста (`build`)
- ℹ️ **Просмотр** информации о файле (`info`)

### 🗜️ Поддерживаемые форматы сжатия

- **gzip** (gzencode)
- **zlib** (gzcompress)
- **deflate** (gzdeflate)
- **plain** (без сжатия)

### 📥 Загрузка

📋 **Готовые сборки** (Windows/Linux/macOS):  
👉 [**Releases · bitrix-bpt-tool**](https://github.com/bslie/bitrix-bpt-tool/releases/latest)

### 🔧 Установка из исходников

```bash
git clone https://github.com/bslie/bitrix-bpt-tool.git
cd bitrix-bpt-tool
dotnet build
```

> **Требования:** .NET 9.0 SDK или новее

### 🚀 Быстрый старт

#### 📤 Извлечение содержимого .bpt

```bash
# Основная выгрузка
dotnet run -- dump bp-179.bpt -o bp-179.txt

# Человекочитаемый вывод (unserialize + дерево)
dotnet run -- dump bp-179.bpt --pretty
```

#### 📦 Сборка обратно в .bpt

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

> ⚠️ **Важно:** `bp-179.txt` должен содержать сырой PHP-serialized текст (результат `dump` без `--pretty`).  
> Не изменяйте содержимое внутри `s:<len>:"..."` — длины должны точно совпадать по байтам.

#### ℹ️ Просмотр информации о файле

```bash
# Базовая информация
dotnet run -- info bp-179.bpt

# С предпросмотром содержимого
dotnet run -- info bp-179.bpt --peek 200
```

### 🔄 Работа с потоками

Поддерживаются **stdin/stdout** для конвейерной обработки:

```bash
# Цепочка команд через pipe
dotnet run -- dump data/bp-179.bpt -o - | \
dotnet run -- build - --out out/bp-179-deflate.bpt --deflate

# Автосоздание директорий
dotnet run -- build data/bp-179.txt --out out/rebuilt/bp-179.bpt --zlib
```

**Альтернативные флаги:**

- `--in <path>` и `--out <path>` (эквивалентны позиционным аргументам и `-o`)

### 🔍 Определение формата упаковки

Чтобы правильно пересобрать файл, сначала определите формат исходного .bpt:

```bash
dotnet run -- info bp-179.bpt
```

Обратите внимание на:

- **Заголовок** — магические байты
- **Фактический формат** — что сработало при распаковке

Используйте соответствующий флаг: `--gzip`, `--zlib`, `--deflate` или `--plain`

### 📂 Получение .bpt файлов из Bitrix

#### 🏢 On-Premise (1С-Битрикс: Управление сайтом)

1. **Настройки** → **Инструменты** → **Бизнес-процессы**
2. **Шаблоны бизнес-процессов**
3. Меню действий нужного шаблона → **Экспорт**

#### ☁️ Bitrix24 (облако)

1. **CRM** → **Настройки CRM** → **Бизнес-процессы** или **Списки** → **Бизнес-процессы**
2. Найдите шаблон → меню действий → **Экспорт**

---

## 🇺🇸 English Version

### ✨ Features

CLI utility for working with **`.bpt`** files (Bitrix business process templates):

- 📤 **Extract** content (`dump`)
- 📦 **Build** back from text (`build`)
- ℹ️ **Display** file information (`info`)

### 🗜️ Supported Compression Formats

- **gzip** (gzencode)
- **zlib** (gzcompress)
- **deflate** (gzdeflate)
- **plain** (uncompressed)

### 📥 Download

📋 **Pre-built binaries** (Windows/Linux/macOS):  
👉 [**Releases · bitrix-bpt-tool**](https://github.com/bslie/bitrix-bpt-tool/releases/latest)

### 🔧 Build from Source

```bash
git clone https://github.com/bslie/bitrix-bpt-tool.git
cd bitrix-bpt-tool
dotnet build
```

> **Requirements:** .NET 9.0 SDK or later

### 🚀 Quick Start

#### 📤 Extract .bpt Content

```bash
# Basic extraction
dotnet run -- dump bp-179.bpt -o bp-179.txt

# Human-readable output (unserialized + tree structure)
dotnet run -- dump bp-179.bpt --pretty
```

#### 📦 Build Back to .bpt

```bash
# gzip (gzencode)
dotnet run -- build bp-179.txt -o bp-179.bpt --gzip

# zlib (gzcompress)
dotnet run -- build bp-179.txt -o bp-179.bpt --zlib

# deflate (gzdeflate)
dotnet run -- build bp-179.txt -o bp-179.bpt --deflate

# plain (uncompressed)
dotnet run -- build bp-179.txt -o bp-179.bpt --plain
```

> ⚠️ **Important:** `bp-179.txt` must contain raw PHP-serialized text (result of `dump` without `--pretty`).  
> Don't modify content inside `s:<len>:"..."` — lengths must match exactly by bytes.

#### ℹ️ View File Information

```bash
# Basic information
dotnet run -- info bp-179.bpt

# With content preview
dotnet run -- info bp-179.bpt --peek 200
```

### 🔄 Stream Processing

Support for **stdin/stdout** for pipeline processing:

```bash
# Command chaining via pipe
dotnet run -- dump data/bp-179.bpt -o - | \
dotnet run -- build - --out out/bp-179-deflate.bpt --deflate

# Auto-create directories
dotnet run -- build data/bp-179.txt --out out/rebuilt/bp-179.bpt --zlib
```

**Alternative flags:**

- `--in <path>` and `--out <path>` (equivalent to positional arguments and `-o`)

### 🔍 Detecting Compression Format

To properly rebuild a file, first determine the original .bpt format:

```bash
dotnet run -- info bp-179.bpt
```

Pay attention to:

- **Header** — magic bytes detection
- **Actual format** — what worked during unpacking

Use the corresponding flag: `--gzip`, `--zlib`, `--deflate`, or `--plain`

### 📂 Getting .bpt Files from Bitrix

#### 🏢 On-Premise (1C-Bitrix: Site Management)

1. **Settings** → **Tools** → **Business Processes**
2. **Business Process Templates**
3. Template action menu → **Export**

#### ☁️ Bitrix24 (Cloud)

1. **CRM** → **CRM Settings** → **Business Processes** or **Lists** → **Business Processes**
2. Find template → action menu → **Export**

---

## 📄 License

**MIT License** - see [LICENSE.txt](LICENSE.txt) for details
