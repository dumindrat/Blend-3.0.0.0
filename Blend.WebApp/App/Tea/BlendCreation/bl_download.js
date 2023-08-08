(function () {
    var urlBlendClose = 'api/BlendClose';

    app.controller('blendDownloadController', function ($scope, $rootScope, $modalInstance, $http, item, globalService) {
        $scope.BlendInfo = [];
        $scope.SelectedBlend = item;

        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        $scope.download = function (type) {
            var retVal = confirm("Are you sure want to download?");
            if (retVal == true) {
                globalService.openLoadingWindow("Downloading..");
                $http.get('api/Blend/' + type + '/' + $scope.SelectedBlend.BlendId, { responseType: 'arraybuffer' }).success(function (data, status, headers) {
                    headers = headers();

                    var filename = headers['x-filename'];
                    var contentType = headers['content-type'];

                    var linkElement = document.createElement('a');
                    var blob = new Blob([data], { type: contentType });
                    var url = window.URL.createObjectURL(blob);

                    linkElement.setAttribute('href', url);
                    linkElement.setAttribute("download", filename);

                    var clickEvent = new MouseEvent("click", {
                        "view": window,
                        "bubbles": true,
                        "cancelable": false
                    });
                    linkElement.dispatchEvent(clickEvent);
                    $scope.$emit('LOAD_DATA_EVENT', true);

                }).error(function (data) {
                    alert('Error in downloading xls file.Contact Administrator.')
                    $scope.$emit('LOAD_DATA_EVENT', true);
                });
            }
        };


    });
})();