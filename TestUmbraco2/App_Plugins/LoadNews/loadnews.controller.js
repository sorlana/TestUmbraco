angular.module("umbraco").controller("LoadNewsController", function ($scope, $http) {
    $scope.result = null;
	
    $scope.import = function () {
        $scope.loading = true;
        $http.get("/umbraco/backoffice/My/News/Import").then(function (r) {
            $scope.loading = false;
            $scope.result = r;
        });
    };
    $scope.import_type = function () {
        $scope.loading = true;
        $http.get("/umbraco/backoffice/My/News/ImportType").then(function (r) {
            $scope.loading = false;
            $scope.result = r;
        });
    };
});