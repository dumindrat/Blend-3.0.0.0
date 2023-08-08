(function () {
    var urlBlenCreation = 'api/Blend';
    var urlDocument = 'api/Document';

    app.controller('blendCreateController', function ($scope, $rootScope, $modal, $modalInstance, $route, $filter, item, httpService, globalService) {
        //var systemDate = $filter('date')(globalService.getDate(), 'yyyy-MM-dd');
        var systemDate = globalService.getDate();
        var completeDate = globalService.convertDateyyyymmdd(systemDate);
        $scope.buttonstatus = "Create";
        SetNew();

        function SetNew() {
            $scope.New = { 'BlendId': 0, 'ContractId': 0, 'StandardId': 0, 'BlendNo': '', 'OrderQty': '', 'IsAutoDocNo': true, 'CompleteDate': completeDate };
            $scope.boxnodisabled = false;
            //$scope.showLoader = false;

        }

        //Set Create or Update status
        SetTransaction(item);

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        globalService.openLoadingWindow('Loading..');

        function AutoGenNo() {
            //$scope.showLoader = true;
            globalService.openLoadingWindow('Loading..');

            httpService.getdata(urlDocument + '/GetDocumentNo/' + 2, function (results) {
                $scope.New.BlendNo = results.data.FormattedDocNo;
                angular.element(document.getElementById('blendno'))[0].disabled = true;
                //$scope.showLoader = false;
                $scope.$emit('LOAD_DATA_EVENT', true);

            });
        }

        $scope.setAutoDocNo = function (status) {
            if (status === true) {
                AutoGenNo();
            }
            else {
                angular.element(document.getElementById('blendno'))[0].disabled = false;
                $scope.New.BlendNo = '';
                angular.element('#' + 'blendno').focus()
            }
        };

        getOpenContractList();

        function getOpenContractList() {
            httpService.getdata('api/Contract/GetAllContracts/all', function (results) {
                $scope.ContractData = results.data;
                $scope.$emit('LOAD_DATA_EVENT', true);

            });

        };

        function getStandardList(contractid) {
            httpService.getdata('api/ContractDetail/GetBuyerContractDetails/' + contractid, function (results) {
                $scope.StandardData = results.data;
            });
        }

        $scope.getStandard = function (contractid) {
            $scope.BlendData = [];
            $scope.New.OrderQty = 0;
            globalService.openLoadingWindow('Loading..');

            httpService.getdata('api/ContractDetail/GetBuyerContractDetails/' + contractid, function (results) {
                $scope.StandardData = results.data;
                $scope.$emit('LOAD_DATA_EVENT', true);

            });
        };

        $scope.getBlendList = function (contractid, c) {

        };

        function SetTransaction(obj) {
            //New trans or update
            if (!angular.isUndefined(obj)) {
                if (obj.BlendId > 0) {
                    getStandardList(obj.ContractId);
                    //SetExportQty(obj.StandardId);
                    $scope.boxnodisabled = true;
                    $scope.buttonstatus = "Update";
                    $scope.New = obj;
                    $scope.New.BlendNo = obj.BlendNo;
                }
            }
            else {
                $scope.New.IsAutoDocNo = true;
                AutoGenNo();
                $scope.buttonstatus = "Create";

            }
        };


        $scope.setDisplayData = function (contractid, standardId) {
            for (var key in $scope.StandardData) {
                var test = {};
                var obj = $scope.StandardData[key];
                if (obj.StandardId === standardId) {
                    $scope.New.OrderQty = obj.OrderQty;
                    //angular.element('#blendno').focus()
                    break;
                }
            }

            if (standardId != null) {
                globalService.openLoadingWindow('Loading existing Blends..');

                httpService.getdata('api/ContractDetail/GetBlendsByContractAndStandard?' + 'contractId=' + contractid + '&standardId=' + standardId, function (results) {
                    $scope.BlendData = results.data;
                    $scope.$emit('LOAD_DATA_EVENT', true);

                });
            }
        };

        //Added lines
        $scope.openViewAddedBlends = function (p, size) {
            var modalInstance = $modal.open({
                templateUrl: '/App/Tea/BlendCreation/AddedBlends.html',
                controller: 'viewAddedBlendController',
                windowClass: 'app-modal-xsm-window',
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

        };

        var Validate = function () {
            if (!angular.isNumber(parseFloat($scope.New.ExportQty)) | angular.isUndefined($scope.New.ExportQty) | $scope.New.StandardId === 0 | $scope.New.ContractId === 0) {
                $scope.message = "Error in [Blend]";
                $scope.messageclass = "alert alert-danger";
                button.disabled = false;
                return false
            }
            return true;
        };

        $scope.create = function (blend) {
            $scope.button = {};
            $scope.button.disabled = true;
            $scope.disablestatus = true;

            if (Validate()) {
                if (blend.BlendId === 0) {
                    blend.CompleteDate = globalService.convertDateToIST(blend.CompleteDate);

                    httpService.savedata(urlBlenCreation, blend, function (response) {
                        if (response.status === 200) {
                            $scope.mesg = true;
                            $scope.message = "Success";
                            $scope.messageclass = "alert alert-success";
                            $scope.disablestatus = false;
                            $scope.$emit('BLEND_EVENT', true);
                            SetNew();
                            AutoGenNo();
                            $scope.button.disabled = false;
                        } else {
                            $scope.error = response.message;
                            $scope.button.disabled = false;
                        }
                    });

                }
                else {
                    httpService.updatedata(urlBlenCreation, blend, function (response) {
                        if (response.status === 200) {
                            alert('Successfully Updated');
                            $scope.$emit('BLEND_EVENT', true);
                            $scope.cancel();

                        } else {
                            $scope.error = response.message;
                            $scope.button.disabled = false;
                        }
                    });
                }
            }


        };

    });

    app.controller('viewAddedBlendController', function ($http, $modal, $scope, $modalInstance, $window, item) {

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.addedBlend = item;


    });
})();