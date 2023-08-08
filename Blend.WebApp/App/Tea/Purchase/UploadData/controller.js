(function () {


    app.controller('uploadDataController', ['$scope', '$rootScope', '$http', '$modal', '$modalInstance', 'httpService', 'globalService', 'item', function ($scope, $rootScope, $http, $modal, $modalInstance, httpService, globalService, item) {
        $scope.SelectedSaleId = item;
        $scope.selectedFile = null;
        $scope.msg = "";
        $scope.lotNo = '';
        $scope.count = 0;
        $scope.BrokerId = 0;
        $scope.disablestatus = true;
        $scope.saletypes = [{ saletype: "MS" }, { saletype: "PS" }, { saletype: "OL" }];
        $scope.SaleType;
        $scope.SaleCode;
        $scope.validating = false;
        $scope.validatecomplete = false;
        // $scope.setclass = 'right-icon';
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $rootScope.$on('SUBMIT_OK', function (event, data) {
            $scope.dataObjects = [];
        });

        getBrokers();

        function getBrokers() {
            httpService.getdata('api/Broker', function (results) {
                $scope.BrokerData = results.data;
            });

        };

        $scope.split = function (val, item) {
            var array = val.split(' ');
            return array[item];
        }

        $scope.setclass = function (val) {
            if (val.IsValid) { return 'right-icon'; }
            return 'error-icon';
        };

        $scope.calreduced = function (net, bags, qty) {
            var calqty = net * bags;
            var retQty = calqty - qty;
            return retQty;

        }

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };




        $scope.count = 0;
        $scope.lineCount = function () {
            return $scope.count = $scope.count + 1;
        }

        //--new change
        var formdata = new FormData();

        $scope.getTheFiles = function ($files) {
            angular.forEach($files, function (value, key) {
                formdata.append(key, value);
            });
        };

        $scope.uploadFiles = function () {
            $scope.dataObjects = [];
            $scope.validating = true;
            $scope.validatecomplete = false;
            globalService.openLoadingWindow('Processing file ..');

            var request = {
                method: 'POST',
                url: '/api/UploadFile/',
                data: formdata,
                headers: {
                    'Content-Type': undefined
                },
                transformRequest: angular.identity
            };
            $scope.empArray = new Array;
            // SEND THE FILES TO THE WEB API.
            $http(request)
                .success(function (data) {
                    $scope.dataObjects = data.ItemList;
                    $scope.$emit('LOAD_DATA_EVENT', true);
                    $scope.disablestatus = data.IsValid ? false : true;
                    formdata = new FormData();
                    $scope.validating = false;
                    $scope.validatecomplete = true;
                })
                .error(function () {
                    alert('Error occured');
                    $scope.$emit('LOAD_DATA_EVENT', true);
                    $scope.validating = false;
                    $scope.validatecomplete = true;
                });
        }






        $scope.getTempReservedNo = function (lotno) {
            if ($scope.lotNo != lotno) {
                $scope.count = $scope.count + 1;
            }
            $scope.lotNo = lotno;
            return $scope.count;

        }
        $scope.inValidLine = 0;
        $scope.validatedata = function () {
            var retStatus = { 'Status': true, 'Error': '' };
            if ($scope.BrokerId == 0) {
                retStatus.Status = false;
                retStatus.Error = '[ Please select Broker ]';
                return retStatus;
            }
            if ($scope.SaleType == 0 || $scope.SaleType == undefined) {
                retStatus.Status = false;
                retStatus.Error = '[ Please select SaleType]';
                return retStatus;
            }
            if ($scope.SaleCode == undefined || $scope.SaleCode == '') {
                retStatus.Status = false;
                retStatus.Error = '[ Please add SaleCode ]';
                return retStatus;
            }
            return retStatus;
        }
        $scope.submit = function () {
            var retSttaus = $scope.validatedata()
            if (retSttaus.Status) {
                var retVal = confirm("Are you sure want to Upload data? " + "Selected Broker: " + $("#broker option:selected").text() + "-" + " SaleType: " + $("#saletype option:selected").text() + "-" + " SaleCode: " + $("#salecode").val());
                if (retVal == true) {
                    globalService.openLoadingWindow('Uploading..');
                    $scope.disablestatus = true;

                    var objlist = [];
                    var values = $scope.dataObjects;
                    var log = [];
                    angular.forEach(values, function (value, key) {
                        var obj = {
                            'SaleId': $scope.SelectedSaleId, 'SaleCode': $scope.SaleCode, 'SaleType': $scope.SaleType, 'BrokerId': $scope.BrokerId,
                            'LotNo': value.Lot, 'Invoice': value.Inv, 'GardenMark': value.Mark, 'Grade': value.Grade, 'Standard': '',
                            'Pack1': value.Bags, 'Net1': value.Net,
                            'ReducedQty': value.RedQty, 'Weight': value.Qty, 'Rate': value.Price, 'Value': value.Value,
                            'VirtualValue': value.Value
                        };

                        objlist.push(obj);
                    }, log);
                    var name = document.getElementById('file1');
                    var saleUpload = {
                        'FileName': name.files.item(0).name,
                        'SaleId': $scope.SelectedSaleId,
                        'SaleCode': $scope.SaleCode,
                        'SaleType': $scope.SaleType,
                        'BrokerId': $scope.BrokerId,
                        'StraightLines': objlist
                    };
                    
                    $http.post('api/StraightLineUpload', saleUpload)
                     .success(function (data, status, headers, config) {
                         $scope.$emit('LOAD_DATA_EVENT', true);
                         $scope.$emit('SALE_ITEM_CHANGE', true);
                         $scope.disablestatus = false;

                         ShowUploadStatus(data);
                     })
                     .error(function (data, status, headers, config) {
                         if (status === 401) {
                             globalService.RequestAuthentication();
                             $location.path('/Login');
                         }
                     });
                }
            }
            else {
                alert('Unable to submit data : ' + '-' + retSttaus.Error);
            }

        }

        function ShowUploadStatus(p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/Purchase/UploadData/UploadStatus.html' + '?ver=' + $scope.ver,
                controller: 'straightUploadStatusController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });
            modalInstance.result.then(function (selectedObject) {
            });
        };

    }]);

    app.controller('straightUploadStatusController', function ($scope, $modalInstance, item) {
        $scope.successrecords = item.SuccessCount;
        $scope.errorrecords = item.ErrorCount;
        $scope.data = item.UploadData;
        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
            $scope.$emit('SUBMIT_OK', true);

        };

    });


})();