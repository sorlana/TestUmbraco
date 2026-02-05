@echo off
chcp 1251 > nul
setlocal enabledelayedexpansion

:: Получаем путь к текущей папке
set "current_dir=%~dp0"
cd /d "%current_dir%"

:: Полностью очищаем или создаем новый файл context.txt
> context.txt echo.

:: Создаем структуру папок без заголовочных строк
echo Структура папки: >> context.txt
echo ================= >> context.txt

:: Сохраняем полную структуру во временный файл
tree /F > temp_full.txt 2>&1

:: Пропускаем первые 3 строки (заголовок тома) и сохраняем остальное
more +3 temp_full.txt > temp_tree.txt 2>nul || (
    copy /y temp_full.txt temp_tree.txt >nul
)

:: Фильтруем временный файл, исключая игнорируемые файлы и папки
findstr /v /i /c:"context.bat" /c:"context.txt" /c:".git" /c:"node_modules" temp_tree.txt >> context.txt

:: Удаляем временные файлы
del temp_full.txt >nul 2>&1
del temp_tree.txt >nul 2>&1

echo. >> context.txt
echo ================= >> context.txt
echo. >> context.txt

:: Счетчики файлов и папок
set file_count=0
set folder_count=0

:: Проходим по всем файлам и папкам для подсчета
for /f "tokens=*" %%a in ('dir /b /s /a-d') do (
    set "file_path=%%a"
    set "file_name=%%~nxa"
    
    :: Проверяем, нужно ли игнорировать файл
    set "ignore_file=0"
    
    :: Игнорируем служебные файлы
    if /i "!file_name!"=="context.bat" set "ignore_file=1"
    if /i "!file_name!"=="context.txt" set "ignore_file=1"
    
    :: Игнорируем графические файлы
    if /i "%%~xa"==".jpg" set "ignore_file=1"
    if /i "%%~xa"==".jpeg" set "ignore_file=1"
    if /i "%%~xa"==".png" set "ignore_file=1"
    if /i "%%~xa"==".svg" set "ignore_file=1"
    if /i "%%~xa"==".gif" set "ignore_file=1"
    if /i "%%~xa"==".bmp" set "ignore_file=1"
    if /i "%%~xa"==".webp" set "ignore_file=1"
    if /i "%%~xa"==".ico" set "ignore_file=1"
    
    :: Игнорируем бинарные и системные файлы
    if /i "%%~xa"==".exe" set "ignore_file=1"
    if /i "%%~xa"==".dll" set "ignore_file=1"
    if /i "%%~xa"==".zip" set "ignore_file=1"
    if /i "%%~xa"==".rar" set "ignore_file=1"
    if /i "%%~xa"==".7z" set "ignore_file=1"
    if /i "%%~xa"==".pdf" set "ignore_file=1"
    if /i "%%~xa"==".doc" set "ignore_file=1"
    if /i "%%~xa"==".docx" set "ignore_file=1"
    if /i "%%~xa"==".xls" set "ignore_file=1"
    if /i "%%~xa"==".xlsx" set "ignore_file=1"
    if /i "%%~xa"==".class" set "ignore_file=1"
    if /i "%%~xa"==".jar" set "ignore_file=1"
    
    :: Если файл не игнорируется, увеличиваем счетчик
    if "!ignore_file!"=="0" set /a file_count+=1
)

:: Подсчитываем папки (исключая служебные)
for /f "tokens=*" %%a in ('dir /b /s /ad') do (
    set "folder_path=%%a"
    set "folder_name=%%~nxa"
    
    :: Игнорируем служебные папки
    if /i not "!folder_name!"==".git" (
        if /i not "!folder_name!"=="node_modules" (
            set /a folder_count+=1
        )
    )
)

:: Выводим статистику
echo Статистика: >> context.txt
echo Всего папок: !folder_count! >> context.txt
echo Всего файлов: !file_count! >> context.txt
echo. >> context.txt
echo ================= >> context.txt
echo Содержимое файлов не выводится. >> context.txt
echo ================= >> context.txt

echo.
echo Файл context.txt успешно создан!
echo Путь к файлу: %current_dir%context.txt
echo.
echo Выведена только структура папок и файлов.
echo.
echo Игнорируемые папки:
echo   - .git, node_modules
echo.
echo Игнорируемые файлы:
echo   - context.bat, context.txt
echo   - Графические файлы: jpg, jpeg, png, svg, gif, bmp, webp, ico
echo   - Бинарные файлы: exe, dll, zip, rar, 7z, pdf
echo   - Офисные файлы: doc, docx, xls, xlsx
echo   - Системные файлы: class, jar

endlocal