# Руководство по настройке Document Types для конструктора форм

## Обзор

Это руководство описывает процесс создания Document Types в бэкофисе Umbraco CMS для модуля "Конструктор форм". Вам необходимо создать два Document Type:
1. **formField** - определяет структуру одного поля формы
2. **formBuilderBlock** - определяет полную конфигурацию формы с настройками

## Предварительные требования

- Доступ к бэкофису Umbraco CMS с правами администратора
- Umbraco CMS версии 10+ установлена и запущена

---

## Часть 1: Создание Document Type `formField`

### Шаг 1.1: Создание нового Document Type

1. Войдите в бэкофис Umbraco (обычно `https://ваш-сайт/umbraco`)
2. В левом меню выберите раздел **Settings** (Настройки)
3. Найдите узел **Document Types** в дереве
4. Нажмите правой кнопкой мыши на **Document Types** → выберите **Create** → **Document Type**
5. В открывшемся окне:
   - **Name**: введите `formField`
   - **Alias**: автоматически заполнится как `formField` (оставьте как есть)
   - **Icon**: выберите иконку `icon-edit` (или любую подходящую)
   - **Description**: введите "Определяет структуру одного поля формы"

### Шаг 1.2: Настройка Document Type

1. Снимите галочку **Allow as root** (Разрешить как корневой элемент)
2. Снимите галочку **Allow in navigation** (Разрешить в навигации)
3. Установите галочку **Is Element Type** (Является типом элемента) - это важно для использования в Block List!

### Шаг 1.3: Добавление свойств

Теперь добавим 6 свойств для `formField`. Для каждого свойства:
- Нажмите кнопку **Add property** (Добавить свойство)
- Заполните поля согласно таблице ниже
- Нажмите **Submit** для сохранения каждого свойства

#### Свойство 1: fieldLabel

| Поле | Значение |
|------|----------|
| **Name** | Field Label |
| **Alias** | fieldLabel |
| **Description** | Надпись над полем формы |
| **Property Editor** | Textstring |
| **Mandatory** | ✓ (обязательное) |
| **Validation** | - |

#### Свойство 2: fieldType

| Поле | Значение |
|------|----------|
| **Name** | Field Type |
| **Alias** | fieldType |
| **Description** | Тип поля формы |
| **Property Editor** | Dropdown |
| **Mandatory** | ✓ (обязательное) |
| **Configuration** | См. ниже |

**Настройка Dropdown для fieldType:**
1. После выбора Property Editor "Dropdown", нажмите на шестеренку (настройки)
2. В разделе **Add prevalue** добавьте следующие значения (по одному):
   - `text` (нажмите Add)
   - `email` (нажмите Add)
   - `phone` (нажмите Add)
   - `textarea` (нажмите Add)
   - `checkbox` (нажмите Add)
   - `radio` (нажмите Add)
   - `select` (нажмите Add)
3. Нажмите **Submit**

#### Свойство 3: fieldPlaceholder

| Поле | Значение |
|------|----------|
| **Name** | Field Placeholder |
| **Alias** | fieldPlaceholder |
| **Description** | Текст подсказки внутри поля (placeholder) |
| **Property Editor** | Textstring |
| **Mandatory** | ☐ (необязательное) |

#### Свойство 4: isRequired

| Поле | Значение |
|------|----------|
| **Name** | Is Required |
| **Alias** | isRequired |
| **Description** | Обязательное поле для заполнения |
| **Property Editor** | Toggle (или Checkbox) |
| **Mandatory** | ☐ |
| **Default Value** | false |

#### Свойство 5: validationPattern

| Поле | Значение |
|------|----------|
| **Name** | Validation Pattern |
| **Alias** | validationPattern |
| **Description** | Регулярное выражение для валидации (например: ^\d{10}$ для 10 цифр) |
| **Property Editor** | Textstring |
| **Mandatory** | ☐ (необязательное) |

#### Свойство 6: errorMessage

| Поле | Значение |
|------|----------|
| **Name** | Error Message |
| **Alias** | errorMessage |
| **Description** | Сообщение об ошибке валидации |
| **Property Editor** | Textstring |
| **Mandatory** | ☐ (необязательное) |

### Шаг 1.4: Сохранение Document Type

1. Нажмите кнопку **Save** в правом верхнем углу
2. Убедитесь, что появилось сообщение об успешном сохранении

---

## Часть 2: Создание Document Type `formBuilderBlock`

### Шаг 2.1: Создание нового Document Type

1. В разделе **Settings** → **Document Types**
2. Нажмите правой кнопкой мыши на **Document Types** → **Create** → **Document Type**
3. В открывшемся окне:
   - **Name**: введите `formBuilderBlock`
   - **Alias**: автоматически заполнится как `formBuilderBlock`
   - **Icon**: выберите иконку `icon-document` или `icon-form`
   - **Description**: введите "Блок конструктора форм с настройками отображения и отправки"

### Шаг 2.2: Настройка Document Type

