///------內容設定-----------///
//#region 內容設定
//label名稱設定
function labelSetting(field) {
    if (field.length > 0) {
        for (let i = 0; i < field.length; i++) {
            if (field[i].show) {
                $('.field' + field[i].id).html(field[i].text);
            }
        }
    }
}
//表頭欄位設定
function fieldStateSet(acttype, row) {
    //let mainfieldlist = fieldSetting('1');
    switch (acttype) {
        case 'add': {
            if (mainfieldlist.length > 0) {
                for (let i = 0; i < mainfieldlist.length; i++) {
                    if (mainfieldlist[i].show) {
                        let colSetting = {};
                        if (mainfieldlist[i].addable) {
                            colSetting = { 'required': mainfieldlist[i].required, 'readonly': false };
                        } else {
                            colSetting = { 'required': mainfieldlist[i].required, 'readonly': true };
                        }
                        switch (mainfieldlist[i].type) {
                            case 'textbox':
                                $('.col' + mainfieldlist[i].id).textbox(colSetting);
                                $('.col' + mainfieldlist[i].id).textbox('setValue', '');
                                break;
                            case 'combobox':
                                $('.col' + mainfieldlist[i].id).combobox(colSetting);
                                $('.col' + mainfieldlist[i].id).combobox('setValue', '');
                                break;
                            case 'datebox':
                                $('.col' + mainfieldlist[i].id).datebox(colSetting);
                                $('.col' + mainfieldlist[i].id).datebox('setValue', '');
                                break;
                            case 'numberbox':
                                $('.col' + mainfieldlist[i].id).numberbox(colSetting);
                                $('.col' + mainfieldlist[i].id).numberbox('setValue', 0);
                                break;
                            case 'input':
                            case 'datetime':
                                $('.col' + mainfieldlist[i].id).val('');
                                break;
                            case 'label':
                                $('.col' + mainfieldlist[i].id).html('');
                                break;
                            case 'radio':
                            case 'checkbox':
                                $('input[name=' + mainfieldlist[i].id + ']').prop('checked', false);
                                $('input[name=' + mainfieldlist[i].id + ']').removeAttr('disabled');
                                break;
                            default:
                        }
                    }
                }
                loadStateSet(acttype, row);
            }
            break;
        }
        case 'edit': {
            if (mainfieldlist.length > 0 && !isEmpty(row)) {
                for (let i = 0; i < mainfieldlist.length; i++) {
                    if (mainfieldlist[i].show) {
                        let colSetting = {};
                        if (mainfieldlist[i].editable) {
                            colSetting = { 'required': mainfieldlist[i].required, 'readonly': false };
                        } else {
                            colSetting = { 'required': mainfieldlist[i].required, 'readonly': true };
                        }
                        switch (mainfieldlist[i].type) {
                            case 'textbox':
                                $('.col' + mainfieldlist[i].id).textbox(colSetting);
                                $('.col' + mainfieldlist[i].id).textbox('setValue', row[mainfieldlist[i].id]);
                                break;
                            case 'combobox':
                                $('.col' + mainfieldlist[i].id).combobox(colSetting);
                                $('.col' + mainfieldlist[i].id).combobox('setValue', row[mainfieldlist[i].id]);
                                break;
                            case 'datebox':
                                $('.col' + mainfieldlist[i].id).datebox(colSetting);
                                $('.col' + mainfieldlist[i].id).datebox('setValue', row[mainfieldlist[i].id]);
                                break;
                            case 'numberbox':
                                $('.col' + mainfieldlist[i].id).numberbox(colSetting);
                                $('.col' + mainfieldlist[i].id).numberbox('setValue', ow[mainfieldlist[i].id]);
                                break;
                            case 'input':
                                $('.col' + mainfieldlist[i].id).val(row[mainfieldlist[i].id]);
                                break;
                            case 'datetime':
                                let rowdate = row[mainfieldlist[i].id] == undefined ? '' : moment(row[mainfieldlist[i].id]).format('YYYY-MM-DD HH:mm:ss');
                                $('.col' + mainfieldlist[i].id).val(rowdate);
                                break;
                                break;
                            case 'label':
                                $('.col' + mainfieldlist[i].id).html(row[mainfieldlist[i].id]);
                                break;
                            default:
                        }
                    }
                }
                loadStateSet(acttype, row);
            }
            break;
        }
        case 'save':
        case 'cancel': {
            if (mainfieldlist.length > 0) {
                for (let i = 0; i < mainfieldlist.length; i++) {
                    if (mainfieldlist[i].show) {
                        let colSetting = { 'required': false, 'readonly': true };
                        switch (mainfieldlist[i].type) {
                            case 'textbox':
                                $('.col' + mainfieldlist[i].id).textbox(colSetting);
                                break;
                            case 'combobox':
                                $('.col' + mainfieldlist[i].id).combobox(colSetting);
                                break;
                            case 'datebox':
                                $('.col' + mainfieldlist[i].id).datebox(colSetting);
                                break;
                            case 'numberbox':
                                $('.col' + mainfieldlist[i].id).numberbox(colSetting);
                                break;
                            case 'radio':
                            case 'checkbox':
                                $('input[name=' + mainfieldlist[i].id + ']').attr('disabled', true);
                                break;
                            case 'input':
                            case 'datetime':
                                break;
                            case 'label':
                            default:
                        }
                    }
                }
            }
            if (!isEmpty(maindgindex) && maindgindex >= 0) {
                maindg.datagrid('selectRow', maindgindex);
            } else {
                for (let i = 0; i < mainfieldlist.length; i++) {
                    if (mainfieldlist[i].show) {
                        switch (mainfieldlist[i].type) {
                            case 'textbox':
                                $('.col' + mainfieldlist[i].id).textbox('setValue', '');
                                break;
                            case 'combobox':
                                $('.col' + mainfieldlist[i].id).combobox('setValue', '');
                                break;
                            case 'datebox':
                                $('.col' + mainfieldlist[i].id).datebox('setValue', '');
                                break;
                            case 'numberbox':
                                $('.col' + mainfieldlist[i].id).numberbox('setValue', 0);
                                break;
                            case 'input':
                            case 'datetime':
                                $('.col' + mainfieldlist[i].id).val('');
                                break;
                            case 'radio':
                            case 'checkbox':
                                $('input[name=' + mainfieldlist[i].id + ']').prop('checked', false);
                                break;
                            case 'label':
                            default:
                        }
                    }
                }
                loadStateSet(acttype, row);
            }
            break;
        }
        case 'select': {
            if (mainfieldlist.length > 0 && !isEmpty(row)) {
                $.messager.progress();
                for (let i = 0; i < mainfieldlist.length; i++) {
                    if (mainfieldlist[i].show) {
                        switch (mainfieldlist[i].type) {
                            case 'textbox':
                                $('.col' + mainfieldlist[i].id).textbox('setValue', row[mainfieldlist[i].id]);
                                break;
                            case 'combobox':
                                $('.col' + mainfieldlist[i].id).combobox('setValue', row[mainfieldlist[i].id]);
                                break;
                            case 'datebox':
                                $('.col' + mainfieldlist[i].id).datebox('setValue', row[mainfieldlist[i].id]);
                                break;
                            case 'input':
                                $('.col' + mainfieldlist[i].id).val(row[mainfieldlist[i].id]);
                                break;
                            case 'datetime':
                                let rowdate = row[mainfieldlist[i].id] == undefined ? '' : moment(row[mainfieldlist[i].id]).format('YYYY-MM-DD HH:mm:ss');
                                $('.col' + mainfieldlist[i].id).val(rowdate);
                                break;
                            case 'numberbox':
                                $('.col' + mainfieldlist[i].id).numberbox('setValue', row[mainfieldlist[i].id]);
                                break;
                            case 'radio':
                            case 'checkbox':
                                $('input[name=' + mainfieldlist[i].id + ']').prop('checked', false);
                                $('#col' + mainfieldlist[i].id + row[mainfieldlist[i].id]).prop('checked', true);
                                break;
                            case 'label':
                                let v;
                                if (row[mainfieldlist[i].id] > 1000) {
                                    v = formatNumber(row[mainfieldlist[i].id]);
                                } else {
                                    v = row[mainfieldlist[i].id];
                                }
                                if (mainfieldlist[i].id == 'Num1') {
                                    $('.col' + mainfieldlist[i].id).html(v);
                                } else {
                                    $('.col' + mainfieldlist[i].id).html('$' + v);
                                }
                                break;
                            default:
                        }
                    }
                }
                loadStateSet(acttype, row);

            }
            break;
        }
        case 'search': {
            break;
        }
        case 'default':
            if (mainfieldlist.length > 0) {
                for (let i = 0; i < mainfieldlist.length; i++) {
                    if (mainfieldlist[i].show) {
                        let colSetting = { 'required': false, 'readonly': true };
                        switch (mainfieldlist[i].type) {
                            case 'textbox':
                                $('.col' + mainfieldlist[i].id).textbox(colSetting);
                                $('.col' + mainfieldlist[i].id).textbox('setValue', '');
                                break;
                            case 'combobox':
                                $('.col' + mainfieldlist[i].id).combobox(colSetting);
                                $('.col' + mainfieldlist[i].id).combobox('setValue', '');
                                $('.col' + mainfieldlist[i].id).combobox({
                                    keyHandler: $.extend({}, $.fn.combobox.defaults.keyHandler, {
                                        down: function (e) {
                                            $(this).combobox('showPanel');
                                            $.fn.combobox.defaults.keyHandler.down.call(this, e);
                                        }
                                    })
                                });
                                break;
                            case 'datebox':
                                $('.col' + mainfieldlist[i].id).datebox(colSetting);
                                $('.col' + mainfieldlist[i].id).datebox('setValue', '');
                                $('.col' + mainfieldlist[i].id).datebox({
                                    inputEvents: $.extend({}, $.fn.combobox.defaults.inputEvents, {
                                        blur: function () {
                                            var event = new $.Event('keydown');
                                            event.keyCode = 13;
                                            $(this).trigger(event);
                                        }
                                    })
                                });
                                break;
                            case 'numberbox':
                                $('.col' + mainfieldlist[i].id).numberbox(colSetting);
                                $('.col' + mainfieldlist[i].id).numberbox('setValue', '');
                                break;
                            case 'input':
                            case 'datetime':
                                $('.col' + mainfieldlist[i].id).val('');
                                break;
                            case 'label':
                                $('.col' + mainfieldlist[i].id).html('');
                                break;
                            case 'radio':
                            case 'checkbox':
                                $('input[name=' + mainfieldlist[i].id + ']').attr('disabled', true);
                                break;
                            default:
                        }
                    }
                }
            }
        default:

    }
}
//按鈕事件
function btnClick(type) {
    switch (type) {
        //#region 方向
        case 'first': {
            maindg.datagrid('selectRow', 0);
            break;
        }
        case 'previous': {
            let nowindex;
            let row = maindg.datagrid('getSelected');
            if (row) {
                nowindex = maindg.datagrid('getRowIndex', row);
            } else if (maindgindex >= 0) {
                nowindex = maindgindex;
            } else {
                let rows = maindg.datagrid('getRows');
                if (rows) {
                    nowindex = 0;
                }
            }
            maindg.datagrid('selectRow', nowindex - 1);
            break;
        }
        case 'next': {
            let nowindex;
            let row = maindg.datagrid('getSelected');
            if (row) {
                nowindex = maindg.datagrid('getRowIndex', row);
            } else if (maindgindex >= 0) {
                nowindex = maindgindex;
            } else {
                let rows = maindg.datagrid('getRows');
                if (rows) {
                    nowindex = -1;
                }
            }
            maindg.datagrid('selectRow', nowindex + 1);
            break;
        }
        case 'last': {
            let index;
            let row = maindg.datagrid('getRows');
            if (row) {
                index = row.length;
            }
            maindg.datagrid('selectRow', index - 1);
            break;
        }
        //#endregion
        //#region other
        case 'search':
            $('#dlgsearch').dialog('open').dialog('center').dialog('setTitle', '查詢視窗');
            break;
        case 'refresh':
            $.messager.confirm('重新整理', '請確定是否要重新整理?', function (r) {
                if (r) {
                    window.location.reload();
                }
            });
            break;            
        //#endregion
        //#region CRUD
        case 'add':
            fieldStateSet(type, null);
            btnStateSet(type);
            break;
        case 'edit':
            if (maindgindex >= 0) {
                let row = maindg.datagrid('getRows')[maindgindex];
                if (row) {
                    fieldStateSet(type, row);
                    btnStateSet(type, row);
                }
            }
            break;
        case 'save':
            if (actionstate == 'add' || actionstate == 'edit') {
                saveConfirm(actionstate);
                type = actionstate;
            }
            break;
        case 'cancel':
            $.messager.confirm('取消', '請確定是否要取消該筆資料??', function (r) {
                if (r) {
                    btnStateSet(type);
                    fieldStateSet(type);
                }
            });
            break;
        case 'delete':
            if (maindgindex >= 0) {
                let row = maindg.datagrid('getRows')[maindgindex];
                if (row) {
                    $.messager.confirm('刪除', '請確定要刪除該筆資料?', function (r) {
                        if (r) {
                            let row = maindg.datagrid('getRows')[maindgindex];
                            let url = defaultUrl + controllerurl + functionurl + 'Delete';
                            datapost(row, url).done(function (msg) {
                                maindg.datagrid('deleteRow', maindgindex);
                                itemdg.datagrid('loadData', []);
                                $.messager.alert('alert', '資料已刪除!');
                            }).fail(function () {
                                $.messager.alert('alert', '刪除資料失敗!');
                            })
                        }
                    });
                }
            }
            break;
        //#endregion
        //#region BillState
        case 'lock':
            if (maindgindex >= 0) {
                let row = maindg.datagrid('getRows')[maindgindex];
                if (row) {
                   loadStateSet(type, row);
                }
            }
            break;
        case 'unlock':
            if (maindgindex >= 0) {
                let row = maindg.datagrid('getRows')[maindgindex];
                if (row) {
                    loadStateSet(type, row);
                }
            }
            break;       
        case 'invalid':
            if (maindgindex >= 0) {
                let row = maindg.datagrid('getRows')[maindgindex];
                if (row) {
                    loadStateSet(type, row);
                }
            }
            break;
        case 'uninvalid':
            if (maindgindex >= 0) {
                let row = maindg.datagrid('getRows')[maindgindex];
                if (row) {
                    loadStateSet(type, row);
                }
            }
            break;       
        //#endregion
        case 'read':
            actionurl = defaultUrl + controllerurl + functionurl + 'Read';
            readData(maindg, actionurl);
            break;
        case 'default':

        default:
    }
    actionstate = type;
    //successCallBack(result);
}
//#endregion

