# Troubleshooting Menu ID Picker

## Issue: "Menu content not configured" error

### Step 1: Check Browser Console

Open browser developer tools (F12) and look for console logs:
- `MenuIdPicker connected`
- `Resolved config:`
- `Loading menu items with config:`

The logs will show what configuration is being received.

### Step 2: Check Data Type Configuration

1. Go to **Settings → Data Types**
2. Find **"Id секции (для лендинга) - Menu ID Picker"**
3. Verify these settings:
   - **Menu Content**: Should have a content item selected (e.g., Home)
   - **Menu Property Alias**: Should be `menuLanding`
4. Click **Save**

### Step 3: Get Content ID Manually

If the config still doesn't work, you can hardcode the content ID temporarily:

1. Go to **Content → Home** (or your page with menuLanding)
2. Look at the URL: `/umbraco#/content/content/edit/XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX`
3. Copy the GUID (the long ID)
4. Edit `menu-id-picker.js` and add this at the start of `loadMenuItems()`:

```javascript
async loadMenuItems() {
  try {
    this.loading = true;
    this.error = null;

    // TEMPORARY HARDCODE - Replace with your content ID
    const HARDCODED_CONTENT_ID = '8c85aff4-3b06-41b6-9ba8-8e2d37c67e5c';
    
    const config = this.getConfig();
    let menuContentId = HARDCODED_CONTENT_ID; // Use hardcoded ID
    const menuPropertyAlias = config.menuPropertyAlias || 'menuLanding';
    
    // ... rest of the code
```

### Step 4: Check API Response

In browser console, check if the API call succeeds:

```javascript
// Run this in browser console
fetch('/umbraco/management/api/v1/document/8c85aff4-3b06-41b6-9ba8-8e2d37c67e5c', {
  headers: { 'Accept': 'application/json' }
})
.then(r => r.json())
.then(data => console.log('API Response:', data));
```

### Step 5: Alternative - Use Property Editor Schema Config

If the config still doesn't pass through, we can try using the property editor schema config instead. Edit `umbraco-package.json`:

```json
{
  "type": "propertyEditorSchema",
  "alias": "My.MenuIdPicker.Schema",
  "name": "Menu ID Picker Schema",
  "meta": {
    "defaultPropertyEditorUiAlias": "My.MenuIdPicker"
  }
}
```

### Common Issues

1. **Cache**: Clear browser cache and restart Umbraco
2. **Permissions**: Ensure you're logged in with admin rights
3. **API Access**: Check that Management API is accessible
4. **Content Structure**: Verify the content has the menuLanding property with data

### Debug Info

The component now shows debug information at the bottom:
- **Config loaded**: Shows if configuration was received
- **Menu items**: Number of items loaded
- **Current value**: The selected value

If "Config loaded" shows ❌, the configuration is not being passed to the component.
