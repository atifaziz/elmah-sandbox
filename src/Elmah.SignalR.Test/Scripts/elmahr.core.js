function keyValuePair(key, value) {
    var self = this;

    self.key = key;
    self.value = value;
}

function errorViewModel(envelope) {
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
        self.serverVariables.push(new keyValuePair(sv, e.serverVariables[sv]));
    };
    for (var f in e.form) {
        self.form.push(new keyValuePair(f, e.form[f]));
    };
    for (var c in e.cookies) {
        self.cookies.push(new keyValuePair(c, e.cookies[c]));
    };
}

function applicationViewModel(applicationName, infoUrl, doStats) {
    var self = this;

    self.applicationName = applicationName;
    self.infoUrl = infoUrl;
    self.errors = ko.observableArray([]);

    self.doStats = doStats;

    self.addError = function (envelope) {

        self.errors.push(new errorViewModel(envelope));
        self.errors.sort(function (l, r) {
            //descending by time
            return l.time > r.time ? -1 : (l.time == r.time ? 0 : 1);
        });
    };

    this.fadeIn = function (elem) { if (elem.nodeType === 1) $(elem).hide().slideDown(1200); };
}

function errorsViewModel() {
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
            self.applications.push(new applicationViewModel(applicationName, infoUrl, self.doStats));
        }
    };

    self.addError = function (envelope) {

        self.allErrors.push(new errorViewModel(envelope));
        self.allErrors.sort(function (l, r) {
            //descending by time
            return l.time > r.time ? -1 : (l.time == r.time ? 0 : 1);
        });

        self.doStats(self.allErrors());
    };
}

var model = new errorsViewModel();

$(function () {

    var elmahr = $.connection.elmahr;

    $.connection.hub.start(function () {
        elmahr.connect();
    });

    elmahr.notifyError = function (envelope) {
        var infoUrl = envelope.infoUrl;

        model.addApplication(envelope.applicationName, infoUrl);

        var apps = model.applications();
        for (a in apps) {
            var appName = apps[a].applicationName;
            if (appName == envelope.applicationName) {
                model.applications()[a].addError(envelope);
            }
        }

        model.addError(envelope);
    };

    ko.applyBindings(model);
});
