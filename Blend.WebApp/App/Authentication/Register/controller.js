(function () {
    'use strict';

    app.controller('RegisterController', ['$scope', '$rootScope', '$location', 'RegistrationService',
        function ($scope, $rootScope, $location, RegistrationService) {
            $scope.showerror = false;
            $scope.error = {};
            $scope.registerObj = {
                username: '',
                password: '',
            };
            $scope.showLoading = false;
            $scope.error = '';
            function Clear() {
                $scope.registerObj.username = '';
                $scope.registerObj.password = '';
                $scope.registerObj.repassword = '';
                $scope.registerObj.email = '';
                $scope.registerObj.name = '';
            };

            $scope.registerUser = function () {
                $scope.showLoading = true;
                RegistrationService.Register($scope.registerObj, function (response) {
                    if (response.status === 200) {
                        $scope.$emit('USER_REGISTER', true);
                        $scope.showLoading = false;
                        Clear();
                    }
                    else {
                        $scope.showLoading = false;
                        $scope.error = response.message;
                    }

                });
            };

            //var compareTo = function () {
            //    return {
            //        require: "ngModel",
            //        scope: {
            //            otherModelValue: "=compareTo"
            //        },
            //        link: function (scope, element, attributes, ngModel) {

            //            ngModel.$validators.compareTo = function (modelValue) {
            //                return modelValue == scope.otherModelValue;
            //            };

            //            scope.$watch("otherModelValue", function () {
            //                ngModel.$validate();
            //            });
            //        }
            //    };
            //};

            //app.directive("compareTo", compareTo);
        }]);
})();