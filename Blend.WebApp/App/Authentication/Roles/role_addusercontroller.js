(function () {
    //var urlRoleAccess = 'api/RoleAccessLevel';
    var urlRoles = 'api/Role';
    var urlUerRole = 'api/UserRole';

    app.controller('addUserController', function ($http, $scope, $modalInstance, $route, $filter, $timeout, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.SelectedRole = item.RoleDescription;
        $scope.searchstr = '';
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        $scope.showLoader = false;

        getUsers("all");

        function getUsers(searchstr) {
            $scope.showLoader = true;
            httpService.getdata(urlRoles + '/GetUsers/' + searchstr, function (results) {
                $scope.userData = results.data;
                $scope.showLoader = false;

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


        

        $scope.save = function (userId) {
            if (userId > 0) {
                var retVal = confirm("Are you sure want to Add user ?");
                if (retVal == true) {
                    $scope.showLoader = true;
                    var userrole = { 'UserId': userId, 'RoleId': item.RoleId };
                    httpService.savedata(urlUerRole, userrole, function (response) {
                        alert(response.message);
                        getUsers("all");
                        $scope.showLoader = false;


                    })
                }

            }
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };


    });
})();
