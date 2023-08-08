(function () {


    app.controller('printClaimNoteController', ['$scope', '$rootScope', '$http', '$modal', '$modalInstance', '$route', 'httpService',
        'globalService', function ($scope, $rootScope, $http, $modal, $modalInstance, $route, httpService, globalService) {
            var renderAction = $route.current.$$route.originalPath;
            $scope.accessLevels = globalService.getaccess(renderAction);
            var loadingDelay = globalService.getDataLoadingDelay();
            $scope.BrokerId = 0;
            $scope.BrokerData = [];
            $scope.LineData = [];
            $scope.AuthorizePerson = '';
            $scope.ForWhom = '';
            $scope.setreasondisable = true;
            $scope.Option1 = true;

            $scope.setOpt1 = function () {
                $scope.setreasondisable = true;
                $scope.ClaimNoteDto.Reason = 'Please note that we are rejecting the above line due to chest to chest varience in the quantity of tea.Please find below our stores address and kindly make necessary arrangements to settle the above claim.'
            }
            $scope.setOpt2 = function () {
                $scope.setreasondisable = true;
                $scope.ClaimNoteDto.Reason = 'Please note that we are rejecting the above line,as the quality of tea is different to the purchase sample. The samples are available at our office for inspection.Please find below our stores address and kindly make necessary arrangements to settle the above claim.'

            }
            $scope.setOpt3 = function () {
                $scope.setreasondisable = false;
                $scope.ClaimNoteDto.Reason = ''

            }

            $scope.ClaimNoteDto = {
                'BrokerId': 0, 'BoxNoList': $scope.LineData,
                'ForWhom': $scope.ForWhom, 'AuthorizedPerson': $scope.AuthorizePerson,
                'Reason': ''
            };

            $rootScope.$on('CLOSE_POPUP', function (event, data) {
                $scope.cancel();
            });

            getBrokers();

            function getBrokers() {
                httpService.getdata('api/Broker', function (results) {
                    $scope.BrokerData = results.data;

                });

            };
            $scope.cancel = function () {
                $modalInstance.dismiss('Close');
            };

            $scope.getLines = function (broker) {
                httpService.getdata('api/AdjustLine/GetAdjustedLineByBrokerAndType' + '?brokerId=' + broker + '&type=' + 1 + '&status=' + 0, function (results) {
                    $scope.ClaimNoteDto.BoxNoList = results.data;
                });
            }

            $scope.print = function () {
                globalService.openLoadingWindow("Claim Note");
                $scope.ClaimNoteDto.BrokerId = $scope.BrokerId;
                $http.post('api/ClaimNote', $scope.ClaimNoteDto, { responseType: 'arraybuffer' }).success(function (data) {
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