var app = angular.module('myApp', ['ngRoute', 'ngGrid', 'ui.bootstrap']);

app.config(['$routeProvider',
function ($routeProvider) {
    $routeProvider.
        when('/Login', {
            templateUrl: '/App/Authentication/Login/login.html',
            controller: 'LoginController'
        }).
         when('/Logoff', {
             templateUrl: '/App/Authentication/Login/logoff.html',
             controller: 'LoginController'
         }).
        when('/ResetDoc', {
            templateUrl: '/App/Document/Document.html',
            controller: 'resetDocumetController'
        }).
        when('/Buyers', {
            templateUrl: '/App/Tea/TeaBuyer/BuyerDetail.html',
            controller: 'buyerController'
        }).
        when('/TeaStandards', {
            templateUrl: '/App/Tea/TeaStandards/TeaStandard.html',
            controller: 'standardController'
        }).
        when('/Broker', {
            templateUrl: '/App/Tea/Broker/BrokerDetail.html',
            controller: 'brokerController'
        }).
         when('/Contracts', {
             templateUrl: 'App/Tea/Contracts/ContractDetail.html',
             controller: 'contractController'
         }).
         when('/Purchase', {
             templateUrl: 'App/Tea/Purchase/Sale.html',
             controller: 'straightLineController'
         }).
         when('/BlendCreation', {
             templateUrl: 'App/Tea/BlendCreation/Blend.html',
             controller: 'blenCreationController'
         }).
         when('/BlendClose', {
             templateUrl: 'TeaMix/BlendClose/BlendDetails.html',
             controller: 'blenCloseDetailController'
         }).
         when('/Adjustments', {
             templateUrl: 'App/Tea/Adjustment/Reject_Short/StraightLines.html',
             controller: 'adjustmentController'
         }).
         when('/BlendClose', {
             templateUrl: 'App/tea/BlendClose/BlendDetails.html',
             controller: 'blenCloseDetailController'
         }).
        when('/LineSummary', {
            templateUrl: 'App/tea/LineSummary/StraightLines.html',
            controller: 'straightLineSummaryController'
        }).
         when('/Role', {
             templateUrl: 'App/Authentication/Roles/Roles.html?ver1.3',
             controller: 'roleController'
         }).
         when('/Register', {
             templateUrl: 'App/Authentication/Register/Register.html',
             controller: 'RegisterController'
         }).
        when('/ShipTea', {
            templateUrl: 'App/Tea/Shipments/ClosedBlends.html',
            controller: 'teaShipmentController'
        }).
        when('/PurchseReports', {
            templateUrl: 'App/Tea/Reports/Purchase/PurchaseReport.html',
            controller: 'purchaseReportController'
        }).
        when('/BlendReport', {
            templateUrl: 'App/Tea/Reports/Blend/BlendReport.html',
            controller: 'blendReportController'
        }).
        when('/StockReport', {
            templateUrl: 'App/Tea/Reports/Stock/StockReport.html',
            controller: 'stockReportController'
        }).
        when('/ApproveReverseAdjustments', {
            templateUrl: 'App/Tea/Adjustment/Approvals/StraightLines.html',
            controller: 'cancellationApproveController'
        }).
        when('/ApproveReverseBlendClose', {
            templateUrl: 'App/Tea/Approval/BlendCloseCancel/BlendCloseRequest.html',
            controller: 'blendCloseReversalAppove'
        }).
           when('/RejectShortReport', {
               templateUrl: 'App/Tea/Reports/Reject-Short/Options.html',
               controller: 'printNoteController'
           }).
        when('/ItemMaster', {
            templateUrl: 'App/Packing/ItemMaster/Items.html',
            controller: 'itemController'
        }).
         when('/Supplier', {
             templateUrl: 'App/Packing/Supplier/List.html',
             controller: 'supplierController'
         }).
        when('/TeaCollection', {
            templateUrl: 'App/Tea/Collection/CollectionNotes.html',
            controller: 'straightLineCollectionController'
        }).
          when('/PurchaseOrder', {
              templateUrl: 'App/Packing/PO/List.html',
              controller: 'purchaseOrderController'
          }).
        when('/ItemReceive', {
            templateUrl: 'App/Packing/ItemReceive/List.html',
            controller: 'itemReceiveController'
        }).
        when('/ItemIssue', {
            templateUrl: 'App/Packing/ItemIssue/List.html',
            controller: 'itemIssueController'
        }).
        when('/ItemReturn', {
            templateUrl: 'App/Packing/ItemReturn/List.html',
            controller: 'itemReturnController'
        }).
        when('/MyProfile', {
            templateUrl: 'App/Authentication/MyProfile/Options.html',
            controller: 'myProfileController'
        }).
        when('/ResetDocumentPack', {
            templateUrl: '/App/Document/Document.html',
            controller: 'resetDocumetController'
        }).
        when('/UserCompany', {
            templateUrl: '/App/Authentication/UserCompany/UserCompany.html',
            controller: 'userCompanyController'
        }).
        otherwise({
            redirectTo: '/'
        });


}]);

