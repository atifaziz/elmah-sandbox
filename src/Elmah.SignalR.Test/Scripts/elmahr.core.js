function KeyValuePair(key, value) {
    var self = this;

    self.key = key;
    self.value = value;
}

function ErrorViewModel(envelope) {
    var self = this;

    var time = new Date(parseFloat(envelope.Error.Time.slice(6, 19))).toLocaleString();
    var e = envelope.Error;

    self.id = envelope.id;
    self.message = e.Message;
    self.time = time;
    self.isoTime = e.IsoTime;
    self.host = e.Host;
    self.type = e.Type;
    self.shortType = e.ShortType;
    self.source = e.Source;
    self.detail = e.Detail;
    self.user = e.User;
    self.statusCode = e.StatusCode;
    self.webHostHtmlMessage = e.WebHostHtmlMessage;
    self.hasYsod = e.HasYsod;
    self.url = e.Url;
    self.browserSupportUrl = e.BrowserSupportUrl;

    self.serverVariables = ko.observableArray([]);
    self.form = ko.observableArray([]);
    self.cookies = ko.observableArray([]);

    for (var sv in e.ServerVariables) {
        self.serverVariables.push(new KeyValuePair(sv, e.ServerVariables[sv]));
    };
    for (var f in e.Form) {
        self.form.push(new KeyValuePair(f, e.Form[f]));
    };
    for (var c in e.Cookies) {
        self.cookies.push(new KeyValuePair(c, e.Cookies[c]));
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

    this.fadeIn = function (elem) {
         if (elem.nodeType === 1) {
             $(elem).hide().slideDown(1200);
             $("abbr.timeago", elem).timeago();
         }
    };
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

        elmahrConnector.notifyErrors = function (envelopes) {            
            
            for (k in envelopes) {
                var envelope = envelopes[k];
                observer.onNext(envelope);
            }
        };
        
    });
    
    d.subscribe(function(envelope) {

        var infoUrl = envelope.InfoUrl;

        elmahr.addApplication(envelope.ApplicationName, infoUrl);

        var apps = elmahr.applications();
        for (a in apps) {
            var appName = apps[a].applicationName;
            if (appName == envelope.ApplicationName) {
                elmahr.applications()[a].addError(envelope);
            }
        }

        elmahr.addError(envelope);
        
        
    });

    ko.applyBindings(elmahr);
    
});
