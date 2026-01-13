angular.module("umbraco").controller("oneColumnSectionController", function ($scope) {
    if ($scope.block.settingsData.imageTransform != null) {
        switch (String($scope.block.settingsData.imageTransform)) {
            case "Обрезать":
                {
                    $scope.backgroundSizeStyle = "cover";
                    break;
                }
            case "Вписать в контейнер":
                {
                    $scope.backgroundSizeStyle = "contain";
                    break;
                }
            case "Оставить как есть":
                {
                    $scope.backgroundSizeStyle = "auto";
                    break;
                }
        }
    }

    $scope.backgroundColor = ($scope.block.settingsData.backgroundColor != null) ? '#' + $scope.block.settingsData.backgroundColor.value : '';
    
});
