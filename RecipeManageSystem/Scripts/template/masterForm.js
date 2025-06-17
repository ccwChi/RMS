
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

//#endregion

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

//數字欄位format
function formatNumber(num) {
    return num.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
}

//左側列表狀態顏色
function maindg_rowStyler(index, row) {
    if (row.IsState) {
        return { class: 'gray-row' };
    } else if (row.IsStatus) {
        return { class: 'pick-row' };
    }
    else if (row.IsClosed) {
        return { class: 'close-row' };
    }
    else if (row.IsChecked) {
        return { class: 'ischecked-row' };
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
                    floadStateSet(type, row);
                }
            }            
            break;
        case 'unlock':
            if (maindgindex >= 0) {
                let row = maindg.datagrid('getRows')[maindgindex];
                if (row) {
                    floadStateSet(type, row);
                }
            }
            break;        
        case 'invalid':
            if (maindgindex >= 0) {
                let row = maindg.datagrid('getRows')[maindgindex];
                if (row) {
                    floadStateSet(type, row);
                }
            }
            break;
        case 'uninvalid':
            if (maindgindex >= 0) {
                let row = maindg.datagrid('getRows')[maindgindex];
                if (row) {
                    floadStateSet(type, row);
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