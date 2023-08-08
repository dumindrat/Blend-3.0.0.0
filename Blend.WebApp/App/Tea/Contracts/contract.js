var conurl = 'api/Contract';
var conDeturl = 'api/ContractDetail';


app.controller('contractController', function ($scope, $rootScope, $modal, $route, $timeout, httpService, globalService) {
    var renderAction = $route.current.$$route.originalPath;
    $scope.accessLevels = globalService.getaccess(renderAction);
    var _timeout;
    var loadingDelay = globalService.getDataLoadingDelay();
    $scope.contractData = {};
    $scope.New = {};
    $scope.searchstr == ''
    $scope.recordCount = globalService.getDataLoadingMessage();

    $rootScope.$on('CONTRACT_EVENT', function (event, data) {
        getContracts('all');
    });

    getContracts('all');

    function getContracts(searchstr) {
        $scope.recordCount = globalService.getDataLoadingMessage();
        httpService.getdata(conurl + '/GetAllContracts/' + searchstr, function (results) {
            $scope.contractData = results.data;
            $scope.recordCount = globalService.getDataCountMessage(results.data.length);
            $('#datasearch').focus();
        });
    };

    $scope.searchdata = function searchdata() {
        if (_timeout) {
            $timeout.cancel(_timeout);
        }
        _timeout = $timeout(function () {
            getContracts($scope.searchstr === '' ? "all" : $scope.searchstr);
            _timeout = null;

        }, loadingDelay);

    };


    //$scope.gridOptions = {
    //    data: 'contractData',
    //    showGroupPanel: false,
    //    rowHeight: globalService.GridRowHeight(),
    //    columnDefs: [{ field: 'ContractId', displayName: 'Id', visible: false, width: '5%' },
    //        { field: 'ContractName', displayName: 'Contract Name', width: '20%' },
    //        { field: 'BuyerId', displayName: 'BuyerId', visible: false, width: '5%' },
    //        { field: 'BuyerName', displayName: 'Buyer Name', width: '30%' },
    //        { field: 'IsClosed', displayName: 'Active', visible: false, width: '10%' },
    //        { field: 'CompletionDate', displayName: 'CompletionDate', visible: true, width: '15%' },
    //        { field: 'CloseStatus', displayName: 'Status', width: '10%' },
    //        { field: 'RowVersion', displayName: 'RowVersion', visible: false, width: '15%' },


    //        {
    //            displayName: 'Options', cellTemplate: '<div class="btn-group"><input class="btn btn-info btn-sm" type="button" ng-click="openCreateWindow(row.entity)" name="edit"  value="Edit">' +
    //              '&nbsp;<input type="button" class="btn btn-danger btn-sm" ng-click="delete(row.entity.ContractId)"  name="delete"  value="Delete">' +
    //              //'&nbsp;<button type="button" class="btn btn-info btn-sm"><span class="glyphicon glyphicon-trash"></span> Trash </button> ' +
    //              '&nbsp;<input type="button" class="btn btn-primary btn-sm" ng-click="openViewAddedStandard(row.entity)" name="addstd"  value="Add Standard"></div>'
    //            , width: '25%'
    //        }
    //    ]
    //};

    $scope.delete = function (id) {
        var retVal = confirm("Are you sure want to remove?");
        if (retVal == true) {
            globalService.openLoadingWindow('Deleting..');
            httpService.deletedata(conurl + '/' + id, function () {
                alert('Data deleted successfully');
                getContracts('all');
                $scope.$emit('LOAD_DATA_EVENT', true);

            })
        }


    };

    //Added standard
    $scope.openViewAddedStandard = function (p, size) {
        var modalInstance = $modal.open({
            templateUrl: '/App/Tea/Contracts/StandardDetail.html',
            controller: 'contractEdtController',
            windowClass: 'app-modal-md-window',
            resolve: {
                item: function () {
                    return p;
                }
            }
        });
        modalInstance.result.then(function (selectedObject) {
            p.contractname = selectedObject.contractname;
        });
    };

    $scope.openCreateWindow = function (p, size) {
        var modalInstance = $modal.open({
            templateUrl: '/App/Tea/Contracts/CreateContract.html',
            controller: 'contractCreateController',
            windowClass: 'app-modal-xsm-window',
            resolve: {
                item: function () {
                    return p;
                }
            }
        });

    };


});