//#region EventSetting
//預設資料讀取
function readData(target, url) {
    $.messager.progress();
    dataget(url).done(function (msg) {
        target.datagrid({
            data: msg,
        });
        $.messager.progress('close');
    }).fail(function () {
        $.messager.progress('close');
        $.messager.alert('alert', '資料讀取失敗!');
    });
}
//搜尋資料讀取
function searchData(target, url, rowdata, successCallback) {
    $.messager.progress();
    datapost(rowdata, url).done(function (msg) {
        maindg.datagrid({
            data: msg,
        });
        $.messager.progress('close');
        successCallback();
    }).fail(function () {
        $.messager.progress('close');
        $.messager.alert('alert', '資料讀取失敗!');
    });
}
//搜尋欄位設定
function searchConfirm() {
    searchlist = {        
        'SearchConditions1': 0,
        'SearchCol1': 0,
        'SearchEqual1': 0,
        'SearchData1': '',
        'SearchConditions2': 0,
        'SearchCol2': 0,
        'SearchEqual2': 0,
        'SearchData2': '',
        'SearchConditions3': 0,
        'SearchCol3': 0,
        'SearchEqual3': 0,
        'SearchData3': ''
    };    
    //Search
    if ($('.searchdata1').val() != '' && $('.searchconditions1').val() > 0 && $('.searchcolumns1').val() > 0 && $('.searchequal1').val() > 0) {
        searchlist.SearchConditions1 = $('.searchconditions1').val();
        searchlist.SearchCol1 = $('.searchcolumns1').val();
        searchlist.SearchEqual1 = $('.searchequal1').val();
        searchlist.SearchData1 = $('.searchdata1').val();
    }
    if ($('.searchdata2').val() != '' && $('.searchconditions2').val() > 0 && $('.searchcolumns2').val() > 0 && $('.searchequal2').val() > 0) {
        searchlist.SearchConditions2 = $('.searchconditions2').val();
        searchlist.SearchCol2 = $('.searchcolumns2').val();
        searchlist.SearchEqual2 = $('.searchequal2').val();
        searchlist.SearchData2 = $('.searchdata2').val();
    }
    if ($('.searchdata3').val() != '' && $('.searchconditions3').val() > 0 && $('.searchcolumns3').val() > 0 && $('.searchequal3').val() > 0) {
        searchlist.SearchConditions3 = $('.searchconditions3').val();
        searchlist.SearchCol3 = $('.searchcolumns3').val();
        searchlist.SearchEqual3 = $('.searchequal3').val();
        searchlist.SearchData3 = $('.searchdata3').val();
    }
    actionurl = defaultUrl + controllerurl + functionurl + 'Read';
    searchData(maindg, actionurl, searchlist, function () { });
    $('#dlgsearch').dialog('close');
}
//更新資料
function updatesuccess(type, actionurl, typename, row) {
    $.messager.progress();
    datapost(row, actionurl).done(function (msg) {
        let url = defaultUrl + controllerurl + functionurl + 'Read';
        dataget(url).done(function (msg) {
            maindg.datagrid({
                data: msg,
            });
            actionstate = 'select';
            maindg.datagrid('selectRow', maindgindex);
            btnStateSet(actionstate, maindg.datagrid('getRows')[maindgindex]);

            $.messager.progress('close');
            $.messager.alert('訊息', typename + '完成!');
            maindg.focus();
        }).fail(function () {
            $.messager.progress('close');
            $.messager.alert('alert', '資料讀取失敗!');
        });
    }).fail(function () {
        $.messager.progress('close');
        $.messager.alert('訊息', typename + '資料失敗!');
    })
}
//#endregion


