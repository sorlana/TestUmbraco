# BlockList Views

Эта папка содержит views для отображения BlockList элементов.

## Структура

- `default.cshtml` - основной view для рендеринга BlockList
- `Components/` - папка с компонентами для каждого типа блока

## Использование

Для каждого типа блока в BlockList создайте соответствующий файл в папке `Components/`:

```
Components/{blockAlias}.cshtml
```

## Block Preview

Block Preview для BlockList **отключен** в `appsettings.json` (`"Enabled": false`).

Если вы хотите включить preview для BlockList:

1. Измените `"Enabled": true` в секции `BlockPreview.BlockList`
2. Создайте папку `Views/Partials/blockpreview/blocklist/`
3. Для каждого компонента создайте preview view, который вызывает основной компонент:

```cshtml
@using Umbraco.Cms.Core.Models.Blocks
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<BlockListItem>

@{
    await Html.PartialAsync("blocklist/Components/{blockAlias}", Model);
}
```

## Текущие компоненты

- `menuItemLanding.cshtml` - компонент для пунктов меню лендинга
