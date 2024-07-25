(function () {
    var urlBlendClose = 'api/BlendClose';

    app.controller('blendTraceController', function ($scope, $rootScope, $modalInstance, item, httpService, globalService) {
        $scope.BlendInfo = [];
        $scope.SelectedBlend = item;
        $scope.LineSummary = {};
        $scope.Allocations = [];
        $scope.linedataLoading = false;
        $scope.summarydataLoading = false;
        $scope.allocationdataLoading = false;
        $scope.linedataLoading = false;

        //Authentication fails. emit from data.service
        $rootScope.$on('CLOSE_POPUP', function (event, data) {
            $scope.cancel();
        });

        $scope.cancel = function () {
            $modalInstance.dismiss('Close');
        };

        getBlendClose(item.BlendId);
        function getBlendClose(blendId) {
            var info = { 'Item': '', 'Value': '' };
            $scope.linedataLoading = true;

            httpService.getdata(urlBlendClose + '/GetBlendCloseInfo/' + blendId, function (results) {
                $scope.BlendInfo.push({ 'Item': 'Export Qty', 'Value': results.data.ExportQty + ' Kg'});
                $scope.BlendInfo.push({ 'Item': 'Total Blend Qty', 'Value': results.data.LineQty + ' Kg' });
                $scope.BlendInfo.push({ 'Item': 'Rate', 'Value': results.data.Rate });
                $scope.BlendInfo.push({ 'Item': 'Blend Closed', 'Value': results.data.IsClosed === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'Closed Rate', 'Value': results.data.ClosedRate });
                $scope.BlendInfo.push({ 'Item': 'STDBB', 'Value': results.data.STDBB + ' Kg' });
                $scope.BlendInfo.push({ 'Item': 'STDBG', 'Value': results.data.STDBG + ' Kg' });
                $scope.BlendInfo.push({ 'Item': 'SIFTED', 'Value': results.data.SiftedQty + ' Kg' });
                $scope.BlendInfo.push({ 'Item': 'Blend Loss', 'Value': results.data.BL + ' Kg' });
                $scope.BlendInfo.push({ 'Item': 'Close Date', 'Value': results.data.CloseDate });
                $scope.BlendInfo.push({ 'Item': 'Requested to Reverse the blend', 'Value': results.data.IsCancelRequest === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'Shipment done', 'Value': results.data.IsShipment === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'Rate Converted', 'Value': results.data.IsConverted === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'PackGoods Created', 'Value': results.data.IsPackGoodsCreate === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'PackGoods Created Date', 'Value': results.data.PackGoodsCreateDate });

                $scope.linedataLoading = false;

            });
        };

        getBlendAllocatedSL(item.BlendId);

        function getBlendAllocatedSL(blendId) {
            $scope.summarydataLoading = true;

            httpService.getdata('api/Blend' + '/GetBlendItemSummaryById/' + blendId, function (results) {
                $scope.LineSummary = results.data;
                $scope.summarydataLoading = false;

            });
        };



        getBlendAllocations(item.BlendId);

        function getBlendAllocations(blendId) {
            $scope.allocationdataLoading = true;

            httpService.getdata('api/Blend' + '/GetBlendAllocationsBylendId/' + blendId, function (results) {
                $scope.Allocations = results.data;
                $scope.allocationdataLoading = false;

            });
        };
    });
})();