//#region 左側列表設定
function maindg_columnsSet() {
    let col = fieldSetting('1');
    let columns = [];
    if (col.length > 0) {
        for (let i = 0; i < col.length; i++) {
            if (col[i].showdg) {
                let newCol = { field: col[i].id, title: col[i].text, width: col[i].width, align: col[i].align, };
                columns.push(newCol);
            }
        }
    }
    let itemdgCol = [];
    itemdgCol.push(columns);
    return itemdgCol;
}
function maindg_rowStyler(index, row) {
    if (row.IsClosed == true) {
        return { class: 'close-row' };
    }
    else if (row.IsState == true) {
        return { class: 'isstate-row' };
    }
    else if (row.IsStatus == true) {
        return { class: 'start-row' };
    }
    else if (row.IsChecked == true) {
        return { class: 'ischecked-row' };
    }
}
//#endregion

//#region dg設定
function itemdg_onBeforeEdit(index, row) {
    row.editing = true;
    itemstatus = true;
    checkrow = 1;
}
function itemdg_onAfterEdit(index, row) {
    if (editrow != null && editrow != undefined) {
        row.CheckRow = editrow.CheckRow;
        row.RowInsertStatus = editrow.RowInsertStatus;
        itemdg.datagrid('updateRow', {
            index: index,
            row: {
                CheckRow: editrow.CheckRow,
                RowInsertStatus: editrow.RowInsertStatus,
                RowCheckStatus: editrow.RowCheckStatus,
                RowStatus: editrow.RowStatus,
                editing: false,
                Linage: index + 1
            }
        });
    }
    editrow = null;
    row.editing = false;
    itemstatus = false;
    checkrow = 0;
}
function itemdg_onCancelEdit(index, row) {
    row.editing = false;
    editrow = null;
    itemstatus = false;
    checkrow = 0;
}
function itemdg_onSelect(index, row) {
    itemstatus = false;
    if (actionstate == 'add' || actionstate == 'edit') {
        if (editrow != null && editrow['RowCheckStatus'] == 2 && !isEmpty(itemdgindex) && itemdgindex != index) {
            itemdg.datagrid('unselectRow', index);
            $.messager.alert('alert', '主要欄位不得空白,請先輸入!');
            itemdg.datagrid('selectRow', itemdgindex);
        } else {
            if (!isEmpty(itemdgindex) && itemdgindex != index) {
                itemdg.datagrid('endEdit', itemdgindex);
                let check = itemdg.datagrid('getRows')[itemdgindex];
                if (check != undefined && check != null && check.CheckRow == 0) {
                    itemdg.datagrid('cancelEdit', itemdgindex);
                }
            }
            itemdgindex = index;
            editrow = row;
            itemdg.datagrid('beginEdit', index);
            itemstatus = true;
        }
    } else {
        itemdg.datagrid('unselectRow', index);
    }
}
function itemdg_onBeginEdit(index, row) {
    editrow = row;

}
function itemdg_onEndEdit(index, row) {
    checkrow = 2;
}
function itemdg_onLoadSuccess() {

}
function itemdg_onRowContextMenu(e, index, row) {
    if ((actionstate == 'add' || actionstate == 'edit')) {
        let rowlen = itemdg.datagrid('getRows').length;
        if (index >= 0 && index != (rowlen)) {
            itemdg.datagrid('selectRow', index);
            e.preventDefault();
            $('#mm').menu('show', {
                left: e.pageX,
                top: e.pageY
            });
        }
    }
}
function itemdg_rowStyler(index, row) {
    if (row.Num == 1) {
        return { class: 'start-row' };
    }
}
//#endregion

