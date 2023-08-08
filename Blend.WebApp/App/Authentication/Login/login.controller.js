(function () {
    'use strict';

    app.controller('LoginController',
        ['$scope', '$location', '$http', '$modal', 'AuthenticationService', 'globalService',
        function ($scope, $location, $http, $modal, AuthenticationService, globalService) {
            $scope.loginObj = {
                UserName: '',
                Password: '',
                CertId: ''
            };
            $scope.dataLoading = false;
            var clientObj = {};
            // reset login status
            AuthenticationService.ClearCredentials();



            $scope.login = function () {
                //alert('Login...');

                angular.copy($scope.loginObj, clientObj);
                $scope.dataLoading = true;
               // globalService.openLoadingWindow('Login..');

                AuthenticationService.Login($scope.loginObj, function (response) {

                    if (response.status === 200) {
                        $scope.showLoader = false;
                        //console.log(response.headers());
                        var token = response.headers('Token');
                        if (token != '') {
                            AuthenticationService.SetCredentials(clientObj.UserName, clientObj.Password, token);
                            $scope.dataLoading = true;
                            $scope.$emit('USER_LOGON', clientObj.UserName);

                            $location.path('/');
                            AuthenticationService.GetPasswordExpireInfo(clientObj.UserName);
                        }
                        else {
                            $scope.error = 'Server returns error status';
                        }
                        //$scope.dataLoading = false;
                    } else {
                        $scope.dataLoading = false;
                        $scope.error = response.message;
                        //$scope.dataLoading = false;
                    }
                    $scope.$emit('LOAD_DATA_EVENT', true);
                });
            };

            $scope.logoff = function () {
                $scope.loginObj.UserName = globalService.getcurrentuser().username;
                $scope.loginObj.Token = globalService.getcurrentuser().token;
                globalService.openLoadingWindow('Signing Out..');

                AuthenticationService.Logoff($scope.loginObj, function (response) {
                    if (response.status === 200) {

                        AuthenticationService.ClearCredentials();
                        $scope.$emit('USER_LOGOFF', true);
                        $location.path('/Login');
                        //$scope.dataLoading = false;
                    } else {
                        $scope.error = response.message;
                        //$scope.dataLoading = false;
                    }
                    $scope.$emit('LOAD_DATA_EVENT', true);

                });

            };

            $scope.cancel = function () {
                $location.path('/');
            };

            $scope.register = function () {
                $location.path('/Register');
            };


        }]);
})();
