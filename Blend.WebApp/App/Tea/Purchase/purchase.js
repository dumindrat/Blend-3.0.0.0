(function () {
    var urlStraightLinePurchase = 'api/StraightLinePurchase';
    var urlDocument = 'api/Document';
    var urlSale = 'api/Sale';
    app.controller('straightLineController', function ($http, $scope, $rootScope, $modal, $route, $filter, $timeout, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        var systemDate = $filter('date')(globalService.getDate(), 'MM-DD-YYYY');
        $scope.accessLevels = globalService.getaccess(renderAction);
        var _timeout;
        var loadingDelay = globalService.getDataLoadingDelay();

        $scope.searchstr = "";
        $scope.recordCount = globalService.getDataLoadingMessage();

        $scope.countries = [{ 'country': 'Srilanka' }, { 'country': 'Japan' }]

        $rootScope.$on('SALE_CREATE_EVENT', function (event, data) {
            getSales('all');
        });

        getSales('all');

        function getSales(seachby) {
            $scope.recordCount = globalService.getDataLoadingMessage();
            var saledate = globalService.replaceSlash(seachby);
            httpService.getdata(urlStraightLinePurchase + '/GetAllSales/' + saledate, function (results) {
                $scope.SaleData = results.data;
                $scope.recordCount = globalService.getDataCountMessage(results.data.length);
                $('#search').focus();
            });

        };


        $scope.searchdata = function searchdata() {
            if (_timeout) {
                $timeout.cancel(_timeout);
            }
            _timeout = $timeout(function () {
                getSales($scope.searchstr === '' ? "all" : $scope.searchstr);
                _timeout = null;

            }, loadingDelay);

        };

        $scope.searchsale = function searchsale() {
            getSales($scope.searchstr === '' ? "all" : $scope.searchstr);
        };

        $scope.setScope = function (obj, action) {
            $scope.action = action;
            $scope.New = obj;
        };

        //$scope.gridOptions = {
        //    data: 'SaleData',
        //    showGroupPanel: false,
        //    rowHeight: globalService.GridRowHeight(),
        //    columnDefs: [{ field: 'SaleId', displayName: 'Id', visible: false, width: '5%' },
        //        { field: 'SaleDate', displayName: 'SaleDate', width: '60%' },
        //        { field: 'IsClosed', displayName: 'Status', visible: false, width: '15%' },
        //        { field: 'SaleStatus', displayName: 'SaleStatus', visible: true, width: '15%' },

        //        {
        //            displayName: 'Options', cellTemplate: '<div class="btn-group"><input type="button" class="btn btn-info btn-sm" ng-click="openCreateSaleWindow(row.entity)" ng-disabled="accessLevels.C || CheckCloseStatus(row.entity)" name="edit"  value="Edit">' +
        //                '&nbsp;<input class="btn btn-info btn-sm" type="button" ng-disabled="accessLevels.M" ng-click="openViewAddedLines(row.entity)" name="add"  value="Straight Lines">' +
        //                '&nbsp;<input type="button" ng-class="SetClass(row.entity)" ng-click="ChangeStatus(row.entity)"  name="clsoe"  ng-value="CheckStatus(row.entity)">'

        //            , width: '25%'
        //        }
        //    ]
        //};

        $scope.CheckCloseStatus = function (obj) {
            if (obj.IsClosed === true) {
                return true;
            }
            return false;
        }
        $scope.SetClass = function (sale) {
            return sale.IsClosed === false ? 'btn btn-danger btn-sm' : 'btn btn-warning btn-sm';
        }
        $scope.CheckStatus = function (sale) {
            return sale.IsClosed === true ? 'Sale Open' : 'Sale Close';
        }

        $scope.ChangeStatus = function (sale) {
            var status = sale.IsClosed === true ? 'Open' : 'Close';
            var retVal = confirm("Are you sure want to " + status + ' ?');
            if (retVal == true) {
                var sale = { 'SaleId': sale.SaleId, 'IsClosed': !sale.IsClosed };
                httpService.updatedata(urlSale, sale, function (response) {
                    alert(response.message);
                    $scope.$emit('SALE_CREATE_EVENT', true);
                    $scope.dataLoading = false;
                })
            };
        }



        //Added standard
        $scope.openViewAddedLines = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/Purchase/StraightLines.html' +'?ver=123',
                controller: 'straightLineAddController',
                windowClass: 'app-modal-window',
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

        //Added lines
        $scope.openCreateSaleWindow = function (p, size) {
            var modalInstanceC = $modal.open({
                templateUrl: '/App/Tea/Purchase/CreateSale.html',
                controller: 'createSaleController',
                windowClass: 'app-modal-xxsm-window',
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

    app.controller('createSaleController', function ($http, $scope, $rootScope, $modalInstance, $route, $filter, item, httpService, globalService) {
        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);

        var systemDate = globalService.getDate();
        var initSaleDate = globalService.convertDateyyyymmdd(systemDate);

        $scope.New = { 'SaleDate': initSaleDate };
        $scope.buttonstatus = 'Create';
        $scope.dataLoading = false;

        if (!angular.isUndefined(item)) {
            $scope.New = item;
            $scope.buttonstatus = 'Update';
        }

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        var validation = function () {
            if ($scope.New === null) {
                alert('Select SaleDate');
                return false;
            }
            return true;
        };

        $scope.create = function () {
            if (validation()) {
                $scope.dataLoading = true;
                $scope.New.SaleDate = globalService.convertDateToIST($scope.New.SaleDate);
                if (angular.isUndefined($scope.New.SaleId)) {
                    httpService.savedata(urlSale, $scope.New, function (response) {
                        //var status = response.status;
                        alert(response.message);
                        $scope.$emit('SALE_CREATE_EVENT', true);
                        $scope.dataLoading = false;
                        $scope.cancel();
                    })
                }
                else {
                    httpService.updatedata(urlSale, $scope.New, function (response) {
                        //var status = response.status;
                        alert(response.message);
                        $scope.$emit('SALE_CREATE_EVENT', true);
                        $scope.dataLoading = false;
                        $scope.cancel();
                    })
                }
            };
        };
    });

    app.controller('straightLineAddController', function ($scope, $rootScope, $modalInstance, $modal, $route, $filter, item, httpService, globalService) {

        ///* Initialization*/
        $scope.selectedSale = item;
        $scope.selectedSale.SaleDate = $filter('date')($scope.selectedSale.SaleDate, 'yyyy-MM-dd');

        $scope.myDecimal = 0;
        $scope.ver = '1w3re4';

        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);

        $scope.uploadData = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/Purchase/UploadData/UploadData.html' + '?ver=' + $scope.ver,
                controller: 'uploadDataController',
                windowClass: 'app-modal-window',
                resolve: {
                    item: function () {
                        return $scope.selectedSale.SaleId;
                    }
                }
            });
            modalInstance.result.then(function (selectedObject) {
            });
        };



        $rootScope.$on('SALE_ITEM_CHANGE', function (event, data) {
            getAddedStraightLine();
        });

        getAddedStraightLine();

        function getAddedStraightLine() {
            globalService.openLoadingWindow('Loading..');
            httpService.getdata(urlStraightLinePurchase + '/GetAddedStraghtLine/' + $scope.selectedSale.SaleId, function (results) {
                $scope.addedStarightLine = results.data;
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

        $scope.addedtealinecolumns = [
                 { text: "Id", predicate: "id", sortable: true, dataType: "number", hide: true },
                  { text: "BoxNo", predicate: "boxno", sortable: true, dataType: "number", hide: false },
                  //{ text: "SaleDate", predicate: "SaleDate", sortable: true, dataType: "number" },
                  //{ text: "Code", predicate: "scode", sortable: true, hide: true },
                  { text: "Type", predicate: "stype", sortable: false, hide: false },
                  { text: "Invoice", predicate: "invoice", sortable: true, hide: false },
                  { text: "LotNo", predicate: "rowversion", sortable: true, hide: false },
                  { text: "BrokerId", predicate: "brokerid", sortable: true, hide: true },
                  { text: "Bk", predicate: "brokercode", sortable: true, hide: false },
                  { text: "Grade", predicate: "grade", sortable: true, hide: false },
                  { text: "GardenMark", predicate: "gardenMark", sortable: true, hide: false },
                  //{ text: "TeaCertificates", predicate: "teacertificates", sortable: true, hide: true },
                  { text: "Net", predicate: "netw", sortable: true, hide: false },
                  { text: "Packs", predicate: "packs", sortable: true, hide: false },
                  { text: "RQty", predicate: "rqty", sortable: true, hide: false },
                  { text: "Weight", predicate: "weight", sortable: true, hide: false },
                  { text: "Rate", predicate: "rate", sortable: true, hide: false },
                  { text: "Value", predicate: "value", sortable: true, hide: false },
                  { text: "Tin", predicate: "tinno", sortable: true, hide: true },
                  { text: "", predicate: "option", sortable: true, hide: false }
                  //{ text: "Delete", predicate: "option", sortable: true, hide: false }

        ];


        $scope.AddLines = function (p) {
            var obj = { 'Sale': $scope.selectedSale, 'Line': p };
            var modalInstance = $modal.open({
                templateUrl: 'App/Tea/Purchase/AddLines.html',
                controller: 'straightLineAddToSaleController',
                windowClass: 'app-modal-sm-window',
                backdrop: 'static',
                keyboard: false,
                resolve: {
                    item: function () {
                        return obj;
                    }
                }
            });
        };

        $scope.HideInfoView = function () {
            $scope.setvisibleaddscreen = false;
            $scope.setvisibleaddedscreen = true;
            $scope.certificateassigned = '';
        };





        $scope.DeleteFromSale = function (straightline) {
            var retVal = confirm("Are you sure want to delete?");
            if (retVal == true) {
                $scope.button = {};
                $scope.dataLoading = true;
                straightline.SaleId = $scope.selectedSale.SaleId;
                globalService.openLoadingWindow('Removing..');

                httpService.updatedata('api/StraightLineDelete', straightline, function (response) {
                    if (response.status === 200) {
                        alert('Data deleted successfully');
                    }
                    getAddedStraightLine();
                    $scope.$emit('LOAD_DATA_EVENT', true);

                });
            }

        };

        $scope.Repeated = function (status) {
            return status === true ? true : false;
        };



        //Added Blends

    });

    app.controller('straightLineAddToSaleController', function ($scope, $rootScope, $modalInstance, $modal, $route, $filter, item, httpService, globalService) {
        $scope.GardenMarkList = [];
        $scope.GradeList = [];
        $scope.addItem = function (item) {
            var ret = $scope.GardenMarkList.indexOf(item);
            if (ret == -1) {
                $scope.GardenMarkList.push(item);
            }
            $scope.item = {};
        }

        $scope.addGrades = function (item) {
            var ret = $scope.GradeList.indexOf(item);
            if (ret == -1) {
                $scope.GradeList.push(item);
            }
            $scope.item = {};
        }

        /* Initialization*/
        $scope.selectedSale = item.Sale;
        $scope.selectedBox = item.BoxNo;
        $scope.teaCertificates = [];
        $scope.selectedSale.SaleDate = $filter('date')($scope.selectedSale.SaleDate, 'yyyy-MM-dd');
        $scope.messageclass = {};
        $scope.message = "";
        $scope.msg = false;
        $scope.addedStarightLine = [];
        $scope.saletypes = [{ saletype: "MS" }, { saletype: "PS" }, { saletype: "OL" }];

        var renderAction = $route.current.$$route.originalPath;
        $scope.accessLevels = globalService.getaccess(renderAction);

        if (!angular.isUndefined(item.Line)) {
            $scope.buttonstatus = 'Update';
            GetStraightLine(item);

        }
        else {
            $scope.buttonStatus = "Create";
            $scope.New = { Id: 0, StockId: 0, SaleCode: 0, Net1: 0, Pack1: 0, Net2: 0, Pack2: 0, Weight: 0, Rate: 0, Value: 0, ReducedQty: 0, TeaCertificates: '' };
        }

        $scope.convertDecimal = function (value) {
            return globalService.convertDecimal(value);
        }

        $rootScope.$on('CHANGE_CERTIFICATEDATA_EVENT', function (event, data) {
            certified(data);
        });


        getTeaCertificates();

        function getTeaCertificates() {
            httpService.getdata(urlStraightLinePurchase, function (results) {
                $scope.teaCertificates = results.data;
            });
        }

        getGardenMarks();

        function getGardenMarks() {
            httpService.getdata('api/GardenMark', function (results) {
                angular.forEach(results.data, function (value, key) {
                    $scope.addItem(value['GardenMarkName']);
                })
            });
        }

        getGrades();

        function getGrades() {
            httpService.getdata('api/Grade', function (results) {
                angular.forEach(results.data, function (value, key) {
                    $scope.addGrades(value['GradeName']);
                })
            });
        }

        function certified(item) {
            //alert(item);
            $scope.teaCertificates.forEach(function (value, index, array) {
                if (value.CertifiedName === item) {
                    $scope.certificateassigned = convertCertListToString();
                    $scope.New.TeaCertificates = $scope.certificateassigned;
                }

            });
        };

        function convertCertListToString() {
            var items = '';
            $scope.teaCertificates.forEach(function (value, index, array) {
                if (value.IsCertified === true) {
                    var seperator = items === '' ? '' : ','
                    items = items + seperator + value.CertifiedName;
                }

            });
            return items;
        }

        getReservedNo();

        function getReservedNo() {
            if (angular.isUndefined(item.Line)) {

                httpService.getdata(urlDocument + '/GetDocumentNo/' + 1, function (results) {
                    $scope.reservedNo = results.data.FormattedDocNo;
                });
            }
            else {
                $scope.reservedNo = item.BoxNo;
            }
        };

        getBrokers("all");

        function getBrokers(searchstr) {
            httpService.getdata('api/Broker/GetBrokers/' + searchstr, function (results) {
                $scope.BrokerData = results.data;
            });

        };

        $scope.SetNew = function () {
            $scope.New = { Id: 0, StockId: 0, Net1: 0, Pack1: 0, Net2: 0, Pack2: 0, Weight: 0, Rate: 0, Value: 0, ReducedQty: 0, Grade: '', Standard: '', GardenMark: '', Standard: '', BrokerId: 0, SaleType: 1, SaleCode: '' };

        }

        $scope.calWeight = function () {
            $scope.New.Weight = ($scope.New.Net1 * $scope.New.Pack1) + ($scope.New.Net2 * $scope.New.Pack2 - $scope.New.ReducedQty);
            $scope.New.Value = $scope.New.Weight * $scope.New.Rate;
        };

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        function GetStraightLine(obj) {
            globalService.openLoadingWindow('Loading..');
            httpService.getdata(urlStraightLinePurchase + '/GetStraightLineById/' + obj.Line.Id, function (results) {
                //$scope.HideAddedLines();
                $scope.New = results.data;
                $scope.reservedNo = results.data.BoxNo;
                $scope.certificateassigned = results.data.TeaCertificates;
                $scope.buttonStatus = "Update";
                $scope.$emit('LOAD_DATA_EVENT', true);

            });
        }

        $scope.AddToSale = function (straightline) {
            $scope.button = {};
            $scope.dataLoading = true;
            straightline.SaleId = $scope.selectedSale.SaleId;
            straightline.TeaCertificates = convertCertListToString();
            if (straightline.Id === 0) {
                httpService.savedata(urlStraightLinePurchase, straightline, function (response) {
                    if (response.status === 200) {
                        $scope.mesg = true;
                        $scope.message = "Success";
                        $scope.messageclass = "alert alert-success";
                        $scope.$emit('SALE_ITEM_CHANGE', true);
                        getReservedNo();
                        $scope.SetNew()
                        $scope.New.BrokerId = straightline.BrokerId;
                        $scope.New.SaleType = straightline.SaleType;
                        $scope.New.SaleCode = straightline.SaleCode;
                        $scope.dataLoading = false;
                        //$scope.teaCertificates = [];
                        $scope.certificateassigned = '';
                        $scope.addItem(straightline.GardenMark);
                        angular.element('#' + 'lotno').focus()
                    } else {
                        $scope.error = response.message;
                        $scope.dataLoading = false;

                    }
                });
            }
            else {
                httpService.updatedata(urlStraightLinePurchase, straightline, function (response) {
                    if (response.status === 200) {
                        $scope.mesg = true;
                        alert("Success");
                        $scope.$emit('SALE_ITEM_CHANGE', true);
                        getReservedNo();
                        $scope.SetNew();
                        $scope.dataLoading = false;
                        $scope.cancel();
                    } else {
                        $scope.error = response.message;
                        $scope.dataLoading = false;

                    }
                });
            }
        };

        $scope.openAddCertificateWindow = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: 'App/Tea/Purchase/Certificates.html',
                controller: 'addCertificateController',
                windowClass: 'app-modal-xsm-window',
                backdrop: 'static',
                keyboard: false,
                resolve: {
                    certificates: function () {
                        return $scope.teaCertificates;
                    },
                    addedcertificates: function () {
                        return $scope.New.TeaCertificates;
                    }
                }
            });
        };

    });

    app.controller('addCertificateController', function ($scope, $rootScope, $modalInstance, $modal, $filter, certificates, addedcertificates, httpService) {
        //$scope.certifiedNames = certificates;

        $scope.addedCertificateList = addedcertificates === undefined ? null : addedcertificates.split(",");



        setCertificates();

        function setCertificates() {
            $scope.certifiedNames = certificates;
            //$scope.clsname = 'btn btn-primary'
        }

        teaCertifiedForExsistingLine();

        function teaCertifiedForExsistingLine() {
            $scope.certifiedNames.forEach(function (value, index, array) {
                if (value != null) {
                    value.IsCertified = false;
                }

            });
            if ($scope.addedCertificateList != null) {
                $scope.addedCertificateList.forEach(function (value, index, array) {
                    if (value != null) {
                        certifiedAddedItems(value);
                    }

                });
            }
        }

        $scope.doCertified = function (item) {
            certified(item);
        }

        function certified(item) {
            if (certificates != null) {
                certificates.forEach(function (value, index, array) {
                    if (value.CertifiedName === item) {
                        if (value.IsCertified === true) {
                            value.IsCertified = false;
                            alert('Removed certification :' + value.CertifiedName);
                        }
                        else {
                            value.IsCertified = true;
                            alert('Added certification :' + value.CertifiedName);
                        }
                        setCertificates();
                        $scope.$emit('CHANGE_CERTIFICATEDATA_EVENT', item);

                    }

                });
            }
        };

        function certifiedAddedItems(item) {
            if (certificates != null) {
                certificates.forEach(function (value, index, array) {
                    if (value.CertifiedName === item) {
                        value.IsCertified = true;
                        setCertificates();
                    }
                });
            }
        };

        $scope.hidecetificate = function (item) {
            $scope.certifiedNames.forEach(function (value, index, array) {
                if (value.CertifiedName === item) {
                    return true;
                }

            });
        };

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };
    });


})();