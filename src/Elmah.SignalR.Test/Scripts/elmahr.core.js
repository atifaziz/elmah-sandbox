function KeyValuePair(key, value) {
    var self = this;

    self.key = key;
    self.value = value;
}

function ErrorViewModel(envelope) {
    var self = this;

    var time = new Date(parseFloat(envelope.error.time.slice(6, 19))).toLocaleString();
    var e = envelope.error;

    self.id = envelope.id;
    self.message = e.message;
    self.time = time;
    self.host = e.host;
    self.type = e.type;
    self.source = e.source;
    self.detail = e.detail;
    self.user = e.user;
    self.statusCode = e.statusCode;
    self.webHostHtmlMessage = e.webHostHtmlMessage;
    self.url = e.url;

    self.serverVariables = ko.observableArray([]);
    self.form = ko.observableArray([]);
    self.cookies = ko.observableArray([]);

    for (var sv in e.serverVariables) {
        self.serverVariables.push(new KeyValuePair(sv, e.serverVariables[sv]));
    };
    for (var f in e.form) {
        self.form.push(new KeyValuePair(f, e.form[f]));
    };
    for (var c in e.cookies) {
        self.cookies.push(new KeyValuePair(c, e.cookies[c]));
    };
}

function ApplicationViewModel(applicationName, infoUrl, doStats) {
    var self = this;

    self.applicationName = applicationName;
    self.infoUrl = infoUrl;
    self.errors = ko.observableArray([]);

    self.doStats = doStats;

    self.addError = function (envelope) {

        self.errors.push(new ErrorViewModel(envelope));
        self.errors.sort(function (l, r) {
            //descending by time
            return l.time > r.time ? -1 : (l.time == r.time ? 0 : 1);
        });
    };

    this.fadeIn = function (elem) { if (elem.nodeType === 1) $(elem).hide().slideDown(1200); };
}

function ElmahrViewModel() {
    var self = this;

    self.applications = ko.observableArray([]);
    self.allErrors = ko.observableArray([]);
    self.stats = ko.observableArray([]);

    self.doStats = function (errors) {
    };

    self.addApplication = function (applicationName, infoUrl) {
        var found = false;
        var apps = self.applications();
        for (a in apps) {
            var appName = apps[a].applicationName;
            if (appName == applicationName) {
                found = true;
                break;
            }
        }
        if (!found) {
            self.applications.push(new ApplicationViewModel(applicationName, infoUrl, self.doStats));
        }
    };

    self.addError = function (envelope) {

        self.allErrors.push(new ErrorViewModel(envelope));
        self.allErrors.sort(function (l, r) {
            //descending by time
            return l.time > r.time ? -1 : (l.time == r.time ? 0 : 1);
        });

        self.doStats(self.allErrors());
    };
}

var elmahr = new ElmahrViewModel();

$(function () {

    var elmahrConnector = $.connection.elmahr;

    var d = Rx.Observable.create(function(observer) {

        $.connection.hub.start(function() {
            elmahrConnector.connect();
        });

        elmahrConnector.notifyError = function (envelope) {            
            observer.onNext(envelope);
        };
        
    })
    .subscribe(function(envelope) {

        var infoUrl = envelope.infoUrl;

        elmahr.addApplication(envelope.applicationName, infoUrl);

        var apps = elmahr.applications();
        for (a in apps) {
            var appName = apps[a].applicationName;
            if (appName == envelope.applicationName) {
                elmahr.applications()[a].addError(envelope);
            }
        }

        elmahr.addError(envelope);
        
    });

    ko.applyBindings(elmahr);
    
});
