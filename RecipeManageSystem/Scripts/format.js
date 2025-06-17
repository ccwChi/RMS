@{
    ViewBag.Title = "配方建立／修改";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

@section css{
    <link rel="stylesheet" href="@Url.Content("~/Scripts/base / easyui / themes / bootstrap / easyui.css")" />
        <link rel="stylesheet" href="@Url.Content("~/Scripts/base / easyui / themes / icon.css")" />
}

<h2>配方建立／修改</h2>

<label>選擇機台：</label>
<input id="recipeMachine" class="easyui-combobox" style="width:200px" />

<table id="paramGrid" class="easyui-datagrid" style="width:100%;height:350px">
        <thead>
            <tr>
                <th data-options="field:'ParamName',title:'參數',width:250">參數</th>
                <th data-options="field:'ParamValue',title:'數值',width:100,editor:{type:'textbox'}">數值</th>
            </tr>
        </thead>
    </table>
@* <table id="paramPg" class="easyui-propertygrid" style="width:100%;height:350px"></table> * @

    @section Scripts {
    <script src="~/Scripts/base/easyui/jquery.easyui.min.js"></script>
    <script>
  $(function(){
    // 1. 拿機台清單
    $.get('@Url.Action("GetMachineList","MachineManage")', res => {
      if (!res.success) {
        return $.messager.alert('錯誤', '無法取得機台清單');
      }

      // 2. 初始化 Combobox
      $('#recipeMachine').combobox({
        data: res.data,             // 確定是 [{ DeviceId, DeviceName }, ...]
        valueField: 'DeviceID',     // 值用 DeviceId
        textField: 'DeviceName',    // 顯示用 DeviceName
        panelHeight: 'auto',
        filter: function(q, row) {
          // 修正 filter，用 DeviceName
          return row.DeviceName.indexOf(q) >= 0;
        },
          onSelect(rec) {
            console.log("rec",rec)
            $.get('@Url.Action("GetParamDetailByDevice","RecipeManage")', { deviceId: rec.DeviceID }, res => {
                console.log("GetParamDetailByDevice res",res)
                const rows = res.data.Params.map(d => ({
                    name: d.Unit ? `${d.ParamName} (${d.Unit})` : d.ParamName,
                    value: '',         // 使用者要填的值
                    paramId: d.ParamId   // 自訂屬性，用於送到後端
                }));

                // 重新初始化 propertygrid，並指定 columns 的 editor
                $('#paramPg').propertygrid({
                    showGroup: false,
                    scrollbarSize: 0,
                    // **這裡要定義哪個欄位可以編輯**
                    columns: [[
                        { field: 'name', title: '參數', width: 250 },
                        {
                            field: 'value', title: '數值', width: 100,
                            editor: { type: 'textbox' }  // <- 一定要在這裡告訴它可以編輯
                        }
                    ]],
                    data: rows
                });
            });
        }
      });
    });

    // 3. 初始化參數表格（不動）
    $('#paramGrid').datagrid({
      singleSelect: true,
      fitColumns:   true,
      nowrap:       true,
      autoRowHeight:true,
      columns: [[
        { field: 'ParamName',  title: '參數', width: 250 },
        { field: 'ParamValue', title: '數值', width: 100, editor:{ type:'textbox' } }
      ]],
      onClickCell(index, field) {
        if (lastEditIndex !== null) {
          $(this).datagrid('endEdit', lastEditIndex);
        }
        $(this).datagrid('beginEdit', index);
        lastEditIndex = index;
      }
    });
  });

  // 4. 當選擇機台時呼叫
  function loadParamGrid(deviceId) {
    console.log('--- loadParamGrid:', deviceId);
    if (lastEditIndex !== null) {
      $('#paramGrid').datagrid('endEdit', lastEditIndex);
      lastEditIndex = null;
    }
    $.get('@Url.Action("GetParamsByMachine","MachineParam")', { deviceId }, function(res) {
      if (!res.success) {
        return $.messager.alert('錯誤', res.message || '無法讀取參數');
      }
      console.log('參數定義回來:', res.data);
      const rows = res.data.map(d => ({
        ParamId:    d.Id,
        ParamName:  d.Unit ? `${d.ParamName} (${d.Unit})` : d.ParamName,
        ParamValue: ''
      }));
      $('#paramGrid').datagrid('loadData', rows);
    });
  }
    </script>

}