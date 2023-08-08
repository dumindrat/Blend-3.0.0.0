(function () {
    var urlteastandard = 'api/TeaStandard';

    app.controller('standardController', function ($http, $scope, $rootScope, $route, $modal, $filter, $timeout, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        $scope.buttonstatus = 'Create';
        $scope.New = {};
        $scope.standardData = {};
        $scope.searchstr = '';

        init();
        function init() {
            $scope.recordCount = 'Loading..';
            $scope.standardData = [];
        }

        $rootScope.$on('STANDARD_EVENT', function (event, data) {
            getStandards('all');
        });

        getStandards('all');

        function getStandards(searchstr) {
            init();
            httpService.getdata(urlteastandard + '/GetAllStandards/' + searchstr, function (results) {
                $scope.standardData = results.data;
                $scope.recordCount = results.data.length;
                $('#search').focus();
            });
        };

        $scope.searchstndard = function searchstndard() {
            getStandards($scope.searchstr === '' ? "all" : $scope.searchstr);
        };


        $scope.delete = function (id) {
            var retVal = confirm("Are you sure want to remove?");
            if (retVal == true) {
                globalService.openLoadingWindow('Removing..');

                httpService.deletedata(urlteastandard + '/' + id, function () {
                    alert('Data deleted successfully');
                    getStandards('all');
                    $scope.$emit('LOAD_DATA_EVENT', true);

                })
            }
        };

     

        $scope.openCreateWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/TeaStandards/CreateStandard.html',
                controller: 'standardCreateController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };
    });

    app.controller('standardCreateController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, item, httpService, globalService) {
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

        $scope.save = function () {
            $scope.dataLoading = true;

            if ($scope.New.StandardId > 0) {
                httpService.updatedata(urlteastandard, $scope.New, function (response) {
                    alert(response.message);
                    $scope.dataLoading = false;
                    $scope.cancel();
                })

            } else {
                httpService.savedata(urlteastandard, $scope.New, function (response) {
                    $scope.message = response.message;
                    $scope.messageclass = "alert alert-success";
                    $scope.$emit('STANDARD_EVENT', true);
                    $scope.dataLoading = false;
                    $scope.New = {};

                })

            }
        };

    });


})();
