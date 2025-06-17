(function ($) {
    $.fn.maindg = function () {
        return this.each(function () {
            let cols = maindg_columnsSet();
            $(this).datagrid({
                columns: cols,
                rownumbers: true, singleSelect: true, remoteSort: false, autoRowHeight: false,
                onSelect: function (index, row) {
                    if (actionstate == 'add' || actionstate == 'edit' || actionstate == 'stockenter') {
                        if (index != maindgindex) {
                            maindg.datagrid('unselectRow', index);
                            $.messager.alert('alert', '資料尚未儲存,請先存檔!');
                        }
                    } else {
                        maindgindex = index;
                        fieldStateSet('select', row);
                        btnStateSet('select', row);
                    }
                },
                onBeforeSortColumn: function (sort, order) {
                    if (actionstate == 'add' || actionstate == 'edit') {
                        return false;
                    } else {
                        actionstate = 'default';
                        btnStateSet(actionstate, null);
                        fieldStateSet(actionstate, null);
                    }
                },
                rowStyler: maindg_rowStyler
            })
        })
    };
    $.parser.plugins.push('maindg');
})(jQuery);

(function ($) {
    $.fn.itemdg = function () {
        return this.each(function () {
            let cols = itemdg_columnsSet();
            $(this).datagrid({
                columns: cols,
                rownumbers: false, singleSelect: true, nowrap: false, fitColumns: false, remoteSort: false,
                onBeforeEdit: itemdg_onBeforeEdit,
                onAfterEdit: itemdg_onAfterEdit,
                onCancelEdit: itemdg_onCancelEdit,
                onSelect: itemdg_onSelect,
                onBeginEdit: itemdg_onBeginEdit,
                onEndEdit: itemdg_onEndEdit,                
                onLoadSuccess: itemdg_onLoadSuccess,
                onRowContextMenu: itemdg_onRowContextMenu,
                rowStyler: itemdg_rowStyler
            })
        })
    };
    $.parser.plugins.push('itemdg');
})(jQuery);