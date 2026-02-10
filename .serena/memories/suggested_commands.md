# Рекомендуемые команды для разработки

## Система: Windows

### Основные команды Windows CMD/PowerShell

#### Навигация и просмотр файлов
```cmd
dir                          # Список файлов и папок
cd <путь>                    # Перейти в директорию
cd ..                        # Вернуться на уровень выше
type <файл>                  # Просмотр содержимого файла
more <файл>                  # Постраничный просмотр файла
```

#### Работа с файлами и папками
```cmd
mkdir <папка>                # Создать папку
rmdir /s /q <папка>          # Удалить папку рекурсивно
del <файл>                   # Удалить файл
copy <источник> <назначение> # Копировать файл
move <источник> <назначение> # Переместить файл
```

#### Поиск
```cmd
dir /s /b <паттерн>          # Рекурсивный поиск файлов
findstr /s /i "текст" *.cs   # Поиск текста в файлах
```

#### Git команды
```cmd
git status                   # Статус репозитория
git add .                    # Добавить все изменения
git commit -m "сообщение"    # Зафиксировать изменения
git pull                     # Получить изменения
git push                     # Отправить изменения
git log --oneline            # История коммитов
git diff                     # Просмотр изменений
```

## .NET и Umbraco команды

### Сборка и запуск проекта

#### Основные команды dotnet
```cmd
dotnet restore               # Восстановить NuGet пакеты
dotnet build                 # Собрать проект
dotnet run                   # Запустить проект (из папки TestUmbraco)
dotnet clean                 # Очистить артефакты сборки
```

#### Запуск проекта из корня
```cmd
cd TestUmbraco
dotnet run
```
Проект будет доступен по адресу: http://localhost:5000

#### Сборка в Release режиме
```cmd
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

### Entity Framework Core команды

#### Миграции базы данных
```cmd
# Из папки TestUmbraco.Domain
dotnet ef migrations add <ИмяМиграции> --startup-project ../TestUmbraco
dotnet ef database update --startup-project ../TestUmbraco
dotnet ef migrations list --startup-project ../TestUmbraco
dotnet ef migrations remove --startup-project ../TestUmbraco
```

#### Создание миграции с контекстом
```cmd
dotnet ef migrations add <ИмяМиграции> --context AppDbContext --startup-project ../TestUmbraco
```

### NuGet пакеты

#### Управление пакетами
```cmd
dotnet add package <ИмяПакета>                    # Добавить пакет
dotnet add package <ИмяПакета> --version <версия> # Добавить конкретную версию
dotnet remove package <ИмяПакета>                 # Удалить пакет
dotnet list package                               # Список установленных пакетов
dotnet list package --outdated                    # Устаревшие пакеты
```

## TypeScript и Frontend команды

### NPM команды

#### Установка зависимостей
```cmd
npm install                  # Установить все зависимости из package.json
npm install <пакет>          # Установить конкретный пакет
npm install --save-dev <пакет> # Установить dev-зависимость
```

#### Сборка TypeScript плагинов
```cmd
npm run build:plugins        # Однократная сборка TypeScript
npm run watch:plugins        # Сборка с отслеживанием изменений
```

#### Разработка
```cmd
npm run dev                  # Запуск TypeScript watch + dotnet run (concurrently)
npm start                    # Алиас для npm run dev
```

**Примечание**: `npm run dev` запускает одновременно:
- TypeScript компилятор в watch режиме
- ASP.NET Core приложение

## Docker команды

### Сборка и запуск контейнера

#### Docker Compose
```cmd
docker-compose build         # Собрать образ
docker-compose up            # Запустить контейнер
docker-compose up -d         # Запустить в фоновом режиме
docker-compose down          # Остановить и удалить контейнер
docker-compose logs -f       # Просмотр логов
docker-compose restart       # Перезапустить контейнер
```

#### Прямые Docker команды
```cmd
docker build -t testumbraco . # Собрать образ
docker run -p 801:80 testumbraco # Запустить контейнер
docker ps                    # Список запущенных контейнеров
docker logs <container_id>   # Просмотр логов контейнера
docker exec -it <container_id> bash # Войти в контейнер
```

Проект в Docker будет доступен по адресу: http://localhost:801

## Тестирование и отладка

### Проверка кода
```cmd
dotnet format                # Форматирование кода (если установлен dotnet-format)
dotnet build --no-incremental # Полная пересборка
```

### Логи
```cmd
# Логи находятся в папке TestUmbraco/Logs/
type TestUmbraco\Logs\log-<дата>.txt # Просмотр лога за конкретную дату
```

### Очистка проекта
```cmd
# Удаление всех bin и obj папок
for /d /r . %d in (bin,obj) do @if exist "%d" rd /s /q "%d"

# Или через PowerShell
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force
```

## Umbraco специфичные команды

### Доступ к бэк-офису
- URL: http://localhost:5000/umbraco
- Логин: admin@test.com
- Пароль: Test12345!

### uSync команды
uSync синхронизирует контент и настройки Umbraco через файлы в папке `uSync/v9/`.

Команды доступны через бэк-офис Umbraco:
- Settings → uSync → Import
- Settings → uSync → Export

### Очистка кэша Umbraco
```cmd
# Удалить папку кэша
rmdir /s /q TestUmbraco\App_Data\TEMP
rmdir /s /q TestUmbraco\umbraco\Data\TEMP
```

## Полезные комбинации команд

### Полная пересборка проекта
```cmd
dotnet clean
dotnet restore
dotnet build
```

### Обновление всех зависимостей
```cmd
# .NET пакеты
dotnet restore

# NPM пакеты
npm install
```

### Запуск проекта для разработки
```cmd
# Вариант 1: С TypeScript watch
npm run dev

# Вариант 2: Только .NET
cd TestUmbraco
dotnet run

# Вариант 3: Docker
docker-compose up
```

## Troubleshooting команды

### Проверка версий
```cmd
dotnet --version             # Версия .NET SDK
node --version               # Версия Node.js
npm --version                # Версия NPM
git --version                # Версия Git
docker --version             # Версия Docker
```

### Проверка портов
```cmd
netstat -ano | findstr :5000 # Проверить, занят ли порт 5000
netstat -ano | findstr :801  # Проверить, занят ли порт 801
```

### Убить процесс на порту
```cmd
# Найти PID процесса
netstat -ano | findstr :5000

# Убить процесс по PID
taskkill /PID <PID> /F
```
