(function () {
    var urlRoles = 'api/Role';

    app.controller('roleController', function ($http, $scope, $rootScope, $route, $modal, $filter, $timeout, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.buttonstatus = 'Create';
        $scope.New = {};
        $scope.roleData = {};
        $scope.searchstr = '';
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        $rootScope.$on('ROLE_EVENT', function (event, data) {
            getRoles('all');
        });

        $scope.recordCount = '( Data loading.. )';
        getRoles('all');

        function getRoles(searchstr) {

            httpService.getdata(urlRoles + '/GetAllRoles/' + searchstr, function (results) {
                $scope.roleData = results.data;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);

            });
        };

        $scope.searchrole = function searchrole() {
            if (_timeout) {
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getRoles($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;

            }, loadingDelay);

        };


        $scope.delete = function (id) {
            var retVal = confirm("Are you sure want to remove?");
            if (retVal == true) {
                httpService.deletedata(urlRoles + '/' + id, function () {
                    alert('Data deleted successfully');
                    getRoles('all');
                })
            }
        };

        $scope.gridOptions = {
            data: 'roleData',
            showGroupPanel: false,
            rowHeight: 35,
            showFooter: false,
            columnDefs: [{ field: 'RoleId', displayName: 'Id', visible: false, width: '5%' },
                { field: 'RoleDescription', displayName: 'Role Name', width: '45%' },
                {
                    displayName: 'Options', cellTemplate: '<div class="btn-group"><input class="btn btn-primary" type="button" ng-disabled="accessLevels.M" ng-click="openCreateWindow(row.entity)" name="edit"  value="Edit">' +
                        '<div class="btn-group"><input class="btn btn-default" type="button" ng-disabled="accessLevels.M" ng-click="openAccessLevelWindow(row.entity)" name="edit"  value="Access Levels">' +
                        '<div class="btn-group"><input class="btn btn-primary" type="button" ng-disabled="accessLevels.M" ng-click="openAddUserWindow(row.entity)" name="edit"  value="Add Users">' +
                        '<div class="btn-group"><input class="btn btn-primary" type="button" ng-disabled="accessLevels.M" ng-click="openRemoveUserWindow(row.entity)" name="edit"  value="Remove Users">' +
                        '&nbsp;<input type="button" class="btn btn-danger" ng-disabled="accessLevels.D" ng-click="delete(row.entity.RoleId)"  name="delete"  value="Delete">'
                    , width: '60%'
                }
            ]
        };

        $scope.openCreateWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: 'App/Authentication/Roles/CreateRole.html',
                controller: 'roleCreateController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };

        $scope.openAccessLevelWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: 'App/Authentication/Roles/AccessLevel.html',
                controller: 'roleAccessLevelController',
                windowClass: 'app-modal-sm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };

        $scope.openAddUserWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: 'App/Authentication/Roles/AddUser.html?ver=1.2',
                controller: 'addUserController',
                windowClass: 'app-modal-sm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };

        $scope.openRemoveUserWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: 'App/Authentication/Roles/RemoveUser.html',
                controller: 'removeUserController',
                windowClass: 'app-modal-sm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };
    });


})();
