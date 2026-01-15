/**
 * Bootstrap Grid Editor for Umbraco
 * Version 1.0.0
 */

(function() {
    'use strict';

    // Проверяем, загружены ли зависимости Umbraco
    if (typeof angular === 'undefined' || typeof umbraco === 'undefined') {
        console.error('Umbraco dependencies not loaded');
        return;
    }

    // Регистрируем Angular модуль для редактора сетки
    angular.module('umbraco').controller('BootstrapGrid.Editor.Controller', [
        '$scope',
        '$element',
        'editorService',
        'umbRequestHelper',
        'notificationsService',
        function($scope, $element, editorService, umbRequestHelper, notificationsService) {
            
            // Инициализация переменных
            const vm = this;
            vm.loading = true;
            vm.config = $scope.model.config || {};
            vm.value = $scope.model.value || {};
            
            // Настройки по умолчанию
            vm.defaults = {
                columns: 12,
                containerType: 'container',
                additionalClass: '',
                breakpoints: [
                    { label: '1920', width: 1920, prefix: 'xl', order: 1 },
                    { label: '1200', width: 1200, prefix: 'lg', order: 2 },
                    { label: '768', width: 768, prefix: 'md', order: 3 },
                    { label: '576', width: 576, prefix: 'sm', order: 4 },
                    { label: '320', width: 320, prefix: 'xs', order: 5 }
                ],
                customColumns: [],
                classes: [],
                cells: []
            };

            // Инициализация
            vm.init = function() {
                console.log('Initializing Bootstrap Grid Editor');
                
                // Загружаем конфигурации
                loadConfigurations();
                
                // Инициализируем значение, если его нет
                if (!vm.value || Object.keys(vm.value).length === 0) {
                    vm.value = {
                        columns: vm.defaults.columns,
                        containerType: vm.defaults.containerType,
                        additionalClass: vm.defaults.additionalClass,
                        cells: vm.defaults.cells
                    };
                }
                
                // Проверяем наличие ячеек
                if (!vm.value.cells || vm.value.cells.length === 0) {
                    addDefaultCell();
                }
                
                vm.loading = false;
                $scope.$applyAsync();
            };

            // Загрузка конфигураций из настроек
            function loadConfigurations() {
                if (vm.config.configurationsNodeId) {
                    umbRequestHelper.resourcePromise(
                        umbraco.resources.contentResource.getById(vm.config.configurationsNodeId),
                        'Failed to load configurations'
                    ).then(function(configNode) {
                        if (configNode && configNode.properties) {
                            // Загружаем breakpoints
                            try {
                                const breakpointsJson = configNode.properties.breakpointsList || '[]';
                                vm.breakpoints = JSON.parse(breakpointsJson);
                            } catch (e) {
                                vm.breakpoints = vm.defaults.breakpoints;
                                console.warn('Failed to parse breakpoints, using defaults');
                            }
                            
                            // Загружаем кастомные колонки
                            try {
                                const customJson = configNode.properties.customList || '[]';
                                vm.customColumns = JSON.parse(customJson);
                            } catch (e) {
                                vm.customColumns = vm.defaults.customColumns;
                                console.warn('Failed to parse custom columns, using defaults');
                            }
                            
                            // Загружаем классы
                            try {
                                const classesJson = configNode.properties.classesList || '[]';
                                vm.classes = JSON.parse(classesJson);
                            } catch (e) {
                                vm.classes = vm.defaults.classes;
                                console.warn('Failed to parse classes, using defaults');
                            }
                        }
                    }).catch(function(error) {
                        console.error('Error loading configurations:', error);
                        vm.breakpoints = vm.defaults.breakpoints;
                        vm.customColumns = vm.defaults.customColumns;
                        vm.classes = vm.defaults.classes;
                    });
                } else {
                    vm.breakpoints = vm.defaults.breakpoints;
                    vm.customColumns = vm.defaults.customColumns;
                    vm.classes = vm.defaults.classes;
                }
            }

            // Добавление ячейки по умолчанию
            function addDefaultCell() {
                if (!vm.value.cells) {
                    vm.value.cells = [];
                }
                
                const newCell = {
                    id: generateId(),
                    position: vm.value.cells.length + 1,
                    settings: {
                        width: vm.value.columns === 0 ? 'auto' : Math.floor(12 / vm.value.columns),
                        customWidth: null,
                        extraClasses: [],
                        offset: 0,
                        order: ''
                    }
                };
                
                // Инициализируем настройки для каждого breakpoint
                if (vm.breakpoints && vm.breakpoints.length > 0) {
                    vm.breakpoints.forEach(function(bp) {
                        newCell.settings[bp.prefix] = vm.value.columns === 0 ? 'auto' : Math.floor(12 / vm.value.columns);
                    });
                }
                
                vm.value.cells.push(newCell);
                updateCellPositions();
            }

            // Генерация уникального ID
            function generateId() {
                return 'cell-' + Date.now() + '-' + Math.floor(Math.random() * 1000);
            }

            // Обновление позиций ячеек
            function updateCellPositions() {
                if (!vm.value.cells) return;
                
                vm.value.cells.forEach(function(cell, index) {
                    cell.position = index + 1;
                });
            }

            // Добавление новой ячейки
            vm.addCell = function() {
                const newCell = {
                    id: generateId(),
                    position: vm.value.cells.length + 1,
                    settings: {
                        width: vm.value.columns === 0 ? 'auto' : Math.floor(12 / vm.value.columns),
                        customWidth: null,
                        extraClasses: [],
                        offset: 0,
                        order: ''
                    }
                };
                
                // Инициализируем настройки для каждого breakpoint
                if (vm.breakpoints && vm.breakpoints.length > 0) {
                    vm.breakpoints.forEach(function(bp) {
                        newCell.settings[bp.prefix] = vm.value.columns === 0 ? 'auto' : Math.floor(12 / vm.value.columns);
                    });
                }
                
                vm.value.cells.push(newCell);
                updateCellPositions();
                
                // Показываем уведомление
                notificationsService.success('Ячейка добавлена', 'Новая ячейка успешно добавлена в сетку');
            };

            // Добавление ячейки в определенную позицию
            vm.insertCell = function(position) {
                const newCell = {
                    id: generateId(),
                    position: position,
                    settings: {
                        width: vm.value.columns === 0 ? 'auto' : Math.floor(12 / vm.value.columns),
                        customWidth: null,
                        extraClasses: [],
                        offset: 0,
                        order: ''
                    }
                };
                
                // Инициализируем настройки для каждого breakpoint
                if (vm.breakpoints && vm.breakpoints.length > 0) {
                    vm.breakpoints.forEach(function(bp) {
                        newCell.settings[bp.prefix] = vm.value.columns === 0 ? 'auto' : Math.floor(12 / vm.value.columns);
                    });
                }
                
                // Вставляем ячейку на указанную позицию
                vm.value.cells.splice(position - 1, 0, newCell);
                updateCellPositions();
                
                notificationsService.success('Ячейка вставлена', 'Ячейка успешно вставлена в позицию ' + position);
            };

            // Удаление ячейки
            vm.removeCell = function(index) {
                if (!confirm('Вы уверены, что хотите удалить эту ячейку?')) {
                    return;
                }
                
                vm.value.cells.splice(index, 1);
                updateCellPositions();
                
                // Если все ячейки удалены, добавляем одну по умолчанию
                if (vm.value.cells.length === 0) {
                    addDefaultCell();
                }
                
                notificationsService.success('Ячейка удалена', 'Ячейка успешно удалена из сетки');
            };

            // Перемещение ячейки
            vm.moveCell = function(fromIndex, toIndex) {
                if (toIndex < 0 || toIndex >= vm.value.cells.length) {
                    return;
                }
                
                const cell = vm.value.cells[fromIndex];
                vm.value.cells.splice(fromIndex, 1);
                vm.value.cells.splice(toIndex, 0, cell);
                updateCellPositions();
            };

            // Открытие редактора ячейки
            vm.editCell = function(cell, index) {
                editorService.open({
                    title: 'Настройки ячейки #' + cell.position,
                    view: '/App_Plugins/BootstrapGrid/cell-editor.html',
                    size: 'large',
                    cell: angular.copy(cell),
                    breakpoints: vm.breakpoints,
                    customColumns: vm.customColumns,
                    classes: vm.classes,
                    columns: vm.value.columns,
                    submit: function(model) {
                        // Обновляем ячейку
                        vm.value.cells[index] = model.cell;
                        notificationsService.success('Ячейка обновлена', 'Настройки ячейки успешно сохранены');
                        editorService.close();
                    },
                    close: function() {
                        editorService.close();
                    }
                });
            };

            // Расчет ширины для отображения
            vm.calculateWidth = function(cell) {
                // Проверяем, есть ли кастомная настройка для этой ячейки
                const customColumn = vm.customColumns.find(function(cc) {
                    return cc.columnNumber === cell.position;
                });
                
                if (customColumn && cell.settings.customWidth) {
                    return cell.settings.customWidth + (customColumn.unit || '');
                }
                
                if (vm.value.columns === 0) {
                    return 'auto';
                }
                
                // Используем настройки для первого breakpoint
                if (vm.breakpoints && vm.breakpoints.length > 0 && cell.settings[vm.breakpoints[0].prefix]) {
                    const width = cell.settings[vm.breakpoints[0].prefix];
                    return width === 'auto' ? 'auto' : width + '/12';
                }
                
                return vm.value.columns === 0 ? 'auto' : Math.floor(12 / vm.value.columns) + '/12';
            };

            // Проверка, является ли ячейка кастомной
            vm.isCustomCell = function(cell) {
                const customColumn = vm.customColumns.find(function(cc) {
                    return cc.columnNumber === cell.position;
                });
                
                return customColumn && cell.settings.customWidth;
            };

            // Обновление ширины всех ячеек при изменении количества колонок
            vm.updateColumnWidths = function() {
                if (!vm.value.cells) return;
                
                const baseWidth = vm.value.columns === 0 ? 'auto' : Math.floor(12 / vm.value.columns);
                
                vm.value.cells.forEach(function(cell) {
                    // Обновляем настройки для каждого breakpoint
                    if (vm.breakpoints && vm.breakpoints.length > 0) {
                        vm.breakpoints.forEach(function(bp) {
                            cell.settings[bp.prefix] = baseWidth;
                        });
                    }
                    
                    // Сбрасываем кастомную ширину, если она есть
                    cell.settings.customWidth = null;
                });
            };

            // Получение классов для ячейки
            vm.getCellClasses = function(cell) {
                const classes = [];
                
                // Добавляем классы для breakpoints
                if (vm.breakpoints && vm.breakpoints.length > 0) {
                    vm.breakpoints.forEach(function(bp) {
                        if (cell.settings[bp.prefix]) {
                            const width = cell.settings[bp.prefix];
                            if (width === 'auto') {
                                classes.push('col-' + bp.prefix + '-auto');
                            } else {
                                classes.push('col-' + bp.prefix + '-' + width);
                            }
                        }
                    });
                }
                
                // Добавляем смещение
                if (cell.settings.offset && cell.settings.offset > 0) {
                    if (vm.breakpoints && vm.breakpoints.length > 0) {
                        vm.breakpoints.forEach(function(bp) {
                            classes.push('offset-' + bp.prefix + '-' + cell.settings.offset);
                        });
                    }
                }
                
                // Добавляем порядок
                if (cell.settings.order) {
                    if (vm.breakpoints && vm.breakpoints.length > 0) {
                        vm.breakpoints.forEach(function(bp) {
                            classes.push('order-' + bp.prefix + '-' + cell.settings.order);
                        });
                    }
                }
                
                // Добавляем кастомные классы
                if (cell.settings.extraClasses && cell.settings.extraClasses.length > 0) {
                    classes.push(cell.settings.extraClasses.join(' '));
                }
                
                return classes.join(' ');
            };

            // Превью сетки
            vm.getPreviewStyle = function(cell) {
                const baseWidth = vm.value.columns === 0 ? 100 : 100 / vm.value.columns;
                let width = baseWidth;
                
                // Проверяем кастомную ширину
                const customColumn = vm.customColumns.find(function(cc) {
                    return cc.columnNumber === cell.position;
                });
                
                if (customColumn && cell.settings.customWidth) {
                    if (customColumn.unit === '%') {
                        width = parseFloat(cell.settings.customWidth);
                    } else if (customColumn.unit === 'px') {
                        return {
                            'flex': '0 0 ' + cell.settings.customWidth + 'px',
                            'background': '#ffc107'
                        };
                    } else {
                        // Для /12
                        const numWidth = parseInt(cell.settings.customWidth);
                        width = (numWidth * 100) / 12;
                    }
                } else if (cell.settings.xl && cell.settings.xl !== 'auto') {
                    // Используем настройку xl
                    const numWidth = parseInt(cell.settings.xl);
                    width = (numWidth * 100) / 12;
                }
                
                const style = {
                    'flex': '0 0 calc(' + width + '% - 15px)',
                    'background': vm.isCustomCell(cell) ? '#ffc107' : '#6c757d'
                };
                
                // Добавляем смещение
                if (cell.settings.offset && cell.settings.offset > 0) {
                    style['margin-left'] = (cell.settings.offset * 100 / 12) + '%';
                }
                
                return style;
            };

            // Сохранение значения
            vm.save = function() {
                $scope.model.value = vm.value;
                notificationsService.success('Сетка сохранена', 'Конфигурация сетки успешно сохранена');
            };

            // Сброс к значениям по умолчанию
            vm.reset = function() {
                if (confirm('Вы уверены, что хотите сбросить все настройки сетки? Это действие нельзя отменить.')) {
                    vm.value = {
                        columns: vm.defaults.columns,
                        containerType: vm.defaults.containerType,
                        additionalClass: vm.defaults.additionalClass,
                        cells: []
                    };
                    addDefaultCell();
                    notificationsService.info('Сетка сброшена', 'Все настройки возвращены к значениям по умолчанию');
                }
            };

            // Инициализация при загрузке
            vm.init();
        }
    ]);

    // Регистрируем property editor контроллер
    angular.module('umbraco').controller('BootstrapGrid.PropertyEditor.Controller', [
        '$scope',
        'editorService',
        'notificationsService',
        function($scope, editorService, notificationsService) {
            
            const vm = this;
            vm.value = $scope.model.value || {};
            vm.config = $scope.model.config || {};
            
            // Открытие редактора сетки
            vm.openGridEditor = function() {
                editorService.open({
                    title: 'Редактор Bootstrap Grid',
                    view: '/App_Plugins/BootstrapGrid/grid-editor.html',
                    size: 'xxl',
                    config: vm.config,
                    value: angular.copy(vm.value),
                    submit: function(model) {
                        vm.value = model.value;
                        $scope.model.value = vm.value;
                        notificationsService.success('Сетка обновлена', 'Конфигурация сетки успешно сохранена');
                        editorService.close();
                    },
                    close: function() {
                        editorService.close();
                    }
                });
            };
            
            // Превью сетки для property editor
            vm.getPreview = function() {
                if (!vm.value || !vm.value.cells || vm.value.cells.length === 0) {
                    return '<div class="alert alert-info">Сетка не настроена</div>';
                }
                
                let html = '<div style="display: flex; flex-wrap: wrap; gap: 5px; padding: 10px;">';
                
                vm.value.cells.forEach(function(cell, index) {
                    let width = 100 / (vm.value.columns || 12);
                    
                    if (cell.settings && cell.settings.xl && cell.settings.xl !== 'auto') {
                        width = (parseInt(cell.settings.xl) * 100) / 12;
                    }
                    
                    const isCustom = vm.customColumns && vm.customColumns.some(function(cc) {
                        return cc.columnNumber === cell.position && cell.settings.customWidth;
                    });
                    
                    html += `
                        <div style="flex: 0 0 calc(${width}% - 5px);
                                   background: ${isCustom ? '#ffc107' : '#6c757d'};
                                   height: 30px;
                                   border-radius: 3px;
                                   display: flex;
                                   align-items: center;
                                   justify-content: center;
                                   color: white;
                                   font-size: 11px;">
                            ${index + 1}
                        </div>
                    `;
                });
                
                html += '</div>';
                return html;
            };
        }
    ]);

    // Регистрируем контроллер для дашборда
    angular.module('umbraco').controller('BootstrapGrid.Dashboard.Controller', [
        '$scope',
        '$controller',
        function($scope, $controller) {
            angular.extend(this, $controller('BootstrapGrid.Editor.Controller', { $scope: $scope }));
        }
    ]);

    // Глобальные функции для кнопок "+" (плюсиков)
    document.addEventListener('DOMContentLoaded', function() {
        console.log('Bootstrap Grid Editor loaded');
        
        window.BootstrapGrid = window.BootstrapGrid || {};
        
        window.BootstrapGrid.addCell = function() {
            const element = document.querySelector('[ng-controller="BootstrapGrid.Editor.Controller"]');
            if (element && angular.element(element).scope()) {
                const scope = angular.element(element).scope();
                scope.$apply(function() {
                    scope.vm.addCell();
                });
            }
        };
        
        window.BootstrapGrid.insertCell = function(position) {
            const element = document.querySelector('[ng-controller="BootstrapGrid.Editor.Controller"]');
            if (element && angular.element(element).scope()) {
                const scope = angular.element(element).scope();
                scope.$apply(function() {
                    scope.vm.insertCell(position);
                });
            }
        };
    });

})();