(function () {
    var urlPO = 'api/PurchaseOrder';
    var urlDocument = 'api/Document';

    app.controller('purchaseOrderController', function ($http, $scope, $rootScope, $route, $modal, $filter, $timeout, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();
        $scope.buttonstatus = 'Create';
        $scope.New = {};
        $scope.poData = {};
        $scope.searchstr = '';
        init();
        function init() {
            $scope.recordCount = 'Loading..';
            $scope.poData = [];
        }

        $rootScope.$on('PO_EVENT', function (event, data) {
            getPOs('all');
        });

        getPOs('all');

        function getPOs(searchstr) {
            init();
            var blendNo = globalService.replaceSlash(searchstr);
            httpService.getdata(urlPO + '/GetAllPO/' + searchstr, function (results) {
                $scope.poData = results.data;
                $scope.recordCount = results.data.length;
            });
        };



        $scope.searchdata = function searchdata() {
            if (_timeout) {
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getPOs($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;

            }, loadingDelay);
        };


        $scope.delete = function (id) {
            var retVal = confirm("Are you sure want to remove?");
            if (retVal == true) {
                globalService.openLoadingWindow('Removing..');

                httpService.deletedata(urlPO + '/' + id, function () {
                    alert('Data deleted successfully');
                    getPOs('all');
                    $scope.$emit('LOAD_DATA_EVENT', true);

                })
            }
        };


        $scope.print = function (obj) {
            var response = {};
            $http.post('api/POPrint', obj, { responseType: 'arraybuffer' }).success(function (data) {
                var file = new Blob([data], { type: 'application/pdf' });
                var fileURL = URL.createObjectURL(file);
                window.open(fileURL);
                $scope.dataLoading = false;


            }).error(function (data) {
                alert('Error in processing report')
                $scope.dataLoading = false;
            });

        };

        $scope.openCreateWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Packing/PO/Create.html',
                controller: 'poCreateController',
                windowClass: 'app-modal-po-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };

        //Added ITEMS
        $scope.openViewAddedItems = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/packing/PO/Items.html' + '?ver=123',
                controller: 'itemsAddController',
                windowClass: 'app-modal-md-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            modalInstance.result.then(function (selectedObject) {
                //alert(p.BoxNo);
                //p.contractname = selectedObject.contractname;
            });
        };

        $scope.email = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Packing/PO/Email.html',
                controller: 'poSendMailController',
                windowClass: 'app-modal-sm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };
    });

    app.controller('poCreateController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.buttonstatus = 'Create';
        $scope.dataLoading = false;

        var systemDate = globalService.getDate();
        var completeDate = globalService.convertDateyyyymmdd(systemDate);
        SetNew();

        function SetNew() {
            $scope.New = { 'Id': 0, 'PONumber': '', 'ContractId': 0, 'SupplierId': 0, 'IsAutoDocNo': true, 'DeliveryDate': completeDate, 'RowVersion': '', 'Instructions': '' };
            $scope.boxnodisabled = false;
        }
        if (!angular.isUndefined(item)) {
            $scope.New = item;
            $scope.buttonstatus = 'Update';
        }

        SetTransaction(item);
        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        globalService.openLoadingWindow('Loading..');
        AutoGenNo();
        function AutoGenNo() {
            //$scope.showLoader = true;
            globalService.openLoadingWindow('Loading..');

            httpService.getdata(urlDocument + '/GetDocumentNo/' + 3, function (results) {
                $scope.New.PONumber = results.data.FormattedDocNo;
                angular.element(document.getElementById('pono'))[0].disabled = true;
                $scope.showLoader = false;
                $scope.$emit('LOAD_DATA_EVENT', true);

            });
        }

        $scope.setAutoDocNo = function (status) {
            if (status === true) {
                AutoGenNo();
            }
            else {
                angular.element(document.getElementById('pono'))[0].disabled = false;
                $scope.New.PONumber = '';
                angular.element('#' + 'pono').focus()
            }
        };

        function SetTransaction(obj) {
            //New trans or update
            if (!angular.isUndefined(obj)) {
                if (obj.Id > 0) {
                    // getStandardList(obj.ContractId);
                    //SetExportQty(obj.StandardId);
                    $scope.boxnodisabled = true;
                    $scope.buttonstatus = "Update";
                    $scope.New = obj;
                    $scope.New.PONumber = obj.PONumber;
                }
            }
            else {
                $scope.New.IsAutoDocNo = true;
                AutoGenNo();
                $scope.buttonstatus = "Create";

            }
        };

        getOpenContractList();

        function getOpenContractList() {
            httpService.getdata('api/Contract/GetAllContracts/all', function (results) {
                $scope.ContractData = results.data;
                $scope.$emit('LOAD_DATA_EVENT', true);

            });

        };

        getSupplier('all');

        function getSupplier(searchstr) {
            var blendNo = globalService.replaceSlash(searchstr);
            httpService.getdata('api/Supplier/GetAllSupplier/' + searchstr, function (results) {
                $scope.SupplierData = results.data;
            });
        };

        $scope.save = function () {
            if (validate($scope.New)) {
                $scope.dataLoading = true;
                if ($scope.New.Id > 0) {
                    httpService.updatedata(urlPO, $scope.New, function (response) {
                        alert(response.message);
                        $scope.dataLoading = false;
                        $scope.$emit('PO_EVENT', true);
                        $scope.cancel();
                    })

                } else {
                    httpService.savedata(urlPO, $scope.New, function (response) {
                        $scope.message = '';

                        if (response.status == 200) {
                            $scope.messageclass = "alert alert-success";
                        }
                        $scope.message = response.message;
                        $scope.$emit('PO_EVENT', true);
                        $scope.dataLoading = false;
                        $scope.New = {};
                        $scope.cancel();

                    })

                }
            }
        };

        var validate = function (po) {
            if (po.PONumber == '' || po.PONumber == undefined) {
                alert('Invalid PO Number');
                return false
            }
           if (po.SupplierId == 0) {
                alert('Invalid Supplier');
                return false
            }
            return true;
        };



    });

    app.controller('poSendMailController', function ($http, $scope, $rootScope, $modal, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.buttonstatus = 'Create';
        $scope.dataLoading = false;
        $scope.disablestatus = false;

        var systemDate = globalService.getDate();
        var completeDate = globalService.convertDateyyyymmdd(systemDate);
        SetNew();

        function SetNew() {
            $scope.Email = {
                'ReferenceId': item.Id,
                'To': item.SupplierEmail,
                'From': 'continentaltea@blendandexport.com',
                'Subject': 'Purchase Order: ' + item.PONumber, 'Message': 'Plese refer the attached PO for delieries.', 'CC': '',
                'IsAttachment': true
            };
            $scope.boxnodisabled = false;
        }


        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        // globalService.openLoadingWindow('Loading..');

        $scope.setAttachment = function (status) {
            if (status === true) {
                $scope.Email.IsAttachment = true;
            }
        }
        $scope.send = function () {
            if (validate($scope.Email)) {
                $scope.disablestatus = true;
                globalService.openLoadingWindow('Sending..');
                httpService.savedata('api/POSendMail', $scope.Email, function (response) {
                    alert(response.message);
                    $scope.disablestatus = false;
                    $scope.$emit('LOAD_DATA_EVENT', true);
                    $scope.$emit('CLOSE_POPUP', true);
                })

            }
        };

        var validate = function (po) {
            if (po.To == '' || po.To == undefined) {
                alert('Invalid Receipient');
                return false
            }
            if (po.Subject == '' || po.Subject == undefined) {
                alert('Invalid Subject');
                return false
            }
            if (po.Message == '' || po.Message == undefined) {
                alert('Invalid Message');
                return false
            }
            return true;
        };

        $scope.viewHistory = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Packing/PO/EmailHistory.html',
                controller: 'poEmailHistoryController',
                windowClass: 'app-modal-sm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };

    });

    app.controller('poEmailHistoryController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);
        $scope.emaildata = [];

        var systemDate = globalService.getDate();
        var completeDate = globalService.convertDateyyyymmdd(systemDate);

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };
        getEmailHistory(item.ReferenceId);

        function getEmailHistory(poid) {
            httpService.getdata('api/POSendMail/GetPOEmailHistory/' + poid , function (results) {
                $scope.emaildata = results.data;
            });

        };
    });
})();