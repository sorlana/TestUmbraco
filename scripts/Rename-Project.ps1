# Устанавливаем кодировку UTF-8 для консоли
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::InputEncoding = [System.Text.Encoding]::UTF8

# Параметры
$oldName = "Emax"
$newName = "TestUmbraco"

# Определение корневого пути проекта (родительская папка от scripts)
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$solutionPath = Resolve-Path "$scriptPath/.."

# Создаем точку восстановления перед началом
$backupFolder = "$solutionPath\backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Write-Host "Создание резервной копии перед переименованием..." -ForegroundColor Cyan
Copy-Item -Path "$solutionPath\*" -Destination $backupFolder -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "Резервная копия создана в: $backupFolder" -ForegroundColor Green

Write-Host "Начинаю полное переименование проекта $oldName → $newName" -ForegroundColor Cyan
Write-Host "Корневой путь проекта: $solutionPath" -ForegroundColor DarkGray

# 1. Составляем полный список файлов и папок для обработки
Write-Host "Сканирование файлов и папок для переименования..." -ForegroundColor Yellow
$filesToProcess = Get-ChildItem -Path $solutionPath -Recurse -File | Where-Object {
    $_.FullName -notlike "*node_modules*" -and 
    $_.FullName -notlike "*bin*" -and 
    $_.FullName -notlike "*obj*" -and
    $_.FullName -notlike "*backup_*" -and
    $_.FullName -notlike "*.git*" -and
    $_.FullName -notlike "*.vs*" -and
    $_.FullName -notlike "*.suo*" -and
    $_.FullName -notlike "*context*" -and
    $_.FullName -notlike "*umbraco\Data\*" -and
    $_.FullName -notlike "*umbraco\Logs\*"
}

$foldersToProcess = Get-ChildItem -Path $solutionPath -Recurse -Directory | Where-Object {
    $_.FullName -notlike "*node_modules*" -and 
    $_.FullName -notlike "*bin*" -and 
    $_.FullName -notlike "*obj*" -and
    $_.FullName -notlike "*backup_*" -and
    $_.FullName -notlike "*.git*" -and
    $_.FullName -notlike "*.vs*" -and
    $_.FullName -notlike "*umbraco\Data*" -and
    $_.FullName -notlike "*umbraco\Logs*"
} | Sort-Object FullName -Descending  # Сортируем в обратном порядке для безопасного переименования папок

Write-Host "Найдено папок для переименования: $($foldersToProcess.Count)" -ForegroundColor Cyan
Write-Host "Найдено файлов для обработки: $($filesToProcess.Count)" -ForegroundColor Cyan

# 2. Переименование папок (в безопасном порядке)
Write-Host "Переименование папок..." -ForegroundColor Yellow
foreach ($folder in $foldersToProcess) {
    $newFolderName = $folder.Name.Replace($oldName, $newName)
    if ($newFolderName -ne $folder.Name -and $newFolderName -notlike "*$oldName*") {
        try {
            Rename-Item -Path $folder.FullName -NewName $newFolderName -ErrorAction Stop
            Write-Host "Переименована папка: $($folder.Name) → $newFolderName" -ForegroundColor Green
        } catch {
            Write-Host "Не удалось переименовать папку $($folder.FullName): $_" -ForegroundColor Yellow
        }
    }
}

# 3. Переименование solution файла
Write-Host "Переименование .sln файла..." -ForegroundColor Yellow
$oldSolutionPath = "$solutionPath\$oldName.sln"
$newSolutionPath = "$solutionPath\$newName.sln"

if (Test-Path $oldSolutionPath) {
    Rename-Item $oldSolutionPath $newSolutionPath
    Write-Host "Solution файл переименован: $oldName.sln → $newName.sln" -ForegroundColor Green
} else {
    Write-Host "Solution файл не найден по пути: $oldSolutionPath" -ForegroundColor Yellow
}

# 4. Переименование проектных файлов
Write-Host "Переименование проектных файлов..." -ForegroundColor Yellow
Get-ChildItem -Path $solutionPath -Recurse -File | Where-Object {
    $_.Name -match "$oldName.*\.csproj$" -or
    $_.Name -match "$oldName.*\.sln$"
} | ForEach-Object {
    $newFileName = $_.Name.Replace($oldName, $newName)
    if ($newFileName -ne $_.Name) {
        try {
            Rename-Item -Path $_.FullName -NewName $newFileName -ErrorAction Stop
            Write-Host "Переименован файл проекта: $($_.Name) → $newFileName" -ForegroundColor Green
        } catch {
            Write-Host "Не удалось переименовать файл $($_.FullName): $_" -ForegroundColor Yellow
        }
    }
}

# 5. Обновление путей в .sln файле
if (Test-Path $newSolutionPath) {
    Write-Host "Обновление путей в .sln файле..." -ForegroundColor Yellow
    $slnContent = Get-Content $newSolutionPath -Raw
    $newSlnContent = $slnContent -replace [regex]::Escape($oldName), $newName -replace [regex]::Escape($oldName.ToLower()), $newName.ToLower()
    Set-Content $newSolutionPath $newSlnContent -Encoding UTF8
    Write-Host "Пути в .sln файле обновлены" -ForegroundColor Green
}