//#region other
function closeEdit() {
    if (itemdgindex != undefined && itemdgindex >= 0) {
        let itemeditrow = itemdg.datagrid('getRows')[itemdgindex];
        if (!isEmpty(itemeditrow) && itemeditrow.editing) {
            itemdg.datagrid('endEdit', itemdgindex);
        }
    }
}
//#endregion

// #region Hide
function itemdg_columnsSet() {
    let col = fieldSetting('2');
    let columns = [];
    if (col.length > 0) {
        for (let i = 0; i < col.length; i++) {
            if (col[i].show) {
                let newCol = { field: col[i].id, title: col[i].text, width: col[i].width, align: col[i].align, editor: col[i].editor, formatter: col[i].formatter };
                columns.push(newCol);
            }
        }
    }
    let itemdgCol = [];
    itemdgCol.push(columns);
    return itemdgCol;
}

//欄位改變事件
function itemcolumnchange(field, newValue) {
    if (editrow != null) {
        //edit
        if (editrow['RowStatus'] == 0) { editrow['RowStatus'] = 2; }
        editrow[field] = newValue;
        //資料改變時
        editrow['RowCheckStatus'] = 2;
        //檢查
        if (itemcheckcolumn()) {
            let rows = itemdg.datagrid('getRows');
            if (rows[rows.length - 1].RowInsertStatus == 0 && itemdgindex == (rows.length - 1)) {
                //插入列
                iteminsert(itemdg);
                editrow['RowInsertStatus'] = 1;
            }
        }
    }
}

