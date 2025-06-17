function dataget(url) {
    return $.ajax({
        type: 'GET',
        url: url,
        contentType: "application/json; charset=utf-8",
        dataType: 'json'
    });
}
function datapost(row, url) {
    return $.ajax({
        type: 'POST',
        url: url,
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        data: JSON.stringify(row)
    });
}
function filedatapost(row, url) {
    return $.ajax({
        type: 'POST',
        url: url,        
        data: row,
        cache: false,
        contentType: false,
        processData: false
    });
}
function readcallback(url, successCallback, errorCallback) {
    $.ajax({
        type: 'GET',
        url: url,
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: successCallback,
        error: errorCallback,
    });
}
function savecallback(row, url) {
    $.ajax({
        type: 'POST',
        url: url,
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        data: JSON.stringify(row),
        success: successCallback,
        error: errorCallback,
    });
}

function isEmpty(value) {
    return (value == undefined || value == null);
}

function getNowDate(plusday) {
    let dt = Date.now();
    let nowday;
    if (plusday == undefined || plusday == null) {        
        let y = dt.getFullYear();
        let m = dt.getMonth();
        let d = dt.getDay();
        nowday = y + '/' + m + '/' + d;        
    } else {

    }
    return nowday;
}
