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
                $scope.BlendInfo.push({ 'Item': 'Export Qty', 'Value': results.data.ExportQty });
                $scope.BlendInfo.push({ 'Item': 'Total Blend Qty', 'Value': results.data.LineQty });
                $scope.BlendInfo.push({ 'Item': 'Rate', 'Value': results.data.Rate });
                $scope.BlendInfo.push({ 'Item': 'Is Closed', 'Value': results.data.IsClosed === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'Closed Rate', 'Value': results.data.ClosedRate });
                $scope.BlendInfo.push({ 'Item': 'Close Date', 'Value': results.data.CloseDate });
                $scope.BlendInfo.push({ 'Item': 'Is Converted', 'Value': results.data.IsConverted === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'Is CancelRequest', 'Value': results.data.IsCancelRequest === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'Is Shipment done', 'Value': results.data.IsShipment === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'Is Rate Converted', 'Value': results.data.IsConverted === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'Is Cancel Request Sent', 'Value': results.data.IsCancelRequest === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'Is PackGoods Create', 'Value': results.data.IsPackGoodsCreate === true ? 'Yes' : 'No' });
                $scope.BlendInfo.push({ 'Item': 'PackGoods Create Date', 'Value': results.data.PackGoodsCreateDate });

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