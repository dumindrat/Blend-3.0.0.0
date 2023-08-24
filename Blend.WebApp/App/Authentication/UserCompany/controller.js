(function () {

    var urlUserCompany = 'api/Company';

    app.controller('userCompanyController', ['$scope', '$rootScope', '$location', '$http', '$modal', 'httpService', 'globalService', function ($scope, $rootScope, $location, $http, $modal, httpService, globalService) {
        $scope.disablestatus = true;
        $scope.validating = false;
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $rootScope.$on('SUBMIT_OK', function (event, data) {
            $scope.dataObjects = [];
        });

        getCompanies('all');

        function getCompanies(blendNo) {
            $scope.recordCount = globalService.getDataLoadingMessage();
            var currentUser = globalService.getcurrentuser();

            httpService.getdata(urlUserCompany + '/GetUserCompanies?id=' + currentUser.username, function (results) {
                $scope.companyData = results.data;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);

            });
        };


        $scope.setclass = function (val) {
            if (val.IsDefault) { return 'right-icon'; }
            return 'error-icon';
        };

        $scope.isDefault = function (val) {
            if (val.IsDefault) { return true; }
            return false;
        };

       $scope.cancel = function () {
            $location.path('/');
        };

        $scope.count = 0;
        $scope.lineCount = function () {
            return $scope.count = $scope.count + 1;
        }


        $scope.setCompany = function (companyId) {
            var currentUser = globalService.getcurrentuser();

            var retVal = confirm("Are you sure want to change working company? ");
            if (retVal == true) {
                globalService.openLoadingWindow('Uploading..');
                $scope.disablestatus = true;

                var objlist = [];
                var defaultCompany = { 'UserName': currentUser.username, 'CompanyId': companyId };


                $http.post('api/Company/SetAsDefaultUserCompany', defaultCompany)
                 .success(function (data, status, headers, config) {
                     $scope.$emit('LOAD_DATA_EVENT', true);
                     $scope.$emit('USER_COMPANY_CHANGE', true);
                     $scope.cancel();

                 })
                 .error(function (data, status, headers, config) {
                     if (status === 401) {
                         globalService.RequestAuthentication();
                         $location.path('/Login');
                     }
                 });
            }

        }

    }]);

})();