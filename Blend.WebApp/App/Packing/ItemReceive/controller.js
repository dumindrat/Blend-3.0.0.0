(function () {
    var urlDocument = 'api/Document';

    app.controller('itemReceiveController', function ($http, $scope, $rootScope, $route, $modal, $filter, $timeout, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        $scope.buttonstatus = 'Create';
        $scope.New = {};
        $scope.rndata = {};
        $scope.searchstr = '';
        init();
        function init() {
            $scope.recordCount = 'Loading..';
            $scope.rndata = [];
        }

        $rootScope.$on('IRN_EVENT', function (event, data) {
            getItemReceiveNotes('all');
        });

        getItemReceiveNotes('all');

        function getItemReceiveNotes(searchstr) {
            init();
            var blendNo = globalService.replaceSlash(searchstr);

            httpService.getdata('api/ItemReceive/GetAllReceiveNotes/' + searchstr, function (results) {
                $scope.data = results.data;
                $scope.recordCount = results.data.length;
            });
        };



        $scope.searchdata = function searchdata() {
            if (_timeout) {
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getItemReceiveNotes($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;

            }, loadingDelay);
        };

        $scope.print = function (obj) {
            var response = {};
            $http.post('api/IRPrint', obj, { responseType: 'arraybuffer' }).success(function (data) {
                var file = new Blob([data], { type: 'application/pdf' });
                var fileURL = URL.createObjectURL(file);
                window.open(fileURL);
                $scope.dataLoading = false;


            }).error(function (data) {
                alert('Error in processing report')
                $scope.dataLoading = false;
            });

        };
        
        $scope.openCreateView = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Packing/ItemReceive/Create.html' + '?ver=123',
                controller: 'irnCreateController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            modalInstance.result.then(function (selectedObject) {
            });
        };

        $scope.openUpdateItemView = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Packing/ItemReceive/ReceiveNoteItems.html' + '?ver=123',
                controller: 'updateItemController',
                windowClass: 'app-modal-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            modalInstance.result.then(function (selectedObject) {
            });
        };

    });

    app.controller('irnCreateController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.buttonstatus = 'Create';
        $scope.dataLoading = false;

        var systemDate = globalService.getDate();
        var completeDate = globalService.convertDateyyyymmdd(systemDate);
        SetNew();

        function SetNew() {
            $scope.New = { 'PoId': 0, 'ReceiveNoteNo': '', 'IsAutoDocNo': false };
        }
        if (!angular.isUndefined(item)) {
            $scope.New = item;
            $scope.buttonstatus = 'Update';
        }

        SetTransaction(item);
        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        globalService.openLoadingWindow('Loading..');
        AutoGenNo();
        function AutoGenNo() {
            //$scope.showLoader = true;
            globalService.openLoadingWindow('Loading..');

            httpService.getdata(urlDocument + '/GetDocumentNo/' + 4, function (results) {
                $scope.New.ReceiveNoteNo = results.data.FormattedDocNo;
                angular.element(document.getElementById('irnno'))[0].disabled = true;
                $scope.showLoader = false;
                $scope.$emit('LOAD_DATA_EVENT', true);

            });
        }

        $scope.setAutoDocNo = function (status) {
            if (status === true) {
                AutoGenNo();
            }
            else {
                angular.element(document.getElementById('irnno'))[0].disabled = false;
                $scope.New.ReceiveNoteNo = '';
                angular.element('#' + 'irnno').focus()
            }
        };

        function SetTransaction(obj) {

            $scope.New.IsAutoDocNo = true;
            AutoGenNo();
            $scope.buttonstatus = "Create";
        };

        getPOList('all');

        function getPOList(searchstr) {
            httpService.getdata('api/ItemReceive/GetPo/' + searchstr, function (results) {
                $scope.podata = results.data;
                $scope.$emit('LOAD_DATA_EVENT', true);

            });

        };

        $scope.save = function () {
            if ($scope.validate($scope.New)) {
                httpService.savedata('api/ItemReceive', $scope.New, function (response) {
                    $scope.message = '';
                    getPOList('all');
                    if (response.status == 200) {
                        $scope.messageclass = "alert alert-success";
                    }
                    $scope.message = response.message;
                    $scope.$emit('IRN_EVENT', true);
                    $scope.dataLoading = false;
                    $scope.New = {};

                })
            }
        };

        $scope.validate = function (item) {
            if (item.ReceiveNoteNo == undefined || item.ReceiveNoteNo == 0) {
                alert('Invalid Received Note No');
                return false;
            }
            if (item.PoId == undefined || item.PoId == 0) {
                alert('Invalid Purchase Order');
                return false;
            }
            return true;
        }

    });
})();