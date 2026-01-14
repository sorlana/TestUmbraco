angular.module('umbraco', []).controller('richTextController', ['$scope', function($scope, $sce) {
    $scope.myHTML = $sce.trustAsHtml($scope.block.data.richText);
}]);