//檢查欄位
function checkSaveData() {
    let chkResult = true;
    if (mainfieldlist.length > 0) {
        for (let i = 0; i < mainfieldlist.length; i++) {
            if (mainfieldlist[i].required) {
                switch (mainfieldlist[i].type) {
                    case 'textbox':
                        if ($('.col' + mainfieldlist[i].id).textbox('getValue') == "") {
                            $.messager.alert('alert', '請選擇' + mainfieldlist[i].text + '!');
                            $('.col' + mainfieldlist[i].id).textbox('textbox').focus();
                            chkResult = false;
                        }
                        break;
                    case 'combobox':
                        if ($('.col' + mainfieldlist[i].id).combobox('getValue') == "") {
                            $.messager.alert('alert', '請選擇' + mainfieldlist[i].text + '!');
                            $('.col' + mainfieldlist[i].id).combobox('textbox').focus();
                            chkResult = false;
                        }
                        break;
                    case 'datebox':
                        if ($('.col' + mainfieldlist[i].id).datebox('getValue') == "") {
                            $.messager.alert('alert', '請選擇' + mainfieldlist[i].text + '!');
                            $('.col' + mainfieldlist[i].id).datebox('textbox').focus();
                            chkResult = false;
                        }
                        break;
                    case 'numberbox':
                        if ($('.col' + mainfieldlist[i].id).numberbox('getValue') == "") {
                            $.messager.alert('alert', '請選擇' + mainfieldlist[i].text + '!');
                            $('.col' + mainfieldlist[i].id).numberbox('textbox').focus();
                            chkResult = false;
                        }
                        break;
                    case 'input':
                        if ($('.col' + mainfieldlist[i].id).val() == "") {
                            $.messager.alert('alert', '請選擇' + mainfieldlist[i].text + '!');
                            $('.col' + mainfieldlist[i].id).focus();
                            chkResult = false;
                        }
                        break;
                    default:
                }
            }
        }
    }
    return chkResult;
}
//檢查欄位
function itemcheckcolumn() {
    let result = true;
    let col = fieldSetting('2');
    if (col.length > 0) {
        for (let i = 0; i < col.length; i++) {
            if (col[i].required) {
                if (isEmpty(editrow[col[i].id]) || editrow[col[i].id] == '') {
                    result = false;
                }
            }
        }
    }

    if (result == true) {
        editrow['CheckRow'] = 1;
        editrow['RowCheckStatus'] = 3;
    } else {
        editrow['CheckRow'] = 0;
    }
    return result;
}

