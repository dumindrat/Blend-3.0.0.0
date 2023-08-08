(function () {


    app.controller('printNoteController', ['$scope', '$rootScope', '$http', '$modal', 'httpService',
        'globalService', function ($scope, $rootScope, $http, $modal, httpService, globalService) {

            $rootScope.$on('CLOSE_POPUP', function (event, data) {
                $scope.cancel();
            });


            $scope.ViewShortNote = function (size) {
                var modalInstance = $modal.open({
                    templateUrl: '/App/Tea/Reports/Reject-Short/ShortNote/PrintOptions.html',
                    controller: 'printShortNoteController',
                    windowClass: 'app-modal-md-window',
                    resolve: {

                    }
                });

            }

            $scope.ViewClaimNote = function (size) {
                var modalInstance = $modal.open({
                    templateUrl: '/App/Tea/Reports/Reject-Short/ClaimNote/PrintOptions.html',
                    controller: 'printClaimNoteController',
                    windowClass: 'app-modal-sm-window',
                    resolve: {

                    }
                });

            }

            $scope.ViewDoNote = function (size) {
                var modalInstance = $modal.open({
                    templateUrl: '/App/Tea/Reports/Reject-Short/DO/PrintOptions.html',
                    controller: 'printDONoteController',
                    windowClass: 'app-modal-xsm-window',
                    resolve: {

                    }
                });

            }

        }]);

})();