(function () {
    var urlRoles = 'api/Role';

    app.controller('roleCreateController', function ($http, $scope, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.buttonstatus = 'Create';
        $scope.dataSaving = false;
        $scope.New = {};

        if (!angular.isUndefined(item)) {
            $scope.New = item;
            $scope.buttonstatus = 'Update';
            //$scope.buttondisable = true;;

        }

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.save = function () {
            $scope.dataSaving = true;

            if ($scope.New.RoleId > 0) {
                httpService.updatedata(urlRoles, $scope.New, function (response) {
                    alert(response.message);
                    $scope.$emit('ROLE_EVENT', true);
                    $scope.dataSaving = false;
                    $scope.cancel();
                })

            } else {
                httpService.savedata(urlRoles, $scope.New, function (response) {
                    $scope.message = response.message;
                    $scope.messageclass = "alert alert-success";
                    $scope.$emit('ROLE_EVENT', true);
                    $scope.dataSaving = false;
                })

            }
        };

    });
})();
