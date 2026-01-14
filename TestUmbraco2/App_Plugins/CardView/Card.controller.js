angular.module("umbraco").controller("cardController", function($scope, $sce) {
    $scope.myHTML = $sce.trustAsHtml($scope.block.data.richText.markup);
    
    
    switch(String($scope.block.data.cropCard[0])) {
        case "Авто": 
        {
            $scope.btn_class = "im-auto";
            break;
        }
        case "16:9":
        {
            $scope.btn_class = "im-16-on-9";
            break;
        }
        case "4:3":
        {
            $scope.btn_class = "im-4-on-3";
            break;
        }
        case "5:6":
        {
            $scope.btn_class = "im-5-on-6";
            break;
        }
        case "2:2":
        {
            $scope.btn_class = "im-2-on-2";
            break;
        }
    }

    switch(String($scope.block.settingsData.imageRepeat)) {
        case "не повторять": 
        {
            $scope.Repeat = "no-repeat";
            break;
        }
        case "повторять по оси Х": 
        {
            $scope.Repeat = "repeat-x";
            break;
        }
        case "повторять по оси Y": 
        {
            $scope.Repeat = "repeat-y";
            break;
        }
        case "повторять по Х и Y": 
        {
            $scope.Repeat = "repeat";
            break;
        }
        default: $scope.Repeat = ""
    }

    switch(String($scope.block.settingsData.imagePositionX)) {
        case "По левому краю": 
        {
            $scope.PosX = "left";
            $scope.GorizontAlign = "aleft";
            break;
        }
        case "По правому краю": 
        {
            $scope.PosX = "right";
            $scope.GorizontAlign = "aright";
            break;
        }
        case "По центру": 
        {
            $scope.PosX = "center";
            $scope.GorizontAlign = "acenter";
            break;
        }
        
        default: $scope.PosX = ""
    }

    switch(String($scope.block.settingsData.imagePositionY)) {
        case "По верхнему краю": 
        {
            $scope.PosY = "top";
            $scope.VerticalAlign = "atop";
            break;
        }
        case "По нижнему краю": 
        {
            $scope.PosY = "bottom";
            $scope.VerticalAlign = "abottom";
            break;
        }
        case "По центру": 
        {
            $scope.PosY = "center";
            $scope.VerticalAlign = "av-center";
            break;
        }
        
        default: $scope.PosY =""
    }

    switch(String($scope.block.settingsData.imageTransform)) {
        case "Обрезать": 
        {
            $scope.Size = "cover";
            break;
        }
        case "Вписать в контейнер": 
        {
            $scope.Size = "contain";
            break;
        }
        case "Оставить как есть": 
        {
            $scope.Size = "auto";
            break;
        }
        
        default: $scope.Size = ""
    }

    if (String($scope.block.settingsData.sizeBackground) !== "") {
        $scope.Size = $scope.block.settingsData.sizeBackground + "% auto"
    }

    if (String($scope.block.settingsData.backgroundColor) !== "") {
        $scope.Color = "#" + $scope.block.settingsData.backgroundColor.value
    }

    if (String($scope.block.settingsData.contrast.value) === "ffffff") {
        $scope.Contrast = "bright-contrast"
    }

    $scope.getStyleRichText = function () {
        return {
            'width': $scope.block.data.billetWidth + '%',
            'height': $scope.block.data.billetHeight + '%',
            'background-color': $scope.block.data.billetColor
        }
    };

    $scope.getStyle = function () {
        
        return {
            'background-repeat': url

        }
    };
});