1. Снимите галочку **Allow as root** (если не планируете использовать как отдельную страницу)
2. Установите галочку **Is Element Type** (если планируете использовать в Block Grid/Block List)
   - *Примечание: если хотите использовать как обычную страницу, оставьте Is Element Type выключенным*

### Шаг 2.3: Создание вкладок

Для организации свойств создадим две вкладки:

#### Создание вкладки "Form Settings"

1. Нажмите кнопку **Add tab** (Добавить вкладку)
2. **Tab name**: введите `Form Settings`
3. Нажмите **Add tab**

#### Создание вкладки "Email Settings"

1. Снова нажмите кнопку **Add tab**
2. **Tab name**: введите `Email Settings`
3. Нажмите **Add tab**

### Шаг 2.4: Добавление свойств на вкладку "Form Settings"

Убедитесь, что выбрана вкладка **Form Settings**, затем добавьте следующие свойства:

#### Свойство 1: formTitle

| Поле | Значение |
|------|----------|
| **Name** | Form Title |
| **Alias** | formTitle |
| **Description** | Заголовок формы |
| **Property Editor** | Textstring |
| **Mandatory** | ✓ (обязательное) |
| **Tab** | Form Settings |

#### Свойство 2: formDescription

| Поле | Значение |
|------|----------|
| **Name** | Form Description |
| **Alias** | formDescription |
| **Description** | Описание формы (отображается под заголовком) |
| **Property Editor** | Textarea |
| **Mandatory** | ☐ (необязательное) |
| **Tab** | Form Settings |

#### Свойство 3: formFields (Block List)

| Поле | Значение |
|------|----------|
| **Name** | Form Fields |
| **Alias** | formFields |
| **Description** | Список полей формы |
| **Property Editor** | Block List |
| **Mandatory** | ✓ (обязательное) |
| **Tab** | Form Settings |
| **Configuration** | См. ниже |

**Настройка Block List для formFields:**

1. После выбора Property Editor "Block List", нажмите на шестеренку (настройки)
2. В разделе **Available Blocks** нажмите **Add**
3. В открывшемся окне:
   - **Content model**: выберите `formField` (созданный ранее)
   - **Label**: введите `{{fieldLabel}} ({{fieldType}})`
   - **Editor Size**: выберите `Medium` или `Large`
4. Нажмите **Submit**
5. В настройках Block List:
   - **Minimum**: оставьте пустым или установите `0`
   - **Maximum**: оставьте пустым (неограниченно)
   - **Live editing mode**: можно включить для удобства
6. Нажмите **Submit**

#### Свойство 4: submitButtonText

| Поле | Значение |
|------|----------|
| **Name** | Submit Button Text |
| **Alias** | submitButtonText |
| **Description** | Текст на кнопке отправки |
| **Property Editor** | Textstring |
| **Mandatory** | ☐ (необязательное) |
| **Default Value** | Отправить |
| **Tab** | Form Settings |

### Шаг 2.5: Добавление свойств на вкладку "Email Settings"

Переключитесь на вкладку **Email Settings** и добавьте следующие свойства:

#### Свойство 5: emailRecipient

| Поле | Значение |
|------|----------|
| **Name** | Email Recipient |
| **Alias** | emailRecipient |
| **Description** | Email адрес получателя формы |
| **Property Editor** | Textstring |
| **Mandatory** | ✓ (обязательное) |
| **Validation** | Email (если доступно) |
| **Tab** | Email Settings |

#### Свойство 6: emailSubject

| Поле | Значение |
|------|----------|
| **Name** | Email Subject |
| **Alias** | emailSubject |
| **Description** | Тема письма |
| **Property Editor** | Textstring |
| **Mandatory** | ✓ (обязательное) |
| **Tab** | Email Settings |

#### Свойство 7: successMessage

| Поле | Значение |
|------|----------|
| **Name** | Success Message |
| **Alias** | successMessage |
| **Description** | Сообщение, отображаемое после успешной отправки формы |
| **Property Editor** | Textarea |
| **Mandatory** | ☐ (необязательное) |
| **Default Value** | Спасибо! Ваша форма успешно отправлена. |
| **Tab** | Email Settings |

#### Свойство 8: redirectPage

| Поле | Значение |
|------|----------|
| **Name** | Redirect Page |
| **Alias** | redirectPage |
| **Description** | Страница для редиректа после отправки (опционально) |
| **Property Editor** | Content Picker |
| **Mandatory** | ☐ (необязательное) |
| **Tab** | Email Settings |
| **Configuration** | Start node: Content root |

**Настройка Content Picker:**
1. После выбора Property Editor "Content Picker", нажмите на шестеренку
2. **Start node**: выберите корень контента или оставьте пустым
3. **Show open button**: можно включить для удобства
4. Нажмите **Submit**

### Шаг 2.6: Сохранение Document Type

1. Нажмите кнопку **Save** в правом верхнем углу
2. Убедитесь, что появилось сообщение об успешном сохранении

---

## Часть 3: Проверка настройки

### Проверка formField