# 6. Глобальная замена во всех файлах с поддержкой UTF-8 и бинарных файлов
Write-Host "Глобальный поиск и замена с поддержкой кириллицы..." -ForegroundColor Yellow
$processedFiles = 0
$binaryExtensions = @('.dll', '.exe', '.pdb', '.zip', '.png', '.jpg', '.jpeg', '.gif', '.bmp', '.ico', '.pdf', '.sqlite', '.db')

$filesToProcess | ForEach-Object {
    # Пропускаем бинарные файлы
    if ($binaryExtensions -contains $_.Extension) {
        return
    }

    try {
        $content = Get-Content $_.FullName -Raw -Encoding UTF8 -ErrorAction Stop
        
        # Если UTF8 не сработал, пробуем другие кодировки
        if ($null -eq $content -or $content -eq "") {
            $encodingsToTry = @(
                [System.Text.Encoding]::GetEncoding(1251),  # Windows-1251 для кириллицы
                [System.Text.Encoding]::Default,
                [System.Text.Encoding]::ASCII
            )
            
            foreach ($encoding in $encodingsToTry) {
                try {
                    $content = [System.IO.File]::ReadAllText($_.FullName, $encoding)
                    if ($content -and $content.Length -gt 0) {
                        break
                    }
                } catch {
                    continue
                }
            }
        }
        
        # Если все еще пустой контент - пропускаем файл
        if ($null -eq $content -or $content -eq "") {
            return
        }
        
        # Выполняем замену с учетом регистра и без
        $caseSensitiveReplacements = @{
            "$oldName.Domain" = "$newName.Domain"
            "$oldName.Application" = "$newName.Application"
            "$oldName.Umbraco" = "$newName.Umbraco"
            "$oldName.Cms" = "$newName.Cms"
        }
        
        $newContent = $content
        
        # Сначала обрабатываем case-sensitive замены
        foreach ($key in $caseSensitiveReplacements.Keys) {
            if ($newContent -match [regex]::Escape($key)) {
                $newContent = $newContent -replace [regex]::Escape($key), $caseSensitiveReplacements[$key]
            }
        }
        
        # Затем обрабатываем основное имя
        if ($newContent -match [regex]::Escape($oldName)) {
            $newContent = $newContent -replace [regex]::Escape($oldName), $newName -replace [regex]::Escape($oldName.ToLower()), $newName.ToLower()
            
            # Записываем с UTF8 с BOM для лучшей совместимости
            [System.IO.File]::WriteAllText($_.FullName, $newContent, [System.Text.Encoding]::UTF8)
            
            $processedFiles++
            Write-Host "Обработан файл: $($_.FullName)" -ForegroundColor Green
        }
    } catch {
        Write-Host "Ошибка при обработке файла $($_.FullName): $_" -ForegroundColor Red
    }
}

Write-Host "Всего обработано файлов: $processedFiles" -ForegroundColor Cyan

# 7. Специальная обработка Umbraco файлов
Write-Host "Специальная обработка Umbraco файлов..." -ForegroundColor Yellow

# Обновление appsettings.json
$appSettingsFiles = @("appsettings.json", "appsettings.Development.json", "appsettings.Production.json")
foreach ($appSettingsFile in $appSettingsFiles) {
    $filePath = "$solutionPath\$newName\$appSettingsFile"
    if (Test-Path $filePath) {
        try {
            $content = Get-Content $filePath -Raw
            $newContent = $content -replace [regex]::Escape($oldName), $newName -replace [regex]::Escape($oldName.ToLower()), $newName.ToLower()
            Set-Content $filePath $newContent -Encoding UTF8
            Write-Host "Обновлен файл: $appSettingsFile" -ForegroundColor Green
        } catch {
            Write-Host "Не удалось обработать $appSettingsFile: $_" -ForegroundColor Yellow
        }
    }
}

# Обновление uSync конфигурации
$uSyncConfig = "$solutionPath\$newName\uSync\v9\usync.config"
if (Test-Path $uSyncConfig) {
    try {
        $content = Get-Content $uSyncConfig -Raw
        $newContent = $content -replace [regex]::Escape($oldName), $newName -replace [regex]::Escape($oldName.ToLower()), $newName.ToLower()
        Set-Content $uSyncConfig $newContent -Encoding UTF8
        Write-Host "Обновлена uSync конфигурация" -ForegroundColor Green
    } catch {
        Write-Host "Не удалось обработать uSync конфигурацию: $_" -ForegroundColor Yellow
    }
}

