(function () {
    'use strict';

    app.factory('globalService', ['$http', '$rootScope', '$modal', '$filter', function ($http, $rootScope, $modal, $filter) {


        var service = {};
        $rootScope.currentUser = { username: '', authdata: '' };
        $rootScope.siteMenu = {};
        $rootScope.menuAccess = { AccessLink: '', C: false, M: false, D: false, V: false, P: false };

        service.setcurrentuser = function (username, authdata, token) {

            $rootScope.currentUser.username = username;
            $rootScope.currentUser.authdata = authdata;
            $rootScope.currentUser.token = token;

        };
        service.getcurrentuser = function () {
            return $rootScope.currentUser;
        };
        service.clearcredentials = function () {

        };
        service.setmenudata = function (menu) {
            $rootScope.siteMenu = menu;
        }
        service.getaccess = function (menuname) {
            var serachChar = menuname.replace('/', '#');
            var menuObject = $filter('filter')($rootScope.siteMenu, { href: serachChar });
            $rootScope.menuAccess.AccessLink = serachChar;
            $rootScope.menuAccess.C = menuObject[0].AccessLevel.split("")[0] == 'C' ? false : true;//ng-disables=true/false
            $rootScope.menuAccess.M = menuObject[0].AccessLevel.split("")[1] == 'M' ? false : true;
            $rootScope.menuAccess.D = menuObject[0].AccessLevel.split("")[2] == 'D' ? false : true;
            $rootScope.menuAccess.V = menuObject[0].AccessLevel.split("")[3] == 'V' ? false : true;
            $rootScope.menuAccess.P = menuObject[0].AccessLevel.split("")[4] == 'P' ? false : true;
            return $rootScope.menuAccess;
        }

        service.gethttpmessage = function (status) {
            switch (status) {
                case 200:
                    return "Success";
                    break;
                case 401:
                    return "Authentication fail";
                    break;
                case 400:
                    return "Bad Request";
                    break;
                case 500:
                    return "Internal Server Error";
                    break;
                case 417:
                    return "System.Expectation Failed";
                    break;
                default:

            }
        };

        service.replaceSlash = function (string) {
            //var result = string.replace("/", "~");
            var result = string.replace(/\//g, "~");
            return result;
        };

        service.getDate = function () {
            return new Date();
        }

        service.GridRowHeight = function () {
            return 30;
        }

        service.getDataLoadingDelay = function () {
            return 1000;
        }

        service.getDataLoadingMessage = function () {
            return '( Loading )';
        }

        service.getDataCountMessage = function (data) {
            return '(' + data.toString() + ') Records found';;
        }

        service.openLoadingWindow = function (p) {
            var modalInstance = $modal.open({
                templateUrl: '/Utility/Loading.html',
                controller: 'loadingController',
                windowClass: 'app-modal-loading-window',
                backdrop: 'static',
                keyboard: false,
                resolve: {
                    item: function () {
                        return p;
                    }
                }
            });

            //$('#myModal').css('display', 'block');
            //angular.element('#myModal').modal({ backdrop: 'static','z-index': 10000000 });
        };

       
        service.convertDecimal = function (value) {
            var dec = value.substr(value.length - 3);
            var dot = dec.charAt(0);
            if (dot === '.') {
                return value;
            }
            else {
                value = value.slice(0, -3);
                value = value + '.00';
            }
            return value;
        };

        service.convertDateToIST = function (date) {
            var d = new Date(date);
            if (!!d.valueOf()) { // Valid date
                var year = d.getFullYear();
                var month = d.getUTCMonth() + 1;
                var day = d.getDate();
            }
            return month + '/' + day + '/' + year;
        }

        service.convertDateyyyymmdd = function (date) {
            var d = new Date(date);
            if (!!d.valueOf()) {
                var year = d.getFullYear();
                var month = d.getUTCMonth() + 1;
                var day = d.getDate();

                var sMonth = month.toString().length === 1 ? '0' + month.toString() : month.toString();
                var sDay = day.toString().length === 1 ? '0' + day.toString() : day.toString();

            }
            return year + '-' + sMonth + '-' + sDay;
        }

        service.RequestAuthentication = function () {
            $rootScope.$emit('USER_AUTH_REQUIRED', true);
            $rootScope.$emit('CLOSE_POPUP', true);
        }
        return service;

    }]);



})();