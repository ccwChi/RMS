(function ($) {
    $.fn.maindg = function () {
        return this.each(function () {
            let cols = maindg_columnset();
            $(this).datagrid({
                columns: cols,
                rownumbers: true, singleSelect: true, remoteSort: false, autoRowHeight: false,
                onSelect: function (index, row) {
                    if (actionstatus == 'add' || actionstatus == 'edit') {
                        if (index != editindex) {
                            dg.datagrid('unselectRow', index);
                            $.messager.alert('alert', '資料尚未儲存,請先存檔!');
                        }
                    } else {
                        editindex = index;
                        mainrowid = row.Id;
                        forminputset('select', row);
                        buttonstatus('select', row);
                    }
                    combostatus = false;
                },
                onBeforeSortColumn: function (sort, order) {
                    if (actionstatus == 'add' || actionstatus == 'edit') {
                        return false;
                    } else {
                        actionstatus = 'default';
                        buttonstatus(actionstatus, null);
                        forminputset(actionstatus, null);
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
            let groupfield = itemdg_groupFieldSet();
            $(this).datagrid({
                columns: cols,
                nowrap: false, rownumbers: true, singleSelect: true,
                fitColumns: false, view: groupview,
                groupField: groupfield,
                groupFormatter: function (value, rows) {
                    return '<span class="groupviewheader">【' + value + '】 - 共' + rows.length + '筆</span>';
                },
                onSelect: function (index, row) {
                    if (actionstatus == 'add' || actionstatus == 'edit') {
                        if (index != itemindex) {
                            itemdg.datagrid('unselectRow', index);
                            $.messager.alert('alert', '待辦事項尚未儲存,請先存檔!');
                            itemdg.datagrid('selectRow', itemindex);
                        }
                    } else {
                        //待辦事項
                        itemindex = index;
                        if (mainrowid != '') {
                            subitemdg.datagrid('loadData', []);
                            let sdata = {
                                'MainId': mainrowid,
                                'ItemId': row.Id
                            };
                            deptid = row.DeptId;
                            let url = defaultUrl + controllerurl + functionurl + 'SubItemRead';
                            clickreadsearch(subitemdg, url, sdata, function () {
                                itemrowid = row.Id;
                                subitemindex = undefined;
                                subiteminsert();
                            });
                        }
                    }
                },
            })
        })
    };
    $.parser.plugins.push('itemdg');
})(jQuery);

(function ($) {
    $.fn.subitemdg = function () {
        return this.each(function () {
            let cols = subitemdg_columnsSet();
            $(this).datagrid({
                columns: cols,
                rownumbers: true, singleSelect: true, nowrap: false,
                onBeforeEdit: subitemdg_onBeforeEdit,
                onAfterEdit: subitemdg_onAfterEdit,
                onCancelEdit: subitemdg_onCancelEdit,
                onSelect: subitemdg_onSelect,
                onBeginEdit: subitemdg_onBeginEdit,
                onEndEdit: subitemdg_onEndEdit,
                onRowContextMenu: subitemdg_onRowContextMenu
            })
        })
    };
    $.parser.plugins.push('subitemdg');
})(jQuery);

(function ($) {
    $.fn.dolistdg = function () {
        return this.each(function () {
            let cols = dolistdg_columnsSet();
            $(this).datagrid({
                columns: cols,
                rownumbers: true, singleSelect: true, nowrap: false, remoteSort: false, //fitColumns: false,
                onBeforeEdit: subitemdg_onBeforeEdit,
                onAfterEdit: subitemdg_onAfterEdit,
                onCancelEdit: subitemdg_onCancelEdit,
                onSelect: subitemdg_onSelect,
                onBeginEdit: subitemdg_onBeginEdit,
                onEndEdit: subitemdg_onEndEdit,
                onBeforeSortColumn: dolistitemdg_onBeforeSortColumn
            })
        })
    };
    $.parser.plugins.push('dolistdg');
})(jQuery);