app.controller('contractCreateController', function ($scope, $rootScope, $route, $modalInstance, $filter, item, httpService, globalService) {
    var renderAction = $route.current.$$route.originalPath;
    $scope.accessLevels = globalService.getaccess(renderAction);
    var systemDate = globalService.getDate();
    var initSaleDate = globalService.convertDateyyyymmdd(systemDate);

    $scope.New = { 'CompletionDate': initSaleDate };

    $scope.buttonstatus = 'Create';
    $scope.dataLoading = false;

    if (!angular.isUndefined(item)) {
        $scope.New = item;
        $scope.buttonstatus = 'Update';
    }


    getBuyers("all");

    function getBuyers(searchStr) {
        globalService.openLoadingWindow('Loading..');
        httpService.getdata('api/Buyer' + '/GetAllBuyers/' + searchStr, function (results) {
            $scope.buyers = results.data;
            $scope.$emit('LOAD_DATA_EVENT', true);

        });
    };
    //Authentication fails. emit from data.service
    $rootScope.$on('CLOSE_POPUP', function (event, data) {
        $scope.cancel();
    });
    $scope.cancel = function () {
        $modalInstance.dismiss('Close');
    };



    $scope.save = function () {
        $scope.dataLoading = true;
        $scope.New.CompletionDate = globalService.convertDateToIST($scope.New.CompletionDate);
        if ($scope.New.ContractId > 0) {
            httpService.updatedata(conurl, $scope.New, function (response) {
                alert('Successfully Updated');
                $scope.dataLoading = false;
                $scope.cancel();
            })

        } else {
            httpService.savedata(conurl, $scope.New, function (response) {
                alert('Successfully Saved');
                $scope.$emit('CONTRACT_EVENT', true);
                $scope.dataLoading = false;

            })

        }
    };


});