1. Перейдите в **Settings** → **Document Types**
2. Найдите и откройте `formField`
3. Убедитесь, что:
   - ✓ **Is Element Type** включен
   - ✓ Все 6 свойств присутствуют
   - ✓ Dropdown `fieldType` содержит все 7 опций

### Проверка formBuilderBlock

1. Откройте `formBuilderBlock`
2. Убедитесь, что:
   - ✓ Есть две вкладки: "Form Settings" и "Email Settings"
   - ✓ На вкладке "Form Settings" 4 свойства
   - ✓ На вкладке "Email Settings" 4 свойства
   - ✓ Block List `formFields` настроен с `formField` как доступный блок

---

## Часть 4: Тестирование (опционально)

### Создание тестовой формы

1. Перейдите в раздел **Content**
2. Создайте новую страницу или откройте существующую
3. Если `formBuilderBlock` настроен как Element Type:
   - Добавьте его в Block Grid или Block List на странице
4. Если `formBuilderBlock` настроен как обычный Document Type:
   - Создайте новую страницу типа `formBuilderBlock`

### Заполнение тестовых данных

1. **Form Settings:**
   - Form Title: "Форма обратной связи"
   - Form Description: "Заполните форму, и мы свяжемся с вами"
   - Form Fields: добавьте несколько полей:
     - Поле 1: Имя (text, required)
     - Поле 2: Email (email, required)
     - Поле 3: Сообщение (textarea, required)
   - Submit Button Text: "Отправить сообщение"

2. **Email Settings:**
   - Email Recipient: ваш email
   - Email Subject: "Новое сообщение с сайта"
   - Success Message: "Спасибо! Мы получили ваше сообщение."

3. Нажмите **Save and Publish**

---

## Часть 5: Использование в Block Grid/Block List (если применимо)

Если вы хотите использовать `formBuilderBlock` в Block Grid или Block List на других страницах:

### Шаг 5.1: Настройка родительского Document Type

1. Откройте Document Type страницы, где хотите использовать формы
2. Добавьте свойство типа **Block Grid** или **Block List**
3. В настройках этого свойства:
   - Нажмите **Add** в разделе Available Blocks
   - Выберите `formBuilderBlock`
   - Настройте отображение по желанию
4. Сохраните Document Type

### Шаг 5.2: Добавление формы на страницу

1. Откройте страницу в разделе **Content**
2. Найдите свойство Block Grid/Block List
3. Нажмите **Add content**
4. Выберите `formBuilderBlock`
5. Заполните настройки формы
6. Сохраните и опубликуйте страницу

---

## Часть 6: Следующие шаги

После завершения настройки Document Types, следующие шаги включают:

1. **Создание представления (View)** - файл `.cshtml` для рендеринга формы на фронтенде
2. **Создание контроллера** - `FormBuilderController` для обработки отправки формы
3. **Настройка сервисов** - для отправки email и сохранения данных в БД
4. **Добавление JavaScript** - для client-side валидации и AJAX отправки
5. **Интеграция reCAPTCHA** - для защиты от спама

Эти шаги будут выполнены в последующих задачах проекта.

---

## Устранение неполадок

### Проблема: Не вижу formField в настройках Block List

**Решение:** Убедитесь, что у `formField` включена опция **Is Element Type**

### Проблема: Dropdown fieldType пустой

**Решение:** Откройте настройки свойства `fieldType`, убедитесь, что все 7 значений добавлены в prevalues

### Проблема: Не могу создать вкладки

**Решение:** В Umbraco 10+ вкладки создаются через кнопку "Add tab" в интерфейсе Document Type. Убедитесь, что используете правильную версию Umbraco.

### Проблема: Content Picker не работает

**Решение:** Убедитесь, что в вашем проекте есть опубликованный контент. Content Picker показывает только опубликованные страницы.

---

## Дополнительные ресурсы

- [Официальная документация Umbraco - Document Types](https://docs.umbraco.com/umbraco-cms/fundamentals/data/defining-content)
- [Официальная документация Umbraco - Block List](https://docs.umbraco.com/umbraco-cms/fundamentals/backoffice/property-editors/built-in-umbraco-property-editors/block-editor/block-list-editor)
- [Официальная документация Umbraco - Element Types](https://docs.umbraco.com/umbraco-cms/fundamentals/data/defining-content#element-types)

---

## Контрольный список

Используйте этот список для проверки выполнения всех шагов:

- [ ] Document Type `formField` создан
- [ ] У `formField` включен **Is Element Type**
- [ ] У `formField` добавлены все 6 свойств
- [ ] Dropdown `fieldType` содержит 7 опций
- [ ] Document Type `formBuilderBlock` создан
- [ ] У `formBuilderBlock` созданы две вкладки
- [ ] На вкладке "Form Settings" добавлены 4 свойства
- [ ] На вкладке "Email Settings" добавлены 4 свойства
- [ ] Block List `formFields` настроен с `formField`
- [ ] Оба Document Type сохранены
- [ ] Создана тестовая форма (опционально)

---

**Поздравляем!** Вы успешно настроили Document Types для конструктора форм Umbraco CMS. Теперь можно переходить к следующим задачам проекта.
