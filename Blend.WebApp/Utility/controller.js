(function () {
    app.controller('loadingController', function ($http, $scope, $rootScope, $modalInstance, $modal, item) {
        $scope.loadingCaption = item;
        $rootScope.$on('LOAD_DATA_EVENT', function (event, data) {
            $modalInstance.dismiss('Close');
        });
    });
})();