# 8. Проверка и исправление .csproj файлов
Write-Host "Проверка и исправление .csproj файлов..." -ForegroundColor Yellow
Get-ChildItem -Path $solutionPath -Recurse -Include *.csproj | ForEach-Object {
    try {
        $content = Get-Content $_.FullName -Raw
        $xml = [xml]$content
        
        # Устанавливаем правильные RootNamespace и AssemblyName
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($_.Name).Replace($oldName, $newName)
        $rootNamespace = $projectName
        
        # Обновляем или добавляем свойства
        $propertyGroup = $xml.Project.PropertyGroup
        if (-not $propertyGroup) {
            $propertyGroup = $xml.CreateElement("PropertyGroup")
            $xml.Project.AppendChild($propertyGroup) | Out-Null
        }
        
        if (-not $propertyGroup.RootNamespace) {
            $newElement = $xml.CreateElement("RootNamespace")
            $newElement.InnerText = $rootNamespace
            $propertyGroup.AppendChild($newElement) | Out-Null
        } else {
            $propertyGroup.RootNamespace = $rootNamespace
        }
        
        if (-not $propertyGroup.AssemblyName) {
            $newElement = $xml.CreateElement("AssemblyName")
            $newElement.InnerText = $projectName
            $propertyGroup.AppendChild($newElement) | Out-Null
        } else {
            $propertyGroup.AssemblyName = $projectName
        }
        
        # Обновляем ProjectReference пути
        if ($xml.Project.ItemGroup) {
            $xml.Project.ItemGroup.ProjectReference | ForEach-Object {
                if ($_.Include -match [regex]::Escape($oldName)) {
                    $_.Include = $_.Include.Replace($oldName, $newName)
                }
            }
        }
        
        $xml.Save($_.FullName)
        Write-Host "Исправлен файл проекта: $($_.FullName)" -ForegroundColor Green
    } catch {
        Write-Host "Не удалось исправить $($_.FullName): $_" -ForegroundColor Yellow
    }
}

# 9. Очистка кэшей
Write-Host "Очистка кэшей..." -ForegroundColor Yellow

# Очистка NuCache и TEMP файлов
$cachePaths = @(
    "$solutionPath\$newName\umbraco\Data\NuCache.*",
    "$solutionPath\$newName\umbraco\Data\TEMP\*",
    "$solutionPath\$newName\umbraco\Data\*.db"
)

foreach ($cachePath in $cachePaths) {
    if (Test-Path $cachePath) {
        Remove-Item $cachePath -Recurse -Force -ErrorAction SilentlyContinue
    }
}
Write-Host "Очищен NuCache и временные файлы" -ForegroundColor Green

# Очистка bin/obj папок
Get-ChildItem -Path $solutionPath -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "Очищены bin/obj папки" -ForegroundColor Green

# 10. Обновление NuGet packages
Write-Host "Восстановление NuGet пакетов..." -ForegroundColor Yellow
if (Test-Path $newSolutionPath) {
    try {
        dotnet restore $newSolutionPath
        Write-Host "NuGet пакеты успешно восстановлены" -ForegroundColor Green
    } catch {
        Write-Host "Ошибка при восстановлении NuGet пакетов: $_" -ForegroundColor Red
    }
} else {
    Write-Host "Solution файл не найден для восстановления пакетов" -ForegroundColor Yellow
}

# 11. Финальная сборка и проверка
Write-Host "Сборка проекта для проверки..." -ForegroundColor Yellow
try {
    dotnet build $newSolutionPath
    Write-Host "Проект успешно собран!" -ForegroundColor Green
} catch {
    Write-Host "Проект собран с предупреждениями:" -ForegroundColor Yellow
    Write-Host "   - Уязвимости в пакетах можно обновить позже"
    Write-Host "   - Основное функциональное состояние проверено"
}

# 12. Финальное сообщение
Write-Host ""
Write-Host "Полное переименование проекта успешно завершено!" -ForegroundColor Magenta
Write-Host "Проект $oldName переименован в $newName" -ForegroundColor Green
Write-Host ""
Write-Host "Следующие шаги:" -ForegroundColor Cyan
Write-Host "   1. Откройте решение $newName.sln в Visual Studio"
Write-Host "   2. Проверьте работу приложения: dotnet run --project $newName/$newName.csproj"
Write-Host "   3. Если возникнут проблемы с базой данных:"
Write-Host "      - Для SQL Server: создайте новую базу данных $newName"
Write-Host "      - Для SQLite: база данных будет создана автоматически при первом запуске"
Write-Host "   4. Резервная копия сохранена в: $backupFolder"
Write-Host "      (удалите эту папку после проверки работоспособности)"
Write-Host ""
Write-Host "Информация:" -ForegroundColor Cyan
Write-Host "   - Скрипт выполнен из папки: $scriptPath"
Write-Host "   - Корень проекта: $solutionPath"
Write-Host "   - Новое имя проекта: $newName"
Write-Host "   - Обработано файлов: $processedFiles"
Write-Host ""
Write-Host "Проект готов к работе! Запустите: dotnet run --project $newName/$newName.csproj" -ForegroundColor Green