//datagrid鍵盤控制設定

function maindg_keyEventSetting() {
    maindg.datagrid('getPanel').panel('panel').attr('tabindex', 1).bind('keydown', function (e) {
        let index = 0;
        switch (e.keyCode) {
            case 38:	// up
                var selected = maindg.datagrid('getSelected');
                index = 0;
                if (selected) {
                    index = maindg.datagrid('getRowIndex', selected);
                    maindg.datagrid('selectRow', index - 1);
                } else {
                    maindg.datagrid('selectRow', index);
                }
                break;
            case 40:	// down
                var selected = maindg.datagrid('getSelected');
                index = 0;
                if (selected) {
                    index = maindg.datagrid('getRowIndex', selected);
                    maindg.datagrid('selectRow', index + 1);
                } else {
                    maindg.datagrid('selectRow', index);
                }
                break;
        }
    });
}
function dg_keyEventSetting(actiondg) {
    var index = 0;
    var selected = null;
    var editors = null;
    var keys = {};
    let custonfirstfield;
    let firstfield;
    let upfield;
    let lastfield;
    if (itemfieldlist.length > 0) {
        let colSeq = 0;
        for (let i = 0; i < itemfieldlist.length; i++) {
            if (itemfieldlist[i].editor != null) {
                if (colSeq == 0) {
                    custonfirstfield = itemfieldlist[i].id;
                    firstfield = itemfieldlist[i].id;
                    upfield = itemfieldlist[i].id;
                    lastfield = itemfieldlist[i].id;
                } else if (i == itemfieldlist.length) {
                    upfield = itemfieldlist[i].id;
                    lastfield = itemfieldlist[i].id;
                }
            }
        }
    }

    actiondg.datagrid('getPanel').panel('panel').attr('tabindex', 1).bind('keydown', function (e) {
        keys[e.which] = true;
        selected = actiondg.datagrid('getSelected');
        if (selected) {
            index = actiondg.datagrid('getRowIndex', selected);
        }
        editors = actiondg.datagrid('getEditors', index);

        //KeyBoard Event----------------------------------------------------------------------------------------
        //Up
        if (e.which === 38 && e.shiftKey) {
            if (selected) {
                //actiondg.datagrid('selectRow', index - 1);
                let prevdg = actiondg.datagrid('getRows')[index - 1];
                if (!isEmpty(prevdg)) {
                    actiondg.datagrid('selectRow', index - 1);
                    let ed4 = actiondg.datagrid('getEditor', { index: index - 1, field: [upfield] });
                    if (!isEmpty(ed4)) {
                        switch (ed4.type) {
                            case 'numberbox':
                                $(ed4.target).numberbox('textbox').focus();
                                break;
                            case 'textbox':
                                $(ed4.target).textbox('textbox').focus();
                                break;
                            case 'combobox':
                                $(ed4.target).combobox('textbox').focus();
                                break;
                        }
                    }
                } else {
                    let ed3 = actiondg.datagrid('getEditor', { index: index, field: [upfield] });
                    $(ed3.target).combobox('textbox').focus();
                }
            } else {
                actiondg.datagrid('selectRow', index);
            }
        }//down                
        else if (e.which === 13) {
            if (selected) {
                let edend = actiondg.datagrid('getRows')[index + 1];

                if (!isEmpty(edend)) {
                    actiondg.datagrid('selectRow', index + 1);
                    let ed5 = actiondg.datagrid('getEditor', { index: index + 1, field: [upfield] });
                    if (!isEmpty(ed5)) {
                        switch (ed5.type) {
                            case 'numberbox':
                                $(ed5.target).numberbox('textbox').focus();
                                break;
                            case 'textbox':
                                $(ed5.target).textbox('textbox').focus();
                                break;
                            case 'combobox':
                                $(ed5.target).combobox('textbox').focus();
                                break;
                        }
                    }
                } else {
                    let ed6 = actiondg.datagrid('getEditor', { index: index, field: [upfield] });
                    $(ed6.target).combobox('textbox').focus();
                }
            } else {
                actiondg.datagrid('selectRow', index);
            }
        }
        //KeyBoard Event----------------------------------------------------------------------------------------
    });
    actiondg.datagrid('getPanel').panel('panel').attr('tabindex', 1).bind('keyup', function (e) {
        delete keys[e.which];
    });
}
//刪除一筆
function deletesubitem() {
    let row = itemdg.datagrid('getRows')[itemdgindex];
    if (row.No != '新增') {
        row.RowStatus = 3;
        itemdgdelrow.push(row);
    }
    itemdg.datagrid('deleteRow', itemdgindex);
    let chkrownum = itemdg.datagrid('getRows').length;
    let chkrow = itemdg.datagrid('getRows')[chkrownum - 1];
    if (chkrow.CheckRow != 0 && chkrow.RowInsertStatus == 0) {
        //插入列
        iteminsert(itemdg);
    }
}
//插入列
function iteminsert(data) {
    //插入一行
    let rowcount = data.datagrid('getRows').length;
    let col = fieldSetting('2');
    let columns = {};
    if (col.length > 0) {
        for (let i = 0; i < col.length; i++) {
            if (col[i].entity) {
                columns[col[i].id] = null;                
            }
        }        
        columns['CheckRow'] = 0;        
        columns['RowInsertStatus'] = 0;        
        columns['RowStatus'] = 1;        
        columns['RowCheckStatus'] = 1; 
    }
    data.datagrid('insertRow', {
        index: rowcount,
        row: columns
    });
}

//數字欄位format
function formatNumber(num) {
    return num.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
}

 // #endregion