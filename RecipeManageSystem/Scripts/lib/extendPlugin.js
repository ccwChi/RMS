$.fn.datebox.defaults.formatter = function (date) {
    var y = date.getFullYear();
    var m = date.getMonth() + 1;
    var d = date.getDate();
    return y + '-' + (m < 10 ? ('0' + m) : m) + '-' + (d < 10 ? ('0' + d) : d);
};
$.fn.datebox.defaults.parser = function (s) {
    if (!s) return new Date();
    var ss = s.split('/');
    var y = parseInt(ss[0], 10);
    var m = parseInt(ss[1], 10);
    var d = parseInt(ss[2], 10);
    if (!isNaN(y) && !isNaN(m) && !isNaN(d)) {
        return new Date(y, m - 1, d);
    } else {
        return new Date();
    }
};
$.map(['validatebox', 'textbox', 'passwordbox', 'filebox', 'searchbox',
    'combo', 'combobox', 'combogrid', 'combotree',
    'datebox', 'datetimebox', 'numberbox',
    'spinner', 'numberspinner', 'timespinner', 'datetimespinner'], function (plugin) {
        if ($.fn[plugin]) {
            $.fn[plugin].defaults.missingMessage = '此欄位為必填欄位.';
        }
    });
function combofilter(q, row) {
    var opts = $(this).combobox('options');
    var rex = /[^\u4e00-\u9fa5]/;
    if (rex.test(q)) {
        return row[opts.valueField].toLowerCase().indexOf(q.toLowerCase()) >= 0;
    } else {
        return row[opts.textField].toLowerCase().indexOf(q.toLowerCase()) >= 0;
    }
}
$.extend({}, $.fn.combobox.defaults.inputEvents, {
    blur: function (e) {
        var target = e.data.target;
        var rows = $(target).combobox('getData');
        var vv = [];

        $.map($(target).combobox('getValues'), function (v) {
            if (getRowIndex(target, v) >= 0) {
                vv.push(v);
            }
        });
        $(target).combobox('setValues', vv);

        function getRowIndex(target, value) {
            var state = $.data(target, 'combobox');
            var opts = state.options;
            var data = state.data;
            for (var i = 0; i < data.length; i++) {
                if (data[i][opts.valueField] == value) {
                    return i;
                }
            }
            return -1;
        }
    }
})

