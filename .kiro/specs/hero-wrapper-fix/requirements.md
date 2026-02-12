# Требования: Исправление лишних оберток в Hero компоненте

## Проблема

Hero компонент рендерится с тремя лишними обертками `<div class="container"><div class="row"><div class="col-12">` вокруг семантического тега `<section>`.

### Текущая структура HTML:
```html
<div class="container">
  <div class="row">
    <div class="col-12">
      <section class="text-inverse-... bg-media-... lazy-bg overlay-...">
        <div class="container">
          <div class="hero-content">
            <!-- Контент hero -->
          </div>
        </div>
      </section>
    </div>
  </div>
</div>
```

### Желаемая структура HTML:
```html
<section class="text-inverse-... bg-media-... lazy-bg overlay-...">
  <div class="container">
    <div class="hero-content">
      <!-- Контент hero -->
    </div>
  </div>
</section>
```

## Причина проблемы

Анализ кода показал:

1. **`TestUmbraco/Views/Partials/blockgrid/default.cshtml`** - оборачивает все блоки (кроме `gridSection`) в структуру `container > row > col-{span}`
2. **`TestUmbraco/Views/Partials/blockgrid/Components/hero.cshtml`** - создает семантический тег `<section>` через partial `_BackgroundClasses`
3. Hero компонент должен рендериться как полноширинный блок (как `gridSection`), но обрабатывается как обычный блок

## Пользовательские истории

### 1. Как разработчик, я хочу, чтобы Hero компонент рендерился без лишних оберток
**Критерии приемки:**
- 1.1 Hero компонент должен рендериться напрямую без оберток `container/row/col`
- 1.2 Семантический тег `<section>` должен быть корневым элементом компонента
- 1.3 Внутри `<section>` должен быть только один `<div class="container">` с контентом
- 1.4 Все существующие стили и классы должны применяться корректно

### 2. Как разработчик, я хочу сохранить совместимость с другими компонентами
**Критерии приемки:**
- 2.1 Изменения не должны влиять на рендеринг других компонентов (кроме hero)
- 2.2 Компонент `gridSection` должен продолжать работать как раньше
- 2.3 Обычные блоки должны продолжать оборачиваться в `container/row/col`

### 3. Как разработчик, я хочу, чтобы решение было расширяемым
**Критерии приемки:**
- 3.1 Если в будущем появятся другие полноширинные компоненты, их легко можно будет добавить в список исключений
- 3.2 Код должен быть понятным и поддерживаемым

## Технические требования

### Изменения в `default.cshtml`

Необходимо добавить проверку для hero компонента аналогично gridSection:

```csharp
if (item.Content.ContentType.Alias == "gridSection" || item.Content.ContentType.Alias == "hero")
{
    // Рендерим напрямую без оберток
    @await Html.PartialAsync($"blockgrid/Components/{item.Content.ContentType.Alias}", item)
}
else
{
    // Обычные блоки оборачиваем
    <div class="container">
        <div class="row">
            <div class="col-@item.ColumnSpan">
                @await Html.PartialAsync($"blockgrid/Components/{item.Content.ContentType.Alias}", item)
            </div>
        </div>
    </div>
}
```

### Проверка результата

После внесения изменений необходимо проверить:
1. Hero компонент рендерится без лишних оберток
2. Фоновые изображения/видео применяются корректно
3. Классы textInverse, overlay и другие стили работают
4. Другие компоненты не затронуты изменениями

## Ограничения

- Изменения должны быть минимальными
- Не должны затрагивать логику `_BackgroundClasses.cshtml`
- Не должны изменять структуру hero.cshtml

## Зависимости

- Umbraco CMS
- Существующая структура BlockGrid
- Partial `_BackgroundClasses.cshtml`
