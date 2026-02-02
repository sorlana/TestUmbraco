@echo off
chcp 1251 > nul
setlocal enabledelayedexpansion

:: Получаем путь к текущей папке
set "current_dir=%~dp0"
cd /d "%current_dir%"

:: Запрашиваем у пользователя папки для обработки
echo.
set /p "folder_list=Введите папки для обработки через запятую (например: src,lib,utils): "
echo.

:: Преобразуем список папок в массив
set "folder_count=0"

:: Очищаем предыдущий список папок
for %%i in (folder_*) do set "%%i="

:: Разделяем папки по запятой и обрабатываем каждую
set "remaining=!folder_list!"
:parse_loop
for /f "tokens=1* delims=," %%a in ("!remaining!") do (
    set "current_folder=%%a"
    set "remaining=%%b"
)

:: Очищаем текущую папку от пробелов
set "current_folder=!current_folder: =!"
if "!current_folder!"=="" goto :end_parse

:: Нормализуем слеши
set "current_folder=!current_folder:/=\!"

:: Добавляем завершающий слеш, если его нет
if not "!current_folder:~-1!"=="\" set "current_folder=!current_folder!\"

:: Сохраняем папку
set "folder_!folder_count!=!current_folder!"
set /a folder_count+=1

if defined remaining goto :parse_loop
:end_parse

:: Проверяем, были ли указаны папки
if !folder_count!==0 (
    echo Не указаны папки для обработки.
    echo Завершение работы скрипта.
    pause
    exit /b 1
)

:: Выводим список обрабатываемых папок
echo Обрабатываемые папки:
for /l %%i in (0,1,!folder_count!) do (
    if defined folder_%%i (
        set "folder=!folder_%%i!"
        echo   !folder!
    )
)
echo.

:: Полностью очищаем или создаем новый файл context.txt
> context.txt echo.

:: Создаем структуру ТОЛЬКО для указанных папок
echo Структура указанных папок: >> context.txt
echo ========================== >> context.txt
echo. >> context.txt

:: Для каждой указанной папки выводим ее структуру
for /l %%i in (0,1,!folder_count!) do (
    if defined folder_%%i (
        set "folder=!folder_%%i!"
        
        :: Проверяем, существует ли папка
        if exist "!folder!" (
            echo Папка: !folder! >> context.txt
            echo. >> context.txt
            
            :: Используем tree для отображения структуры папки
            tree "!folder!" /F > temp_full.txt 2>&1
            
            :: Пропускаем первые 3 строки (заголовок тома) и сохраняем остальное
            more +3 temp_full.txt > temp_tree.txt 2>nul || (
                copy /y temp_full.txt temp_tree.txt >nul
            )
            
            :: Фильтруем временный файл, исключая игнорируемые файлы
            findstr /v /i /c:"context.bat" /c:"context.txt" /c:".git" /c:"node_modules" temp_tree.txt >> context.txt
            
            :: Удаляем временные файлы
            del temp_full.txt >nul 2>&1
            del temp_tree.txt >nul 2>&1
            
            echo. >> context.txt
        ) else (
            echo Папка "!folder!" не найдена! >> context.txt
            echo. >> context.txt
        )
    )
)

echo ========================== >> context.txt
echo. >> context.txt

:: Добавляем заголовок для содержимого файлов
echo Содержимое файлов: >> context.txt
echo ================= >> context.txt
echo. >> context.txt

:: Создаем временный файл для отслеживания уже обработанных файлов
set "temp_processed=%temp%\processed_%random%.txt"
> "!temp_processed!" echo.

