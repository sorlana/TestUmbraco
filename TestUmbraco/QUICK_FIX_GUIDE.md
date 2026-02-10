# Быстрое решение проблем Backoffice

## 🔴 BlockList не отображается

**Решение:** Block Preview для BlockList **ВКЛЮЧЕН** в `appsettings.json`

```json
"BlockList": {
  "Enabled": true,
  "Stylesheets": [
    "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css",
    "/css/style.css",
    "/css/backgrounds.css",
    "/css/backoffice-preview.css"
  ]
}
```

Views находятся в: `Views/Partials/blocklist/`

---

## 🔴 Не видны фоны и overlay в Block Preview

**Решение:** Добавлен специальный CSS файл

Проверьте в `appsettings.json`:

```json
"BlockGrid": {
  "Stylesheets": [
    "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css",
    "/css/style.css",
    "/css/backgrounds.css",
    "/css/backoffice-preview.css"  // ← Должен быть
  ]
}
```

Файл: `wwwroot/css/backoffice-preview.css`

---

## ⚡ Быстрый перезапуск

```bash
# Остановить
Ctrl+C

# Запустить
dotnet run
```

Или Docker:

```bash
docker-compose restart
```

---

## 📋 Чеклист после перезапуска

- [ ] BlockList отображается в backoffice
- [ ] В Block Preview видны цветные фоны
- [ ] Overlay (наложения) работают
- [ ] Градиенты применяются
- [ ] Видео фоны отображаются

---

## 📚 Подробная документация

- `FIXES_APPLIED_BACKOFFICE.md` - что было исправлено
- `BACKOFFICE_PREVIEW_FIX.md` - детали про фоны
- `BLOCK_PREVIEW_SETUP.md` - настройка Block Preview
- `Views/Partials/blocklist/README.md` - про BlockList
