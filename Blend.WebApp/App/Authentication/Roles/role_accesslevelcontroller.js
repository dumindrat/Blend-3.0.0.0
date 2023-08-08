(function () {
    var urlRoleAccess = 'api/RoleAccessLevel';
    var urlRoles = 'api/Role';

    app.controller('roleAccessLevelController', function ($http, $scope, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.SelectedRole = item.RoleDescription;
        $scope.dataLoading = false;

        getAccessLevels();

        function getAccessLevels() {
            $scope.dataLoading = true;
            $scope.accessLevelData = [];
            httpService.getdata(urlRoles + '/GetRoleAccesssLevels/' + item.RoleId, function (results) {
                $scope.accessLevelData = results.data;
                $scope.dataLoading = false;

            });
        };

        $scope.roledatacolumns = [
                   //{ text: "RoleId", predicate: "roleid", hide: true, sortable: true, dataType: "number" },
                   { text: "#", predicate: "menuid", hide: false, sortable: true },
                   { text: "Function", predicate: "menuname", hide: false, sortable: true },
                   { text: "Level", predicate: "level", hide: false, sortable: true },
                   { text: "Access Level", predicate: "accesslevel", hide: false, sortable: true },
        ];

        $scope.getAccessLevelsClass = function (accessLevel, type) {
            var index = 0;
            if (type === 'C')
                index = 0;
            if (type === 'M')
                index = 1;
            if (type === 'D')
                index = 2;
            if (type === 'V')
                index = 3;
            if (type === 'P')
                index = 4;
            return accessLevel.split("")[index] == type ? 'btn btn-primary' : 'btn btn-danger';
        }

        $scope.getAccessLevelsType = function (accessLevel, type) {
            var index = 0;
            if (type === 'C')
                index = 0;
            if (type === 'M')
                index = 1;
            if (type === 'D')
                index = 2;
            if (type === 'V')
                index = 3;
            if (type === 'P')
                index = 4;
            return accessLevel.split("")[index] == type ? true : false;
        }

        $scope.cmdvp = '#####';

        $scope.updateAccessLevel = function (menuId, type) {
            globalService.openLoadingWindow('updating..');

            var roleaccess = { 'RoleId': item.RoleId, 'MenuId': menuId, 'AccessLevel': type };
            httpService.updatedata(urlRoleAccess, roleaccess, function (response) {
                alert(response.message);
                //getAccessLevels();
                $scope.$emit('LOAD_DATA_EVENT', true);
            });
        }

        $scope.gridOptions = {
            data: 'accessLevelData',
            showGroupPanel: false,
            rowHeight: 35,
            showFooter: false,
            columnDefs: [{ field: 'RoleId', displayName: 'Id', visible: false, width: '5%' },
                { field: 'MenuId', displayName: 'MenuId', visible: false, width: '10%' },
                { field: 'MenuName', displayName: 'Menu Name', width: '60%' },
                { field: 'AccessLevel', displayName: 'Access Level', visible: false, width: '20%' },
                {
                    displayName: 'Access Levels', cellTemplate: '<div class="btn-group"><input class="btn btn-primary" type="button" ng-disabled="accessLevels.M" ng-click="openCreateWindow(row.entity)" name="edit"  value="C">' +
                        '<div class="btn-group"><input class="btn btn-primary" type="button"  ng-click="openAccessLevelWindow(row.entity)" name="edit"  value="M">' +
                        '&nbsp;<input type="button" class="btn btn-danger"  ng-click="delete(row.entity.StandardId)"  name="delete"  value="D">' +
                        '&nbsp;<input type="button" class="btn btn-danger"  ng-click="delete(row.entity.StandardId)"  name="delete"  value="V">' +
                        '&nbsp;<input type="button" class="btn btn-danger"  ng-click="delete(row.entity.StandardId)"  name="delete"  value="P">'
                    , width: '40%'
                }
            ]
        };

        $scope.ValidateLevel = function (accesslevel, type) {

        }

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

    });
})();
