# Block Preview Configuration

Эта папка содержит preview views для отображения блоков в Umbraco backoffice с теми же стилями, что и на сайте.

## Как это работает

1. **Конфигурация** в `appsettings.json`:
   - Подключены стили Bootstrap, style.css и backgrounds.css
   - Включен preview для BlockGrid и BlockList

2. **Preview Views**:
   - Каждый файл `{blockAlias}.cshtml` соответствует типу блока
   - Все preview views просто вызывают основные компоненты из `blockgrid/Components/`
   - Это гарантирует, что preview выглядит точно так же, как на сайте

## Созданные Preview Views

- `title.cshtml` - заголовки
- `textEditor.cshtml` - текстовые блоки
- `img.cshtml` - изображения и карточки
- `video.cshtml` - видео блоки
- `separator.cshtml` - разделители
- `quote.cshtml` - цитаты
- `gridSection.cshtml` - секции
- `gridColumn.cshtml` - колонки

## Добавление новых блоков

Чтобы добавить preview для нового блока:

1. Создайте файл `{blockAlias}.cshtml` в этой папке
2. Добавьте код:

```cshtml
@using Umbraco.Cms.Core.Models.Blocks
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<BlockGridItem>

@{
    await Html.PartialAsync("blockgrid/Components/{blockAlias}", Model);
}
```

3. Перезапустите приложение

## Стили

Стили загружаются из:
- Bootstrap 5.3.0 (CDN)
- `/css/style.css` - основные стили сайта
- `/css/backgrounds.css` - динамически генерируемые фоновые стили

## Примечания

- Preview отображается в отдельном iframe с изолированными стилями
- Изменения в основных компонентах автоматически отражаются в preview
- Для применения изменений в конфигурации нужен перезапуск приложения
