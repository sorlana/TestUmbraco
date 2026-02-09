import { LitElement, html, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';

export class SimpleMenuPicker extends LitElement {
  
  static properties = {
    value: { type: String },
    config: { type: Object }
  };

  static styles = css`
    :host {
      display: block;
      padding: 15px;
      background: #f8f9fa;
      border-radius: 8px;
      border: 1px solid #dee2e6;
    }
    
    select {
      width: 100%;
      padding: 10px;
      border: 1px solid #6c757d;
      border-radius: 4px;
      font-size: 14px;
      margin-bottom: 10px;
    }
    
    .instructions {
      font-size: 12px;
      color: #6c757d;
      margin-top: 15px;
      padding: 10px;
      background: white;
      border-radius: 4px;
      border-left: 4px solid #0d6efd;
    }
    
    .instructions h4 {
      margin: 0 0 8px 0;
      color: #0d6efd;
    }
  `;

  constructor() {
    super();
    this.value = '';
    
    // Статические тестовые данные
    this.menuItems = [
      { id: '123e4567-e89b-12d3-a456-426614174000', name: 'Home' },
      { id: '223e4567-e89b-12d3-a456-426614174001', name: 'About Us' },
      { id: '323e4567-e89b-12d3-a456-426614174002', name: 'Services' },
      { id: '423e4567-e89b-12d3-a456-426614174003', name: 'Contact' }
    ];
  }

  connectedCallback() {
    super.connectedCallback();
    console.log('SimpleMenuPicker loaded');
    
    // Проверяем наличие конфигурации
    if (this.config) {
      console.log('Config received:', this.config);
    }
  }

  _onChange(e) {
    this.value = e.target.value;
    this.dispatchEvent(new UmbPropertyValueChangeEvent());
  }

  render() {
    return html`
      <div>
        <label style="display: block; margin-bottom: 8px; font-weight: 600;">
          Select Menu Item ID:
        </label>
        
        <select 
          .value=${this.value || ''}
          @change=${this._onChange}
        >
          <option value="">-- Select a menu item --</option>
          ${this.menuItems.map(item => html`
            <option value=${item.id}>${item.name} (${item.id.substring(0, 8)}...)</option>
          `)}
        </select>
        
        ${this.value ? html`
          <div style="
            margin-top: 10px; 
            padding: 8px; 
            background: #d1e7dd; 
            border-radius: 4px;
            font-size: 13px;
          ">
            ✅ Selected: <strong>${this.value}</strong>
          </div>
        ` : ''}
        
        <div class="instructions">
          <h4>ℹ️ How to get real menu item IDs:</h4>
          <ol style="margin: 0 0 0 15px; padding: 0;">
            <li>Edit the page containing your menu block list</li>
            <li>Open the block list editor (menuLanding)</li>
            <li>For each block, click "Info" button</li>
            <li>Copy the "Key" value</li>
            <li>Add it to this component's code</li>
          </ol>
        </div>
      </div>
    `;
  }
}

customElements.define('simple-menu-picker', SimpleMenuPicker);