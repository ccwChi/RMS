(function ($) {
    $.fn.maindg = function () {
        return this.each(function () {
            let cols = maindg_columnsSet();
            $(this).datagrid({
                columns: cols,
                rownumbers: true, singleSelect: true, remoteSort: false,
                onSelect: function (index, row) {
                    if (actionstatus == 'add' || actionstatus == 'edit') {
                        if (index != editindex) {
                            dg.datagrid('unselectRow', index);
                            $.messager.alert('alert', '資料尚未儲存,請先存檔!');
                        }
                    } else {
                        editindex = index;
                        mainrowid = row.Id;
                        actionstatus = 'select';
                        buttonstatus(actionstatus, row);
                        forminputset(actionstatus, row);
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
                nowrap: false, rownumbers: true, singleSelect: true, autoRowHeight: false,
                fitColumns: false, view: detailview,
                groupField: groupfield,
                detailFormatter: function (index, row) {
                    return '<div style="padding:2px"><table class="ddv"></table></div>';
                },
                onExpandRow: function (mainindex, mainrow) {
                    let columndata = subitemdg_columnsSet();
                    mainitemrow = mainrow;
                    let mainitemid = mainrow.DocId;
                    deptno = mainrow.DeptNo;
                    maineditindex = mainindex;
                    var ddv = $(this).datagrid('getRowDetail', mainindex).find('table.ddv');
                    let subgroupfield = subitemdg_groupFieldSet();
                    let data;
                    if (itemlist.length > 0) {
                        data = itemlist.filter(item => { return item.DeptNo === deptno; });
                    }
                    ddv.datagrid({
                        data: data,
                        columns: columndata,
                        singleSelect: true, striped: true, nowrap: false, autoRowHeight: false,
                        loadMsg: '', height: 'auto', foreignField: subgroupfield,
                        onResize: function () {
                            itemdg.datagrid('fixDetailRowHeight', mainindex);
                        },
                        onLoadSuccess: function () {
                            setTimeout(function () {
                                itemdg.datagrid('fixDetailRowHeight', mainindex);
                            }, 0);
                            //插入列
                            iteminsert(ddv, mainrow);
                        },
                        onBeforeEdit: function (index, row) {
                            ddvdg.editing = true;
                            editrow.editing = true;
                            itemstatus = true;
                            checkrow = 1;
                        },
                        onAfterEdit: function (index, row) {
                            if (editrow != null && editrow != undefined) {
                                //check
                                row.CheckRow = editrow.CheckRow;
                                row.RowInsertStatus = editrow.RowInsertStatus;
                                ddv.datagrid('updateRow', {
                                    index: index,
                                    row: {
                                        CheckRow: editrow.CheckRow,
                                        RowInsertStatus: editrow.RowInsertStatus,
                                        RowCheckStatus: editrow.RowCheckStatus,
                                        RowStatus: editrow.RowStatus,
                                        editing: false
                                    }
                                });
                                // update Row
                                let foundindex = itemlist.findIndex(x => x.DocId == row.DocId && x.No == row.No && x.Linage == row.Linage);
                                if (foundindex < 0) {
                                    itemlist.push(row);
                                } else {
                                    itemlist[foundindex] = row;
                                }

                            }
                            ddvdg.editing = false;
                            editrow = null;
                            itemstatus = false;
                            checkrow = 0;
                        },
                        onCancelEdit: function (index, row) {
                            //ddv.datagrid('updateRow', { index: index, row: { editing: false } })
                            ddvdg.editing = false;
                            editrow['RowCheckStatus'] = 1;
                            editrow = null;
                            itemstatus = false;
                            checkrow = 0;
                        },
                        onSelect: function (index, row) {
                            itemstatus = false;
                            if (actionstatus == 'add' || actionstatus == 'edit') {
                                if ((editrow != null && editrow['RowCheckStatus'] == 2) &&
                                    ((maineditindex != undefined && maineditindex == mainindex && itemindex != index && itemindex != undefined) ||
                                        (maineditindex != undefined && maineditindex != mainindex && itemindex == index && itemindex != undefined) ||
                                        (maineditindex != undefined && maineditindex != mainindex && itemindex != index && itemindex != undefined))
                                ) {
                                    if (maineditindex != undefined && maineditindex != mainindex) {
                                        itemdg.datagrid('selectRow', maineditindex);
                                        itemdg.datagrid('fixDetailRowHeight', mainindex);
                                    }
                                    if (itemindex != index && itemindex != undefined) {
                                        ddv.datagrid('unselectRow', index);
                                        if (!msgalert) {
                                            $.messager.alert({
                                                title: 'alert',
                                                msg: '主要欄位不得空白,請先輸入!',
                                                fn: function () { msgalert = false; }
                                            });
                                            msgalert = true;
                                        }
                                        ddv.datagrid('selectRow', itemindex);
                                    }
                                } else {
                                    if ((maineditindex != undefined && maineditindex == mainindex && itemindex != index && itemindex != undefined) ||
                                        (maineditindex != undefined && maineditindex != mainindex && itemindex == index && itemindex != undefined) ||
                                        (maineditindex != undefined && maineditindex != mainindex && itemindex != index && itemindex != undefined)
                                    ) {
                                        if (editrow != null) {
                                            if (itemcheckcolumn()) { ddvdg.datagrid('endEdit', itemindex); }
                                            else { ddvdg.datagrid('cancelEdit', itemindex); }
                                        }
                                    }
                                    if (maineditindex != undefined && maineditindex != mainindex) {
                                        if (ddvdg != undefined && ddvdg != null) {
                                            ddvdg.datagrid('unselectRow', itemindex);
                                        }
                                    }
                                    maineditindex = mainindex;
                                    mainitemrow = itemdg.datagrid('getRows')[maineditindex];
                                    itemindex = index;
                                    ddvdg = ddv;
                                    editrow = row;
                                    ddv.datagrid('beginEdit', index);
                                    itemstatus = true;
                                    //代理人
                                    //let mainrow = dg.datagrid('getRows')[editindex];
                                    //if (mainrow.ProjectContacts == Roles || (agent.filter(item => { return item.No === mainrow.ProjectContacts; })).length > 0) {
                                    //    subitemdg.datagrid('beginEdit', index);
                                    //    subitemstatus = true;
                                    //}
                                }
                            } else {
                                ddv.datagrid('unselectRow', index);
                            }
                        },
                        onBeginEdit: function (index, row) {
                            editrow = row;
                            let ed1 = ddv.datagrid('getEditor', { index: itemindex, field: "DeptId" });
                            $(ed1.target).combobox('loadData', deptno);
                            editrow['DeptId'] = $(ed1.target).combobox('getValue');
                            ////檢查欄位
                            //if (!((row.ItemContent != '' && row.ItemContent != null && row.ItemContent != undefined)
                            //    && (row.ItemContactsId != '' && row.ItemContactsId != null && row.ItemContactsId != undefined)
                            //    && (row.DeptId != '' && row.DeptId != null && row.DeptId != undefined)
                            //    && (row.ItemDate != '' && row.ItemDate != null && row.ItemDate != undefined))) {
                            //    editrow['RowCheckStatus'] = 2;
                            //}
                        },
                        onEndEdit: function (index, row) {
                            checkrow = 2;
                            //設定名稱
                            let userid = ddv.datagrid('getEditor', { index: index, field: 'ItemContactsId' });
                            row.ItemContacts = $(userid.target).combobox('getText');
                            let deptid = ddv.datagrid('getEditor', { index: index, field: 'DeptId' });
                            row.DeptName = $(deptid.target).combobox('getText');
                        },
                        onRowContextMenu: function (e, index, row) {
                            if ((isEmpty(row.SchedulerNum) || row.SchedulerNum <= 0)
                                 && (actionstatus == 'add' || actionstatus == 'edit')) {
                                let rowlen = ddv.datagrid('getRows').length;
                                if (index >= 0 && index != (rowlen - 1)) {
                                    ddv.datagrid('selectRow', index);
                                    e.preventDefault();
                                    $('#mm').menu('show', {
                                        left: e.pageX,
                                        top: e.pageY
                                    });
                                }
                            }
                        },
                        rownumbers: true, striped: true, autoRowHeight: false
                    });
                },
                onSelect: function (index, row) {
                    if (actionstatus != 'add' && actionstatus != 'edit') {
                        itemdg.datagrid('unselectRow', index);
                    }
                },
                rowStyler:itemdg_rowStyle
            })
        })
    };
    $.parser.plugins.push('itemdg');
})(jQuery);