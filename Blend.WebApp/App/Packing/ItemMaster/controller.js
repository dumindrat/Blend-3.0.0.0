(function () {
    var urlItemMaster = 'api/Item';

    app.controller('itemController', function ($http, $scope, $rootScope, $route, $modal, $filter, $timeout, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        $scope.buttonstatus = 'Create';
        $scope.New = {};
        $scope.itemData = {};
        $scope.searchstr = '';
        $scope.uomData = [];
        init();
        function init() {
            $scope.recordCount = 'Loading..';
            $scope.itemData = [];
        }

        $rootScope.$on('ITEM_EVENT', function (event, data) {
            getItems('all');
        });

        

        getItems('all');

        function getItems(searchstr) {
            init();
            httpService.getdata(urlItemMaster + '/GetAllItems/' + searchstr, function (results) {
                $scope.itemData = results.data;
                $scope.recordCount = results.data.length;
            });
        };

        //$scope.searchitems = function searchitems() {
        //    getItems($scope.searchstr === '' ? "all" : $scope.searchstr);
        //};

        $scope.searchdata = function searchdata() {
            if (_timeout) {
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getItems($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;

            }, loadingDelay);

        };

        $scope.delete = function (id) {
            var retVal = confirm("Are you sure want to remove?");
            if (retVal == true) {
                globalService.openLoadingWindow('Removing..');

                httpService.deletedata(urlItemMaster + '/' + id, function () {
                    //alert('Data deleted successfully');
                    getItems('all');
                    $scope.$emit('LOAD_DATA_EVENT', true);

                })
            }
        };
        $scope.openCreateWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Packing/ItemMaster/CreateItem.html',
                controller: 'itemCreateController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };

        $scope.openpROPERTYWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Packing/ItemMaster/CreateItemProperty.html',
                controller: 'itemCreateController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };
    });

    app.controller('itemCreateController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.buttonstatus = 'Create';
        $scope.dataLoading = false;
        $scope.New = {};

        if (!angular.isUndefined(item)) {
            $scope.New = item;
            $scope.buttonstatus = 'Update';
            //$scope.buttondisable = true;;

        }
        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        getUOMs();

        function getUOMs() {
            httpService.getdata('api/Uom/GetUom/', function (results) {
                $scope.uomData = results.data;
            });
        };

        $scope.save = function () {
            $scope.dataLoading = true;

            if ($scope.New.Id > 0) {
                httpService.updatedata(urlItemMaster, $scope.New, function (response) {
                    alert(response.message);
                    $scope.dataLoading = false;
                    $scope.cancel();
                })

            } else {
                httpService.savedata(urlItemMaster, $scope.New, function (response) {
                    $scope.message = response.message;
                    $scope.messageclass = "alert alert-success";
                    $scope.$emit('ITEM_EVENT', true);
                    $scope.dataLoading = false;
                    $scope.New = {};

                })

            }
        };

    });


})();
