app.controller('mainController', function ($scope, $rootScope, $location, $modal, $filter, globalService, httpService, AuthenticationService) {
    $rootScope.loadingdata = true;
    $rootScope.loadings = false;
    $rootScope.hideMenu = true;
    $rootScope.showbanner = false;
    $rootScope.moduleName = 'Tea'
    $rootScope.moduleId = 1;

    $rootScope.SiteMenu = {};
    $scope.SiteMenuType1 = {};
    $scope.SiteMenuType2 = {};
    $scope.SiteMenuType3 = {};
    $scope.SiteMenuType4 = {};
    $scope.SiteMenuType5 = {};
    $scope.SiteMenuType6 = {};
    $scope.SiteMenuType7 = {};
    $scope.SiteMenuType8 = {};
    $scope.SiteMenuType9 = {};
    $scope.QuickLinks = {};

    $scope.resetmenu = function (mainModule, moduleName) {
        $scope.selectedMod = moduleName;
        var retVal = confirm("Are you sure want to access module " + moduleName + "?");
        if (retVal == true) {
            var currentUser = globalService.getcurrentuser();
            $rootScope.dataLoading = true
            httpService.getdata('api/Menu/GetMenuByUserAndMainModule?id=' + currentUser.username + '&mainmodule=' + mainModule, function (results) {
                $rootScope.$broadcast('menuChange', results.data);
                $rootScope.dataLoading = false
                $rootScope.moduleName = moduleName;
                $rootScope.moduleId = mainModule;
                $('#mainView').empty();
            });
        }
    }

    function GetCompany() {
        var currentUser = globalService.getcurrentuser();
        $rootScope.menuAccess.AccessLink = '#UserCompany'

        httpService.getdata('api/Company/GetDefaultUserCompany?id=' + currentUser.username + '&' + 'status=IsDefault', function (results) {
            if (results.data.CompanyId > 0) {                
                $rootScope.userCompany = results.data;
            }
            else {
                alert('No company assigned to the user. Please contact system Administrator to get assigned a working company. !');
                var user = { 'UserName': '', 'Token': '' };
                user.UserName = globalService.getcurrentuser().username;
                user.Token = globalService.getcurrentuser().token;
                globalService.openLoadingWindow('Signing Out..');

                AuthenticationService.Logoff(user, function (response) {
                    if (response.status === 200) {

                        AuthenticationService.ClearCredentials();
                        $scope.$emit('USER_LOGOFF', true);
                        $location.path('/Login');
                    } else {
                        $scope.error = response.message;
                    }
                    $scope.$emit('LOAD_DATA_EVENT', true);

                });
            }
        });
    }

    function showCompanyWindow() {
        alert('I am foo!');
        return false;
    }

    //This is for event listner
    $rootScope.currentuser = {};

    $rootScope.logonstatus = { currentuser: '', href: '', text: '', link: '' };

    $rootScope.$on('menuChange', function (event, data) {
        $scope.SiteMenu = data;
        $scope.SiteMenuType1 = getMenuList(1);
        $scope.SiteMenuType2 = getMenuList(2);
        $scope.SiteMenuType3 = getMenuList(3);
        $scope.SiteMenuType4 = getMenuList(4);
        $scope.SiteMenuType5 = getMenuList(5);
        $scope.SiteMenuType7 = getMenuList(7);
        $scope.SiteMenuType8 = getMenuList(8);
        $scope.SiteMenuType9 = getMenuList(9);
        $scope.QuickLinks = getQuickMenu();

        globalService.setmenudata(data);
    });

    function getMenuList(type) {
        var SiteMenuByType = [];
        for (var key in $scope.SiteMenu) {
            var test = {};
            var obj = $scope.SiteMenu[key];
            if (obj.ModuleId === type) {
                SiteMenuByType.push(obj);
            }
        }
        return SiteMenuByType;
    };

    function getQuickMenu() {
        var QuickLinks = [];
        for (var key in $scope.SiteMenu) {
            var obj = $scope.SiteMenu[key];
            if (obj.IsQuickLink === true) {
                QuickLinks.push(obj);
            }
        }
        return QuickLinks;
    };

    //Logon user name event listner
    $scope.$on('USER_LOGON', function (event, data) {
        $scope.currentuser = data;
        $scope.logonstatus.currentuser = 'Welcome :' + data;
        $scope.logonstatus.href = '#Logoff';
        $scope.logonstatus.text = 'Welcome :';
        $scope.logonstatus.link = 'Logoff';
        $scope.logonstatus.class = 'glyphicon glyphicon-log-in';
        $rootScope.hideMenu = false;
        GetCompany();
    });

    //Logoff user  event listner.Clear menu items
    $rootScope.$on('USER_LOGOFF', function (event, data) {
        $scope.currentuser = '';
        $scope.SiteMenu = {};
        $scope.logonstatus.currentuser = '';
        $scope.logonstatus.href = '#Login';
        $scope.logonstatus.text = '';
        $scope.logonstatus.link = 'Login';
        $scope.logonstatus.class = 'glyphicon glyphicon-log-out';
        $rootScope.hideMenu = true;

    });

    $rootScope.$on('USER_AUTH_REQUIRED', function (event, data) {
        $scope.currentuser = '';
        $scope.SiteMenu = {};
        $scope.logonstatus.currentuser = '';
        $scope.logonstatus.href = '#Login';
        $scope.logonstatus.text = '';
        $scope.logonstatus.link = 'Login';
        $scope.logonstatus.class = 'glyphicon glyphicon-log-out';
        $rootScope.hideMenu = true;

    });

    $rootScope.$on('USER_REGISTER', function (event, data) {
        $scope.currentuser = '';
        $scope.SiteMenu = {};
        $scope.logonstatus.currentuser = '';
        $scope.logonstatus.href = '#Login';
        $scope.logonstatus.text = '';
        $scope.logonstatus.link = 'Login';
    });

    $scope.$on('USER_COMPANY_CHANGE', function (event, data) {
        GetCompany();
    });

    $rootScope.CloseBanner = function () {
        $rootScope.showbanner = false;

    }

    $rootScope.$on('PASSWORD_EXPIRE', function (event, data) {
        $rootScope.showbanner = true;
        $rootScope.bannermessage = 'Your Password already being Expired.Please reset the Password';
    });

    $scope.$on('DATA_LOADING', function (event, data) {
        $rootScope.maindataloading = data;
    });



    var curUser = globalService.getcurrentuser();
    var username = curUser.username;
    if (username == '') {
        //alert('Required login to the system');
        $location.path('/Login');
    }


});