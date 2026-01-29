var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
import { LitElement, html, css } from 'lit';
import { customElement } from 'lit/decorators.js';
let BackgroundSettingsElement = class BackgroundSettingsElement extends LitElement {
    static styles = css `
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
        return html `
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
};
BackgroundSettingsElement = __decorate([
    customElement('background-settings-element')
], BackgroundSettingsElement);
export { BackgroundSettingsElement };
