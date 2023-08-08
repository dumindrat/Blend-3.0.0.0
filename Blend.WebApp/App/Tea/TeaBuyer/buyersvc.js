(function () {

    var urlBuyer = 'api/Buyer';

    //Controller
    app.controller('buyerController', function ($http, $scope, $rootScope, $modal, $route, $timeout, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        $scope.searchstr = '';
        $scope.dataloading = true;
        init();
        function init() {
            $scope.recordCount = '( Data loading.. )';
        }

        $rootScope.$on('BUYER_EVENT', function (event, data) {
            getBuyers("all");
        });

        getBuyers("all");

        function getBuyers(searchstr) {
            $scope.BuyerData = [];
            $scope.dataloading = false;

            init();
            httpService.getdata(urlBuyer + '/GetAllBuyers/' + searchstr, function (results) {
                $scope.BuyerData = results.data;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);
                $scope.dataloading = false;
                $('#datasearch').focus();

            });

        };

        $scope.searchdata = function searchdata() {
            if (_timeout) {
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getBuyers($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;

            }, loadingDelay);

        };


        $scope.delete = function (id) {
            var retVal = confirm("Are you sure want to remove?");
            if (retVal == true) {
                globalService.openLoadingWindow('Removing..');
                httpService.deletedata(urlBuyer + '/' + id, function (response) {
                    if (response.status = 200) {
                        getBuyers("all");
                        $scope.$emit('LOAD_DATA_EVENT', true);
                    }
                    else {
                        $scope.$emit('LOAD_DATA_EVENT', true);
                    }

                })
            }
        };

        $scope.openCreateWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/TeaBuyer/CreateBuyer.html',
                controller: 'buyerCreateController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };


    });

    app.controller('buyerCreateController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.buttonstatus = 'Create';
        //$scope.New = { 'BuyerId': 0, 'BuyerName': '', 'Address': '', 'Email': '' };
        $scope.dataLoading = false;

        if (!angular.isUndefined(item)) {
            $scope.New = item;
            $scope.buttonstatus = 'Update';
        }
        else {
            init();
        }

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };



        function init() {
            $scope.New = { 'BuyerId': 0, 'BuyerName': '', 'Address': '', 'Email': '' };
        }


        function setdefault() {

            if ($scope.New.Address === '') {
                $scope.New.Address = 'NA';
            }
            if ($scope.New.Email === '') {
                $scope.New.Email = 'NA';
            }
        }

        $scope.save = function () {

            setdefault();
            $scope.dataLoading = true;
            if ($scope.New.BuyerId > 0) {
                httpService.updatedata(urlBuyer + '/' + $scope.New.BuyerId, $scope.New, function (response) {
                    alert('Success');
                    $scope.dataLoading = false;
                    $scope.cancel();
                    getBuyers();
                });
            } else {
                httpService.savedata(urlBuyer, $scope.New, function (response) {
                    $scope.dataLoading = false;
                    $scope.message = response.message;
                    $scope.messageclass = "alert alert-success";
                    $scope.$emit('BUYER_EVENT', true);
                    init();


                })
            }
        }

    });

})();
