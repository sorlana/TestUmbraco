# Дизайн: Исправление лишних оберток в Hero компоненте

## Обзор решения

Проблема заключается в том, что `default.cshtml` оборачивает все компоненты (кроме `gridSection`) в структуру Bootstrap grid (`container > row > col`). Hero компонент должен быть полноширинным и рендериться напрямую, как `gridSection`.

## Архитектурное решение

### Подход: Расширение списка исключений

Добавим `hero` в список компонентов, которые рендерятся без оберток, аналогично `gridSection`.

**Преимущества:**
- Минимальные изменения кода
- Понятная логика
- Легко расширяется для других полноширинных компонентов
- Не влияет на другие компоненты

**Недостатки:**
- Требует явного перечисления компонентов

### Альтернативные подходы (отклонены)

1. **Добавить флаг в настройки компонента** - избыточно для простой задачи
2. **Изменить структуру hero.cshtml** - нарушит существующую логику фонов
3. **Убрать обертки для всех компонентов** - сломает layout других блоков

## Детальный дизайн

### Изменяемый файл

**Файл:** `TestUmbraco/Views/Partials/blockgrid/default.cshtml`

### Текущий код

```csharp
@if (Model != null && Model.Any())
{
    foreach (var item in Model)
    {
        if (item.Content.ContentType.Alias == "gridSection")
        {
            @* Grid Section рендерится напрямую *@
            @await Html.PartialAsync("blockgrid/Components/gridSection", item)
        }
        else
        {
            @* Обычные блоки оборачиваем в container и row *@
            <div class="container">
                <div class="row">
                    <div class="col-@item.ColumnSpan">
                        @await Html.PartialAsync($"blockgrid/Components/{item.Content.ContentType.Alias}", item)
                    </div>
                </div>
            </div>
        }
    }
}
```

### Новый код

```csharp
@if (Model != null && Model.Any())
{
    foreach (var item in Model)
    {
        var alias = item.Content.ContentType.Alias;
        
        @* Полноширинные компоненты рендерятся напрямую без оберток *@
        if (alias == "gridSection" || alias == "hero")
        {
            @await Html.PartialAsync($"blockgrid/Components/{alias}", item)
        }
        else
        {
            @* Обычные блоки оборачиваем в container и row *@
            <div class="container">
                <div class="row">
                    <div class="col-@item.ColumnSpan">
                        @await Html.PartialAsync($"blockgrid/Components/{alias}", item)
                    </div>
                </div>
            </div>
        }
    }
}
```

### Изменения

1. Извлекаем `alias` в переменную для читаемости
2. Добавляем проверку `|| alias == "hero"` в условие
3. Используем переменную `alias` вместо повторного обращения к `item.Content.ContentType.Alias`

## Влияние на систему

### Затронутые компоненты

- ✅ **hero.cshtml** - будет рендериться без оберток
- ✅ **gridSection.cshtml** - без изменений
- ✅ **Другие компоненты** - без изменений

### Результирующая HTML структура

**До изменений:**
```html
<div class="container">
  <div class="row">
    <div class="col-12">
      <section class="...">
        <div class="container">
          <div class="hero-content">...</div>
        </div>
      </section>
    </div>
  </div>
</div>
```

**После изменений:**
```html
<section class="...">
  <div class="container">
    <div class="hero-content">...</div>
  </div>
</section>
```

## Тестирование

### Сценарии тестирования

1. **Hero компонент рендерится корректно**
   - Проверить отсутствие лишних оберток в HTML
   - Проверить наличие всех классов на `<section>`
   - Проверить корректность структуры контента

2. **Фоновые изображения/видео работают**
   - Проверить применение фоновых изображений
   - Проверить работу lazy loading
   - Проверить overlay эффекты

3. **Стили применяются корректно**
   - Проверить textInverse классы
   - Проверить пользовательские классы из настроек
   - Проверить responsive поведение

4. **Другие компоненты не затронуты**
   - Проверить рендеринг обычных блоков с обертками
   - Проверить gridSection без изменений
   - Проверить работу ColumnSpan для обычных блоков

### Ручное тестирование

1. Открыть страницу с Hero компонентом
2. Открыть DevTools и проверить структуру DOM
3. Проверить визуальное отображение
4. Проверить на разных разрешениях экрана

## Риски и митигация

| Риск | Вероятность | Влияние | Митигация |
|------|-------------|---------|-----------|
| Сломается layout других страниц | Низкая | Высокое | Тщательное тестирование всех типов компонентов |
| Проблемы с responsive | Низкая | Среднее | Проверка на разных устройствах |
| Конфликт стилей | Низкая | Низкое | Hero уже имеет собственный container |

## Расширяемость

Для добавления других полноширинных компонентов в будущем:

```csharp
// Список полноширинных компонентов
var fullWidthComponents = new[] { "gridSection", "hero", "banner", "footer" };

if (fullWidthComponents.Contains(alias))
{
    @await Html.PartialAsync($"blockgrid/Components/{alias}", item)
}
```

Но для текущей задачи достаточно простого условия с двумя компонентами.

## Correctness Properties

### Property 1: Hero компонент не имеет лишних оберток
**Описание:** При рендеринге hero компонента, корневым элементом должен быть `<section>`, без оберток `container/row/col`.

**Формальное свойство:**
```
∀ hero_component ∈ BlockGrid:
  if hero_component.alias == "hero" then
    rendered_html.root_element == "<section>" AND
    NOT contains(rendered_html, "<div class=\"container\"><div class=\"row\"><div class=\"col-")
```

**Тестирование:** Unit test проверяет отсутствие паттерна `<div class="container"><div class="row"><div class="col-` перед `<section>`.

### Property 2: Обычные компоненты сохраняют обертки
**Описание:** Компоненты, не являющиеся `gridSection` или `hero`, должны оборачиваться в grid структуру.

**Формальное свойство:**
```
∀ component ∈ BlockGrid:
  if component.alias ∉ {"gridSection", "hero"} then
    rendered_html starts_with "<div class=\"container\"><div class=\"row\"><div class=\"col-"
```

**Тестирование:** Unit test проверяет наличие оберток для обычных компонентов.

### Property 3: Структура hero контента сохраняется
**Описание:** Внутренняя структура hero компонента (container > hero-content) должна остаться неизменной.

**Формальное свойство:**
```
∀ hero_component ∈ BlockGrid:
  if hero_component.alias == "hero" then
    contains(rendered_html, "<section.*><div class=\"container.*\"><div class=\"hero-content\">")
```

**Тестирование:** Unit test проверяет правильную вложенность элементов внутри section.

## Метрики успеха

- ✅ Hero компонент рендерится без лишних оберток
- ✅ Все существующие стили применяются корректно
- ✅ Другие компоненты работают без изменений
- ✅ Код остается читаемым и поддерживаемым
- ✅ Нет регрессий в визуальном отображении
