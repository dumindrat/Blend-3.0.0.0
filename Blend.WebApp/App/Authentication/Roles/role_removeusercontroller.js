(function () {
    var urlRoles = 'api/Role';
    var urlUerRole = 'api/UserRole';

    app.controller('removeUserController', function ($http, $scope, $modal, $modalInstance, $route, $filter, $timeout, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.SelectedRole = item.RoleDescription;
        $scope.searchstr = '';
        $scope.dataLoading = false;
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();

        getUsers("all");



        function getUsers(searchstr) {
            var data = JSON.stringify({
                id: searchstr,
                rolw: item.RoleId
            });
            $scope.dataLoading = true;
            $scope.userData = [];

            httpService.getdata(urlRoles + '/GetAddedUsers?id=' + searchstr + '&role=' + item.RoleId, function (results) {
                $scope.userData = results.data;
                var datacount = results.data.length;
                if (datacount > 0) {
                    $scope.message = "Added users (" + datacount + ")";
                    $scope.messageclass = "alert alert-success";
                }
                else {
                    $scope.message = "Added users (" + datacount + ")";
                    $scope.messageclass = "alert alert-error";
                }
                $scope.dataLoading = false;
                //$scope.$emit('LOAD_DATA_EVENT', true);
            });
        };

        $scope.searchusers = function searchusers() {
            if (_timeout) {
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getUsers($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;

            }, loadingDelay);

        };




        $scope.delete = function (id) {
            var retVal = confirm("Are you sure want to remove?");
            if (retVal == true) {
                $scope.dataLoading = true;
                httpService.deletedata(urlUerRole + '/' + id, function () {
                    
                    $scope.dataLoading = false;
                    alert('Data deleted successfully');
                    getUsers('all');
                })
            }
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        //$rootScope.openLoadingWindow = function (p) {
        //    var modalInstance = $modal.open({
        //        templateUrl: '/Utility/Loading.html',
        //        controller: 'loadingController',
        //        windowClass: 'app-modal-loading-window',
        //        backdrop: 'static',
        //        keyboard: false,
        //        resolve: {
        //            item: function () {
        //                return p;
        //            }
        //        }
        //    });
        //};

    });
})();
