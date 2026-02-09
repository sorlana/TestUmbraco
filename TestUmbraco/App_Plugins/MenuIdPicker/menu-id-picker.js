import { LitElement, html, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

export default class MenuIdPickerElement extends UmbElementMixin(LitElement) {
  
  static properties = {
    value: { type: String },
    alias: { type: String },
    menuItems: { state: true },
    loading: { state: true },
    error: { state: true },
    // Configuration properties - these will be set by Umbraco
    menuContentId: { type: Array, attribute: false },
    menuPropertyAlias: { type: String, attribute: false },
    includeEmptyOption: { type: Boolean, attribute: false },
    emptyOptionText: { type: String, attribute: false }
  };

  static styles = css`
    :host {
      display: block;
    }
    
    select {
      width: 100%;
      padding: 10px;
      border: 1px solid #ced4da;
      border-radius: 3px;
      font-size: 14px;
      background: white;
      cursor: pointer;
    }
    
    select:disabled {
      background: #e9ecef;
      cursor: not-allowed;
    }
    
    .loading {
      padding: 10px;
      text-align: center;
      color: #6c757d;
    }
    
    .error {
      padding: 10px;
      background: #f8d7da;
      border: 1px solid #f5c2c7;
      border-radius: 4px;
      color: #842029;
      margin-bottom: 10px;
    }
  `;

  constructor() {
    super();
    this.value = '';
    this.menuItems = [];
    this.loading = false;
    this.error = null;
    this.menuContentId = null;
    this.menuPropertyAlias = 'menuLanding';
    this.includeEmptyOption = true;
    this.emptyOptionText = '-- Выберите значение --';
  }

  async connectedCallback() {
    super.connectedCallback();
    
    // Получаем репозиторий документов из контекста
    this.consumeContext('UMB_DOCUMENT_WORKSPACE_CONTEXT', (context) => {
      this._workspaceContext = context;
    });
    
    // Небольшая задержка, чтобы дать время Umbraco установить свойства
    setTimeout(() => {
      this.loadMenuItems();
    }, 100);
  }

  // Следим за изменениями menuContentId
  updated(changedProperties) {
    if (changedProperties.has('menuContentId') && this.menuContentId) {
      console.log('menuContentId changed:', this.menuContentId);
      this.loadMenuItems();
    }
  }

  async loadMenuItems() {
    try {
      this.loading = true;
      this.error = null;

      let contentId = this.menuContentId;
      const propertyAlias = this.menuPropertyAlias || 'menuLanding';

      // Обрабатываем формат content picker (массив с объектами)
      if (Array.isArray(contentId) && contentId.length > 0) {
        contentId = contentId[0].unique || contentId[0].id;
      }

      // Если все еще нет ID, используем хардкод из конфига
      if (!contentId) {
        contentId = '8c85aff4-3b06-41b6-9ba8-8e2d37c67e5c'; // Home page ID из конфига
      }

      // Используем Content Delivery API (публичный доступ включен)
      const response = await fetch(`/umbraco/delivery/api/v2/content/item/${contentId}`, {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error(`Failed to fetch content: ${response.status} ${response.statusText}`);
      }

      const contentData = await response.json();

      // Delivery API имеет другую структуру - свойства в properties
      const blockListData = contentData.properties?.[propertyAlias];
      
      if (!blockListData) {
        this.error = `Property "${propertyAlias}" not found in content.`;
        this.loading = false;
        return;
      }

      // Парсим данные
      let menuData;
      if (typeof blockListData === 'string') {
        menuData = JSON.parse(blockListData);
      } else {
        menuData = blockListData;
      }

      // Delivery API может возвращать данные в разных форматах
      let items = [];
      
      if (menuData.contentData && Array.isArray(menuData.contentData)) {
        items = menuData.contentData;
      } else if (Array.isArray(menuData)) {
        items = menuData;
      } else if (menuData.items && Array.isArray(menuData.items)) {
        items = menuData.items;
      } else if (menuData.blocks && Array.isArray(menuData.blocks)) {
        items = menuData.blocks;
      } else {
        this.error = 'Invalid block list structure - no recognizable array found.';
        this.loading = false;
        return;
      }

      // Извлекаем элементы меню
      if (items.length > 0) {
        this.menuItems = items
          .map(item => {
            // Функция для получения значения свойства из разных форматов
            const getValue = (alias) => {
              // Формат 1: values array
              if (item.values && Array.isArray(item.values)) {
                const valueObj = item.values.find(v => v.alias === alias);
                return valueObj?.value || '';
              }
              // Формат 2: properties object
              if (item.properties && item.properties[alias]) {
                return item.properties[alias];
              }
              // Формат 3: content.properties
              if (item.content && item.content.properties && item.content.properties[alias]) {
                return item.content.properties[alias];
              }
              // Формат 4: прямое свойство
              if (item[alias]) {
                return item[alias];
              }
              return '';
            };

            const name = getValue('nameItem');
            const idItem = getValue('idItem');
            const show = getValue('show');

            // Возвращаем только видимые элементы (если есть свойство show)
            if (show === '0' || show === false) {
              return null;
            }

            return {
              key: item.key || item.id || item.contentKey,
              name: name || 'Unnamed',
              idItem: idItem || item.key || item.id
            };
          })
          .filter(item => item !== null);

        if (this.menuItems.length === 0) {
          this.error = 'No visible menu items found in the block list.';
        }
      } else {
        this.error = 'Block list is empty.';
      }

    } catch (err) {
      this.error = `Error: ${err.message}`;
    } finally {
      this.loading = false;
    }
  }

  _onChange(e) {
    const newValue = e.target.value;
    this.value = newValue;
    this.dispatchEvent(new UmbPropertyValueChangeEvent());
  }

  render() {
    if (this.loading) {
      return html`
        <div class="loading">⏳ Загрузка...</div>
      `;
    }

    return html`
      <div>
        ${this.error ? html`
          <div class="error">
            ${this.error}
          </div>
        ` : ''}
        
        <select 
          .value=${this.value || ''}
          @change=${this._onChange}
          ?disabled=${this.menuItems.length === 0}
        >
          ${this.includeEmptyOption ? html`<option value="" ?selected=${!this.value}>${this.emptyOptionText}</option>` : ''}
          ${this.menuItems.map(item => html`
            <option 
              value=${item.idItem || item.key}
              ?selected=${this.value === (item.idItem || item.key)}
            >
              ${item.name} ${item.idItem ? `(#${item.idItem})` : ''}
            </option>
          `)}
        </select>
      </div>
    `;
  }
}

customElements.define('menu-id-picker', MenuIdPickerElement);