angular.module("umbraco").controller("richTextController", function ($scope, $sce) {
    if ($scope.block.data.richText != null) {
        $scope.myHTML = $sce.trustAsHtml($scope.block.data.richText.markup);
    }
});