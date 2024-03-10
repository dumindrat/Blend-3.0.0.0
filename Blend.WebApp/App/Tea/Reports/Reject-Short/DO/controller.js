(function () {


    app.controller('printDONoteController', ['$scope', '$rootScope', '$http', '$modal', '$modalInstance', '$route', 'httpService',
        'globalService', function ($scope, $rootScope, $http, $modal, $modalInstance, $route, httpService, globalService) {
            var renderAction = $route.current.$$route.originalPath;
            $scope.accessLevels = globalService.getaccess(renderAction);
            var loadingDelay = globalService.getDataLoadingDelay();
            $scope.BrokerId = 0;
            $scope.BrokerData = [];
            $scope.LineData = [];
            $scope.AuthorizePerson = '';
            $scope.ForWhom = '';

            $scope.ShortNoteDto = {
                'BrokerId': 0, 'BoxNoList': $scope.LineData, 
                'ForWhom': $scope.ForWhom, 'AuthorizedPerson': $scope.AuthorizePerson
            };

            $rootScope.$on('CLOSE_POPUP', function (event, data) {
                $scope.cancel();
            });

            getBrokers("all");

            function getBrokers(searchstr) {
                $scope.dataLoading = true;
                $scope.recordCount = globalService.getDataLoadingMessage();
                httpService.getdata('api/Broker' + '/GetBrokers/' + searchstr, function (results) {
                    $scope.BrokerData = results.data;
                    $scope.dataLoading = false;

                });

            };
            $scope.cancel = function () {
                $modalInstance.dismiss('Close');
            };

            $scope.getLines = function (broker) {
                httpService.getdata('api/AdjustLine/GetAdjustedLineByBrokerAndType' + '?brokerId=' + broker + '&type=' + 1 + '&status=' + 3, function (results) {
                    $scope.ShortNoteDto.BoxNoList = results.data;
                    // $scope.disablestatus = false;

                });
            }

            $scope.print = function () {
                globalService.openLoadingWindow("DO Note");
                $scope.ShortNoteDto.BrokerId = $scope.BrokerId;
                $http.post('api/DONote', $scope.ShortNoteDto, { responseType: 'arraybuffer' }).success(function (data) {
                    var file = new Blob([data], { type: 'application/pdf' });
                    var fileURL = URL.createObjectURL(file);
                    window.open(fileURL);
                    $scope.$emit('LOAD_DATA_EVENT', true);

                }).error(function (data) {
                    alert('Error in processing report')
                    $scope.$emit('LOAD_DATA_EVENT', true);

                });
            }
        }]);


})();