:: Обрабатываем файлы ТОЛЬКО из указанных папок
set "file_count=0"
for /l %%i in (0,1,!folder_count!) do (
    if defined folder_%%i (
        set "folder=!folder_%%i!"
        
        :: Проверяем, существует ли папка
        if exist "!folder!" (
            echo Обработка файлов из папки: !folder!
            
            :: Рекурсивно обрабатываем все файлы в текущей папке и подпапках
            for /r "!folder!" %%f in (*.*) do (
                set "file_path=%%f"
                set "file_name=%%~nxf"
                set "file_ext=%%~xf"
                
                :: Проверяем, не обрабатывали ли мы уже этот файл
                findstr /x /c:"%%f" "!temp_processed!" >nul
                if errorlevel 1 (
                    :: Файл еще не обработан
                    echo %%f>> "!temp_processed!"
                    
                    :: Приводим расширение к нижнему регистру для сравнения
                    set "file_ext_lower=!file_ext!"
                    if defined file_ext_lower (
                        set "file_ext_lower=!file_ext_lower:.=!"
                        set "file_ext_lower=!file_ext_lower:,=!"
                        set "file_ext_lower=!file_ext_lower: =!"
                        set "file_ext_lower=!file_ext_lower:~0,4!"
                    )
                    
                    :: Проверяем, нужно ли игнорировать файл
                    set "ignore_file=0"
                    
                    :: Игнорируем служебные файлы
                    if /i "!file_name!"=="context.bat" set "ignore_file=1"
                    if /i "!file_name!"=="context.txt" set "ignore_file=1"
                    
                    :: Игнорируем графические файлы
                    if /i "!file_ext_lower!"=="jpg" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="jpeg" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="png" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="svg" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="gif" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="bmp" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="webp" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="ico" set "ignore_file=1"
                    
                    :: Игнорируем бинарные и системные файлы
                    if /i "!file_ext_lower!"=="exe" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="dll" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="zip" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="rar" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="7z" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="pdf" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="doc" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="docx" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="xls" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="xlsx" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="class" set "ignore_file=1"
                    if /i "!file_ext_lower!"=="jar" set "ignore_file=1"
                    
                    :: Если файл нужно игнорировать, пропускаем его
                    if "!ignore_file!"=="1" (
                        echo Игнорируем файл: %%f
                    ) else (
                        echo Обработка файла: %%f
                        set /a file_count+=1
                        
                        :: Получаем относительный путь от текущей папки
                        set "relative_path=%%f"
                        set "relative_path=!relative_path:%current_dir%=!"
                        
                        :: Убираем возможный начальный обратный слэш
                        if "!relative_path:~0,1!"=="\" set "relative_path=!relative_path:~1!"
                        
                        :: Записываем заголовок файла
                        echo. >> context.txt
                        echo ==================================== >> context.txt
                        echo Файл: !relative_path! >> context.txt
                        echo ==================================== >> context.txt
                        echo. >> context.txt
                        
                        :: Читаем содержимое файла с помощью TYPE - простой и надежный способ
                        type "%%f" >> context.txt 2>nul
                        if errorlevel 1 (
                            echo [Не удалось прочитать содержимое файла] >> context.txt
                        )
                        
                        :: Добавляем разделитель после каждого файла
                        echo. >> context.txt
                        echo. >> context.txt
                    )
                )
            )
        ) else (
            echo Папка "!folder!" не найдена, файлы не обрабатываются.
        )
    )
)

:: Удаляем временный файл
del "!temp_processed!" >nul 2>&1

echo.
if !file_count! GTR 0 (
    echo Файл context.txt успешно создан и заполнен!
    echo Обработано файлов: !file_count!
) else (
    echo ВНИМАНИЕ: Файл context.txt создан, но содержимое файлов не было добавлено!
    echo Проверьте, что указанные папки существуют и содержат текстовые файлы.
    echo Возможные причины:
    echo   1. Указаны несуществующие папки
    echo   2. Файлы в папках имеют расширения, которые игнорируются
    echo   3. В папках нет файлов
)
echo Путь к файлу: %current_dir%context.txt
echo.
echo Игнорируемые файлы:
echo   - context.bat, context.txt
echo   - Графические файлы: jpg, jpeg, png, svg, gif, bmp, webp, ico
echo   - Бинарные файлы: exe, dll, zip, rar, 7z, pdf
echo   - Офисные файлы: doc, docx, xls, xlsx
echo   - Системные файлы: class, jar
echo.
echo Обработанные папки:
for /l %%i in (0,1,!folder_count!) do (
    if defined folder_%%i (
        set "folder=!folder_%%i!"
        if exist "!folder!" (
            echo   ✓ !folder!
        ) else (
            echo   ✗ !folder! (не найдена)
        )
    )
)

endlocal
pause