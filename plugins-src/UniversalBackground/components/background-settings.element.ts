import { LitElement, html, css } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('background-settings-element')
export class BackgroundSettingsElement extends LitElement {
  static styles = css`
    :host {
      display: block;
      padding: 16px;
      background: #f8f9fa;
      border-radius: 8px;
      margin: 8px 0;
    }
    select {
      width: 100%;
      padding: 8px;
      margin: 8px 0;
      border: 1px solid #ddd;
      border-radius: 4px;
    }
    label {
      display: block;
      margin-bottom: 4px;
      font-weight: 500;
    }
  `;

  render() {
    return html`
      <div>
        <label for="backgroundType">Тип фона</label>
        <select id="backgroundType">
          <option value="">Выберите тип</option>
          <option value="color">Цвет</option>
          <option value="image">Изображение</option>
          <option value="gradient">Градиент</option>
        </select>
      </div>
    `;
  }
}