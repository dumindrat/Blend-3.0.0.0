(function () {

    app.controller('postReportViewController', function ($http, $scope, $route, $modalInstance, $location, $sce, item, globalService) {
        $scope.dataLoading = false;
        $scope.reportTitle = item.title;
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $http.post(item.api, item.data, { responseType: 'arraybuffer' }).success(function (data) {
            $scope.dataLoading = true;

            var file = new Blob([data], { type: 'application/pdf' });
            var fileURL = URL.createObjectURL(file);
            $scope.reportContent = $sce.trustAsResourceUrl(fileURL);
            //$scope.reportContent = 'http://www.pdf995.com/samples/pdf.pdf';
            $scope.dataLoading = false;

        });
    });

    app.controller('getReportViewController', function ($http, $scope, $route, $modalInstance, $location, $sce, item, globalService) {
        $scope.dataLoading = false;
        $scope.reportTitle = item.title;
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $http.get(item.api, { responseType: 'arraybuffer' }).success(function (data) {
            $scope.dataLoading = true;

            var file = new Blob([data], { type: 'application/pdf' });
            var fileURL = URL.createObjectURL(file);
            $scope.reportContent = $sce.trustAsResourceUrl(fileURL);
            $scope.dataLoading = false;

        });
    });

})();