app.controller('contractEdtController', function ($scope, $modalInstance, $filter, $timeout, item, httpService, globalService) {
    $scope.setvisibleaddedstandard = true;
    $scope.setvisiblestandard = false;
    $scope.selectedContract = item;
    $scope.selectedStandard = {};
    $scope.selectedAddedStandard = {};
    var _timeout;
    var loadingDelay = globalService.getDataLoadingDelay();
    $scope.searchstr == ''
    $scope.dataloading = false;

    $scope.payload = { ContractId: 0, StandardId: 0, OrderQty: 0, RowNo: [] };

    $scope.addedteastandardscolumns = [
                   { text: "Id", predicate: "contractid", hide: true, sortable: true, dataType: "number" },
                   { text: "Id", predicate: "standardid", hide: true, sortable: true, dataType: "number" },
                   { text: "StandardName", predicate: "standardname", hide: false, sortable: true },
                   { text: "Order Qty", predicate: "orderqty", hide: false, sortable: true },
                   { text: "Version", predicate: "rowversion", hide: true, sortable: true },
                   { text: "Action", predicate: "Action", hide: false, sortable: false }
    ];

    $scope.teastandardscolumns = [
                   { text: "Id", predicate: "StandardId", hide: false, sortable: true, dataType: "number" },
                   { text: "Standard Name", predicate: "standardname", hide: false, sortable: true },
                   { text: "Order Qty", predicate: "orderqty", hide: false, sortable: true },
                   //{ text: "Version", predicate: "rowversion", sortable: true },
                   { text: "Action", predicate: "", hide: false, sortable: false }
    ];

    $scope.searchdata = function searchdata() {
        if (_timeout) {
            $timeout.cancel(_timeout);
        }
        _timeout = $timeout(function () {
            getAvailabaleStandards($scope.searchstr === '' ? "all" : $scope.searchstr);
            _timeout = null;

        }, loadingDelay);

    };

    getAddedStandards();

    function getAddedStandards() {

        httpService.getdata(conDeturl + '/' + 'GetBuyerContractDetails' + '/' + $scope.selectedContract.ContractId, function (results) {
            $scope.addedStandardData = results.data;
        });
    };

    getAvailabaleStandards('all');

    function getAvailabaleStandards(searchstr) {
        $scope.dataloading = true;

        httpService.getdata(conDeturl + '/' + 'GetAvailableStandards?id=' + $scope.selectedContract.ContractId + '&standard=' + searchstr, function (results) {
            $scope.teastandards = results.data;
            $scope.dataloading = false;

        });

    };

    $scope.setVisibleAddedStandard = function () {
        $scope.setvisibleaddedstandard = true;
        $scope.setvisiblestandard = false;

    };

    $scope.setHideAddedStandard = function () {
        $scope.setvisibleaddedstandard = false;
        $scope.setvisiblestandard = true;

    };

    /*for available standard*/
    $scope.getTemplate = function (standard) {
        if (standard.StandardId === $scope.selectedStandard.StandardId) {
            return 'edit';
        }
        else return 'display';
    };

    $scope.editStandard = function (standard) {
        $scope.selectedStandard = angular.copy(standard);
    };

    $scope.reset = function () {
        $scope.selectedStandard = {};
    };
    /*---------------------------------------*/

    /*for aded standard*/
    $scope.getUpdateTemplate = function (standard) {
        if (standard.StandardId === $scope.selectedAddedStandard.StandardId) {
            return 'displayUpdateTemplate';
        }
        else return 'displayAddedTemplate';
    };

    $scope.editAddedStandard = function (standard) {
        $scope.selectedAddedStandard = angular.copy(standard);
    };

    $scope.resetUpdate = function () {
        $scope.selectedAddedStandard = {};
    };
    /*---------------------------------------*/

    $scope.addToContract = function (standard) {
        $scope.payload.ContractId = $scope.selectedContract.ContractId;
        $scope.payload.StandardId = standard.StandardId;
        $scope.payload.OrderQty = standard.OrderQty;
        globalService.openLoadingWindow('Adding..');

        httpService.savedata(conDeturl, $scope.payload, function (response) {
            if (response.status === 200) {
                $scope.messageclass = "alert alert-success";
            }

            $scope.message = response.message;
            $scope.teastandards = [];
            getAvailabaleStandards();
            getAddedStandards();
            $scope.$emit('LOAD_DATA_EVENT', true);

        })
    };

    $scope.updateContractDet = function (standard) {
        $scope.payload.ContractId = $scope.selectedContract.ContractId;
        $scope.payload.StandardId = standard.StandardId;
        $scope.payload.OrderQty = standard.OrderQty;
        $scope.payload.RowVersion = standard.RowVersion;
        globalService.openLoadingWindow('Updating..');

        httpService.updatedata(conDeturl, $scope.payload, function (response) {
            if (response.status === 200) {
                $scope.messageclass = "alert alert-success";
            }

            $scope.message = response.message;
            getAvailabaleStandards();
            getAddedStandards();
            $scope.$emit('LOAD_DATA_EVENT', true);

        })
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('Close');
    };

    $scope.removeFromContract = function (standard) {
        $scope.payload.ContractId = $scope.selectedContract.ContractId;
        $scope.payload.StandardId = standard.StandardId;
        $scope.payload.RowVersion = standard.RowVersion;
        contractRepository.deleteStandard(function () {
            var status = 'Standard removed successfully';
            $scope.status = status;
            alert(status);
            getAvailabaleStandards();
            getAddedStandards();
        }, $scope.payload);

        var retVal = confirm("Are you sure want to remove?");
        if (retVal == true) {
            httpService.deletedata(conDeturl + '/' + id, function () {
                alert('Data deleted successfully');
                getBlendList("all");
            })
        }
    };
});