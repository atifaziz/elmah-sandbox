elmahr.doStats = function (errors) {

    //errors count
    var total = _.chain(errors)
        .reduce(function (acc, e) { return acc + 1; }, 0)
        .value();
    $('#total').text(total);

    //error types count
    var types = _.chain(errors)
        .groupBy(function (e) { return e.type; })
        .reduce(function (acc, e) { return acc + 1; }, 0)
        .value();
    $('#types').text(types);

    //types stats
    elmahr.stats.removeAll();
    _.chain(errors)
        .groupBy(function (q) { return q.type; })
        .map(function (val, key) { return new KeyValuePair(key, val.length); })
        .sortBy(function (kvp) { return kvp.key; })
        .each(function (kvp) { elmahr.stats.push(kvp); });
    
};