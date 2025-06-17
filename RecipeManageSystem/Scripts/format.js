@{
    ViewBag.Title = "�t��إߡ��ק�";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

@section css{
    <link rel="stylesheet" href="@Url.Content("~/Scripts/base / easyui / themes / bootstrap / easyui.css")" />
        <link rel="stylesheet" href="@Url.Content("~/Scripts/base / easyui / themes / icon.css")" />
}

<h2>�t��إߡ��ק�</h2>

<label>��ܾ��x�G</label>
<input id="recipeMachine" class="easyui-combobox" style="width:200px" />

<table id="paramGrid" class="easyui-datagrid" style="width:100%;height:350px">
        <thead>
            <tr>
                <th data-options="field:'ParamName',title:'�Ѽ�',width:250">�Ѽ�</th>
                <th data-options="field:'ParamValue',title:'�ƭ�',width:100,editor:{type:'textbox'}">�ƭ�</th>
            </tr>
        </thead>
    </table>
@* <table id="paramPg" class="easyui-propertygrid" style="width:100%;height:350px"></table> * @

    @section Scripts {
    <script src="~/Scripts/base/easyui/jquery.easyui.min.js"></script>
    <script>
  $(function(){
    // 1. �����x�M��
    $.get('@Url.Action("GetMachineList","MachineManage")', res => {
      if (!res.success) {
        return $.messager.alert('���~', '�L�k���o���x�M��');
      }

      // 2. ��l�� Combobox
      $('#recipeMachine').combobox({
        data: res.data,             // �T�w�O [{ DeviceId, DeviceName }, ...]
        valueField: 'DeviceID',     // �ȥ� DeviceId
        textField: 'DeviceName',    // ��ܥ� DeviceName
        panelHeight: 'auto',
        filter: function(q, row) {
          // �ץ� filter�A�� DeviceName
          return row.DeviceName.indexOf(q) >= 0;
        },
          onSelect(rec) {
            console.log("rec",rec)
            $.get('@Url.Action("GetParamDetailByDevice","RecipeManage")', { deviceId: rec.DeviceID }, res => {
                console.log("GetParamDetailByDevice res",res)
                const rows = res.data.Params.map(d => ({
                    name: d.Unit ? `${d.ParamName} (${d.Unit})` : d.ParamName,
                    value: '',         // �ϥΪ̭n�񪺭�
                    paramId: d.ParamId   // �ۭq�ݩʡA�Ω�e����
                }));

                // ���s��l�� propertygrid�A�ë��w columns �� editor
                $('#paramPg').propertygrid({
                    showGroup: false,
                    scrollbarSize: 0,
                    // **�o�̭n�w�q�������i�H�s��**
                    columns: [[
                        { field: 'name', title: '�Ѽ�', width: 250 },
                        {
                            field: 'value', title: '�ƭ�', width: 100,
                            editor: { type: 'textbox' }  // <- �@�w�n�b�o�̧i�D���i�H�s��
                        }
                    ]],
                    data: rows
                });
            });
        }
      });
    });

    // 3. ��l�ưѼƪ��]���ʡ^
    $('#paramGrid').datagrid({
      singleSelect: true,
      fitColumns:   true,
      nowrap:       true,
      autoRowHeight:true,
      columns: [[
        { field: 'ParamName',  title: '�Ѽ�', width: 250 },
        { field: 'ParamValue', title: '�ƭ�', width: 100, editor:{ type:'textbox' } }
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

  // 4. ���ܾ��x�ɩI�s
  function loadParamGrid(deviceId) {
    console.log('--- loadParamGrid:', deviceId);
    if (lastEditIndex !== null) {
      $('#paramGrid').datagrid('endEdit', lastEditIndex);
      lastEditIndex = null;
    }
    $.get('@Url.Action("GetParamsByMachine","MachineParam")', { deviceId }, function(res) {
      if (!res.success) {
        return $.messager.alert('���~', res.message || '�L�kŪ���Ѽ�');
      }
      console.log('�ѼƩw�q�^��:', res.data);
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