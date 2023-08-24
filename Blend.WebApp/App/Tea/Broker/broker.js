(function () {

    var urlBroker = 'api/Broker';

    //Controller
    app.controller('brokerController', function ($http, $scope, $rootScope, $route, $modal, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $rootScope.$on('BROKER_EVENT', function (event, data) {
            getBrokers("all");
        });

        $scope.dataLoading = false;
        $scope.recordCount = globalService.getDataLoadingMessage();

        getBrokers("all");

        function getBrokers(searchstr) {
            $scope.dataLoading = true;
            $scope.recordCount = globalService.getDataLoadingMessage();
            httpService.getdata(urlBroker + '/GetBrokers/' + searchstr, function (results) {
                $scope.BrokerData = results.data;
                $scope.dataLoading = false;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);

            });

        };


        $scope.setScope = function (obj, action) {
            $scope.action = action;
            $scope.New = obj;
        };

        $scope.filterOptions = {
            filterText: "",
            useExternalFilter: true
        };



        $scope.delete = function (id) {
            var retVal = confirm("Are you sure want to remove?");
            if (retVal == true) {
                globalService.openLoadingWindow('Removing..');
                httpService.deletedata(urlBroker + '/' + id, function () {
                    alert('Data deleted successfully');
                    getBrokers("all");
                    $scope.$emit('LOAD_DATA_EVENT', true);

                })
            }
        };

        $scope.openCreateWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: '/App/Tea/Broker/CreateBroker.html',
                controller: 'brokerCreateController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            modalInstanceC.result.then(function (selectedObject) {
            });
        };

    });

    app.controller('brokerCreateController', function ($scope, $rootScope, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.buttonstatus = 'Create';
        $scope.buttondisable = false;
        $scope.dataLoading = false;

        if (!angular.isUndefined(item)) {
            $scope.New = item;
            $scope.buttonstatus = 'Update';
            $scope.buttondisable = true;;
        }
        else {
            init();
        }

        function init() {
            $scope.New = { 'BrokerCode': '', 'BrokerName': '', 'Address': '', 'Email': '', 'Tel': '', 'ContactPerson': '' };
        }
        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.save = function () {
            $scope.dataLoading = true;
            setdefault();
            if ($scope.New.BrokerId > 0) {
                httpService.updatedata(urlBroker + '/' + $scope.New.BrokerId, $scope.New, function (response) {
                    alert(response.message);
                    $scope.message = response.message;
                    $scope.messageclass = "alert alert-success";
                    $scope.$emit('BROKER_EVENT', true);
                    $scope.cancel();
                    $scope.dataLoading = false;

                });
            } else {
                httpService.savedata(urlBroker, $scope.New, function (response) {
                    $scope.message = response.message;
                    $scope.messageclass = "alert alert-success";
                    $scope.$emit('BROKER_EVENT', true);
                    $scope.dataLoading = false;
                    init();

                })
            }


        };

        function setdefault() {
            
            if ($scope.New.Address === '') {
                $scope.New.Address = 'NA';
            }
            if ($scope.New.Email === '') {
                $scope.New.Email = 'NA';
            }
            if ($scope.New.Tel === '') {
                $scope.New.Tel = 'NA';
            }
            if ($scope.New.ContactPerson === '') {
                $scope.New.ContactPerson = 'NA';
            }
        }



    });
})();
