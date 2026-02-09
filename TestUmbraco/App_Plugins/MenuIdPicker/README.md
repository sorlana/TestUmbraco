# Menu ID Picker - Custom Property Editor

A custom Umbraco property editor that allows you to select menu section IDs from a Block List property.

## Features

- Dynamically loads menu items from a specified content node's Block List property
- Displays menu item names with their IDs
- Filters out hidden menu items (where `show` = false)
- Configurable empty option
- Real-time loading with error handling

## Configuration

The data type is configured with the following settings:

1. **Menu Content** - Select the content item that contains the menu Block List (e.g., Home page)
2. **Menu Property Alias** - The alias of the Block List property (default: `menuLanding`)
3. **Include Empty Option** - Whether to show an empty option in the dropdown
4. **Empty Option Text** - Custom text for the empty option

## How It Works

1. The component fetches the configured content item via Umbraco Management API
2. Extracts the Block List data from the specified property
3. Parses the menu items (looking for `nameItem`, `idItem`, and `show` properties)
4. Displays visible menu items in a dropdown
5. Saves the selected `idItem` value

## Usage

1. **Configure the Data Type:**
   - Go to Settings → Data Types
   - Find "Id секции (для лендинга) - Menu ID Picker"
   - Set the "Menu Content" to your home page (or page containing menuLanding)
   - Set "Menu Property Alias" to `menuLanding`

2. **Add to Document Type:**
   - Add a property using this data type to your document type (e.g., GridSection)
   - The property will display a dropdown with all menu items

3. **Use in Frontend:**
   - The saved value will be the `idItem` from the selected menu item
   - Use this value to create anchor links or section navigation

## Example Menu Item Structure

```json
{
  "key": "f9324120-cbea-44f9-a159-debb5276d3df",
  "values": [
    {
      "alias": "nameItem",
      "value": "О нас"
    },
    {
      "alias": "idItem",
      "value": "about"
    },
    {
      "alias": "show",
      "value": "1"
    }
  ]
}
```

## Troubleshooting

### No menu items displayed

1. Check browser console for errors
2. Verify the content item has menu items in the Block List
3. Ensure the Block List property alias matches the configuration
4. Check that menu items have the `nameItem` and `idItem` properties

### API errors

- Ensure you're logged into Umbraco backoffice
- Check that the Management API is accessible
- Verify the content ID is correct

## Technical Details

- **Element Name:** `menu-id-picker`
- **Property Editor Alias:** `My.MenuIdPicker`
- **Base Schema:** `Umbraco.TextBox`
- **API Used:** Umbraco Management API v1
- **Framework:** Lit Element with UmbElementMixin

## Files

- `menu-id-picker.js` - Main component logic
- `menu-id-picker.css` - Styling
- `umbraco-package.json` - Package manifest
- `README.md` - This file
