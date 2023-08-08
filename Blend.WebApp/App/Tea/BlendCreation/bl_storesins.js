(function () {
    var urlBlendIns = 'api/StoresInstruction';

    app.controller('blendStoresInstructionController', function ($scope, $rootScope, $modalInstance, $http, item, httpService, globalService) {
        $scope.BlendInfo = [];
        $scope.SelectedBlend = item;
        var obj = { 'BlendId': item.BlendId, 'StoresInstruction': '' };


        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $rootScope.$on('LOAD_DATA_EVENT', function (event, data) {
            getStoresIns(item.BlendId);
        });

        getStoresIns(item.BlendId);

        function getStoresIns(blendId) {
            $scope.summarydataLoading = true;

            httpService.getdata(urlBlendIns + '/GetStoresInstructions/' + blendId, function (results) {
                $scope.storesInstructions = results.data;
                $scope.summarydataLoading = false;

            });
        };

        $scope.AddInstruction = function (ins) {
            obj.StoresInstruction = ins;
            globalService.openLoadingWindow("Adding..");
            httpService.savedata(urlBlendIns, obj, function (response) {
                if (response.status === 200) {
                    $scope.$emit('LOAD_DATA_EVENT', true);
                } else {
                    $scope.message = response.message;
                    $scope.$emit('LOAD_DATA_EVENT', true);

                }
            });
        }

        $scope.RemoveInstruction = function (id) {
            obj.Id = id;
            var retVal = confirm("Are you sure want to remove Instruction ?");
            if (retVal == true) {
                globalService.openLoadingWindow("Removing..");
                httpService.updatedata(urlBlendIns, obj, function (response) {
                    if (response.status === 200) {
                        $scope.$emit('LOAD_DATA_EVENT', true);
                    } else {
                        $scope.message = response.message;
                        $scope.$emit('LOAD_DATA_EVENT', true);

                    }
                });
            }
        }

        $scope.print = function () {
            globalService.openLoadingWindow("Stores Instructions..");
            var doctype = 'application/pdf';

            $http.post('api/StoresInstructionReport', obj, { responseType: 'arraybuffer' }).success(function (data) {
                var file = new Blob([data], { type: doctype });
                var fileURL = URL.createObjectURL(file);
                window.open(fileURL);
                $scope.$emit('LOAD_DATA_EVENT', true);

            }).error(function (data) {
                alert('Error in processing report')
                $scope.$emit('LOAD_DATA_EVENT', true);

            });
        };

    })

})();