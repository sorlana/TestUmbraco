angular.module("umbraco").controller("divController", function ($scope) {

    if ($scope.block.settingsData.imageTransform != null) {
        switch (String($scope.block.settingsData.imageTransform[0])) {
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
    $scope.backgroundImage = ($scope.mediaItem != null) ? 'url(' + $scope.mediaItem.mediaLink + ')' : '';

    $scope.getStyle = function () {
        return {
            'background-color': ($scope.block.settingsData.backgroundColor != null) ? '#' + $scope.block.settingsData.backgroundColor.value : '',
            'background-image': ($scope.mediaItem != null) ? 'url(' + $scope.mediaItem.mediaLink + ')' : '',
            'background-size': $scope.backgroundSizeStyle
        }
    };

});
