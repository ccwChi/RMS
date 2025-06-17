(function ($) {
    $.fn.maindg = function () {
        return this.each(function () {
            let cols = maindg_columnset();
            let groupfield = itemdg_groupFieldSet();
            $(this).datagrid({
                columns: cols,
                nowrap: false, rownumbers: true, singleSelect: true,
                fitColumns: false, view: groupview,
                groupField: groupfield, 
                groupFormatter: function (value, rows) {
                    return '<span class="groupviewheader" datagrid-group-lentgh="' + rows.length + '">【' + value + '】 - 共' + rows.length + '筆資料</span > ';
                },
                onSelect: function (index, row) {
                    if (actionstatus == 'add' || actionstatus == 'edit') {
                        if (index != editindex) {
                            dg.datagrid('unselectRow', index);
                            $.messager.alert('alert', '資料尚未儲存,請先存檔!');
                        }
                    } else {                        
                        editindex = index;
                        mainrowid = row.Id;
                        deptid = row.DeptId;
                        forminputset('select', row);
                        buttonstatus('select', row);                        
                        //itemdg.datagrid('loadData', []);
                        //itemindex = undefined;                        
                    }                    
                },
                onBeforeEdit: function (index, row) {
                    row.editing = true;
                    dg.datagrid('refreshRow', index);
                },
                onAfterEdit: function (index, row) {
                    row.editing = false;
                    dg.datagrid('refreshRow', index);
                },
                onCancelEdit: function (index, row) {
                    row.editing = false;
                    dg.datagrid('refreshRow', index);
                },
                onDblClickRow: function (index, row) {
                    if (actionstatus == 'add' || actionstatus == 'edit') {
                        if (index != editindex) {
                            dg.datagrid('unselectRow', index);
                            $.messager.alert('alert', '資料尚未儲存,請先存檔!');
                            dg.datagrid('selectRow', editindex);
                        }
                    } else {
                        dg.datagrid('beginEdit', index);
                    }
                }
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
                rownumbers: true, singleSelect: true, nowrap: false, fitColumns: false,
                onBeforeEdit: itemdg_onBeforeEdit,
                onAfterEdit: itemdg_onAfterEdit,
                onCancelEdit: itemdg_onCancelEdit,
                onSelect: itemdg_onSelect,
                onBeginEdit: itemdg_onBeginEdit,
                onEndEdit: itemdg_onEndEdit,
                onRowContextMenu: function (e, index, row) {
                    if (index >= 0) {
                        itemdg.datagrid('selectRow', index);
                        e.preventDefault();
                        $('#mm').menu('show', {
                            left: e.pageX,
                            top: e.pageY
                        });
                    }
                },
            })
        })
    };
    $.parser.plugins.push('itemdg');
})(jQuery);