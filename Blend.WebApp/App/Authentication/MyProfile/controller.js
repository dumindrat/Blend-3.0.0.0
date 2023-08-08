++(function () {

    //'use strict';

    app.controller('myProfileController', function ($http, $scope, $rootScope, $route, $modal, httpService, globalService) {

       

        $scope.openChangePasswordWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: '/App/Authentication/MyProfile/ChangePassword.html',
                controller: 'changePasswordController',
                windowClass: 'app-modal-sm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            //modalInstanceC.result.then(function (selectedObject) {
            //});
        };



    });

    app.controller('changePasswordController', function (Base64, $scope, $rootScope, $modalInstance, $route, $filter, $http, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.dataLoading = false;
        $scope.validateOldPassword = false;
        $scope.confPassword = false;

        $scope.disableSave = true;
        $scope.statusIcon = false;
        $scope.setclass = '';
        $scope.setstrengthclass = '';
        $scope.accessLevels = globalService.getaccess(renderAction);

        var strongRegularExp = new RegExp("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])(?=.{8,})");
        var mediumRegularExp = new RegExp("^(((?=.*[a-z])(?=.*[A-Z]))|((?=.*[a-z])(?=.*[0-9]))|((?=.*[A-Z])(?=.*[0-9])))(?=.{6,})");
        $scope.validationInputPwdText = function (value) {
            if (strongRegularExp.test(value)) {
                $scope.checkpwdStrength["background-color"] = "green";
            } else if (mediumRegularExp.test(value)) {
                $scope.checkpwdStrength["background-color"] = "orange";
            } else {
                $scope.checkpwdStrength["background-color"] = "red";
            }
        };
        $scope.checkpwdStrength = {
            "width": "100px",
            "height": "32px",
            "float": "right",
            "border-radius": "5px"
        };
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };
        $scope.obj = { 'OldPassword': '', 'NewPassword': '', 'ConfPassword': '' };
        $scope.userObj = { 'Password': '' };
        var checkconfpassword = function () {
            if ($scope.obj.NewPassword === $scope.obj.ConfPassword) {
                $scope.confPassword = true;
            }
        }
        var validPassword = function () {
            if ($scope.confPassword && $scope.validateOldPassword) {
                return true;
            }
            else {
                alert('Invalid Old Password or Invalid Confirmation');
                return false;
            }
        }
        $scope.validatepassword = function () {
            var authlogindata = Base64.encode('d45T==' + ':' + $scope.obj.OldPassword);
            $scope.dataLoading = true;

            $scope.userObj.Password = authlogindata;
            $http.post('api/ChangePassword', $scope.userObj)
               .success(function (data, status, headers, config) {
                   $scope.dataLoading = false;
                   $scope.statusIcon = true;
                   $scope.disableSave = false;

                   if (status == 200) {
                       $scope.setclass = 'right-icon';
                       $scope.validateOldPassword = true;
                   }

               })
               .error(function (data, status, headers, config) {
                   $scope.dataLoading = false;
                   $scope.setclass = 'error-icon';
                   $scope.validateOldPassword = false;
               });
        }

        $scope.changepassword = function () {
            checkconfpassword();
            if (validPassword()) {
                var retVal = confirm("Are you sure want to change the Password ?");
                if (retVal == true) {
                    var authlogindata = Base64.encode('d45T==' + ':' + $scope.obj.ConfPassword);
                    $scope.dataSaving = true;

                    $scope.userObj.Password = authlogindata;
                    $http.put('api/ChangePassword', $scope.userObj)
                       .success(function (data, status, headers, config) {
                           $scope.dataSaving = false;
                           alert('Successfully updated');

                       })
                       .error(function (data, status, headers, config) {
                           $scope.dataSaving = false;
                           $scope.disableSave = false;
                           alert('Error in updating Password');

                       });
                }
            }
        }
    });

})();