app.config(['$httpProvider', function ($httpProvider, $rootScope, $route, globalService) {
    // Pull in `userService` from the dependency injector
    $httpProvider.interceptors.push(function ($rootScope) {
        return {
            request: function (req) {
                // Set the `Authorization` header for every outgoing HTTP request
                req.headers.Authorization = $rootScope.currentUser.authdata;
                req.headers['SysUser'] = $rootScope.currentUser.username;
                req.headers['Token'] = $rootScope.currentUser.token;
                req.headers['AccessLink'] = $rootScope.menuAccess.AccessLink;
                if ($rootScope.userCompany != undefined) {
                    req.headers['SysCompany'] = $rootScope.userCompany.CompanyId;
                }
                return req;
            }
        };
    })
}]);

app.config(['$compileProvider', function ($compileProvider) {
    $compileProvider.debugInfoEnabled(false);
}])

app.directive('validNumber', function () {
    return {
        require: '?ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            if (!ngModelCtrl) {
                return;
            }

            ngModelCtrl.$parsers.push(function (val) {
                if (angular.isUndefined(val)) {
                    var val = '';
                }
                var clean = val.replace(/[^0-9]+/g, '');
                if (val !== clean) {
                    ngModelCtrl.$setViewValue(clean);
                    ngModelCtrl.$render();
                }
                return clean;
            });

            element.bind('keypress', function (event) {
                if (event.keyCode === 32) {
                    event.preventDefault();
                }
            });
        }
    };
});

app.directive('onlyDigits', function () {
    return {
        require: 'ngModel',
        restrict: 'A',
        link: function (scope, element, attr, ctrl) {
            function inputValue(val) {
                if (val) {
                    var digits = val.replace(/[^0-9.]/g, '');

                    if (digits !== val) {
                        ctrl.$setViewValue(digits);
                        ctrl.$render();
                    }
                    return parseFloat(digits);
                }
                return undefined;
            }
            ctrl.$parsers.push(inputValue);
        }
    };
});

app.directive("limitTo", [function () {
    return {
        restrict: "A",
        link: function (scope, elem, attrs) {
            var limit = parseInt(attrs.limitTo);
            angular.element(elem).on("keypress", function (e) {
                if (this.value.length == limit) e.preventDefault();
            });
        }
    }
}]);

app.directive('ngFiles', ['$parse', function ($parse) {

    function fn_link(scope, element, attrs) {
        var onChange = $parse(attrs.ngFiles);
        element.on('change', function (event) {
            onChange(scope, { $files: event.target.files });
        });
    };

    return {
        link: fn_link
    }
}]);

app.directive('myEnter', function () {
    return function (scope, element, attrs) {
        element.bind("keydown keypress", function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    scope.$eval(attrs.myEnter);
                });

                event.preventDefault();
            }
        });
    };
});


app.directive('focusMe', ['$timeout', '$parse', function ($timeout, $parse) {
    return {
        //scope: true,   // optionally create a child scope
        link: function (scope, element, attrs) {
            var model = $parse(attrs.focusMe);
            scope.$watch(model, function (value) {
                console.log('value=', value);
                //alert('value='+ value);
                if (value === true) {
                    $timeout(function () {
                        element[0].focus();
                    });
                }
            });
            // to address @blesh's comment, set attribute value to 'false'
            // on blur event:
            element.bind('blur', function () {
                console.log('blur');
                scope.$apply(model.assign(scope, false));
            });
        }
    };
}]);






