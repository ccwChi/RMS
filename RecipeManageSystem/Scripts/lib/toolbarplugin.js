(function ($) {
	$.fn.toolBar = function (options) {
		var settings = $.extend({
			//預設值
			toolBarTitle: "會議資料管理",
		}, options);
		this.each(function () {
			let toolbartxt =  '<div class="bs-panel-group margin_0" id="accordion1"><div class="bs-panel bs-panel-default">'
				+ '<div class="bs-panel-heading"><div class="bs-panel-title"><a class="collapsed" data-toggle="collapse" data-parent="#accordion1" href="#collapse1">'
				+ '<i class="rt-icon2-bubble highlight"></i><svg width="1em" height="1em" viewBox="0 0 16 16" class="bi bi-ui-checks-grid" fill="currentColor" xmlns="http://www.w3.org/2000/svg">'
				+ '<path fill-rule="evenodd" d="M2 10a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1H2zm9-9a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1V2a1 1 0 0 0-1-1h-3zm0 9a1 1 0 0 0-1 1v3a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-3a1 1 0 0 0-1-1h-3zm0-10a2 2 0 0 0-2 2v3a2 2 0 0 0 2 2h3a2 2 0 0 0 2-2V2a2 2 0 0 0-2-2h-3zM2 9a2 2 0 0 0-2 2v3a2 2 0 0 0 2 2h3a2 2 0 0 0 2-2v-3a2 2 0 0 0-2-2H2zm7 2a2 2 0 0 1 2-2h3a2 2 0 0 1 2 2v3a2 2 0 0 1-2 2h-3a2 2 0 0 1-2-2v-3zM0 2a2 2 0 0 1 2-2h3a2 2 0 0 1 2 2v3a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V2zm5.354.854l-2 2a.5.5 0 0 1-.708 0l-1-1a.5.5 0 1 1 .708-.708L3 3.793l1.646-1.647a.5.5 0 1 1 .708.708z" />'
				+ '</svg>' + settings.toolBarTitle
				+ '<div id="collapse1" class="panel-collapse collapse in"><div class="bs-panel-body" style="display:inline-block;margin:1px;"><div class="header-toolbar">'
				+ '<a href="#" class="btnfirst" onclick="btnClick(\'first\');">首筆</a>'
				+ '<a href="#" class="btnprevious" onclick="btnClick(\'previous\');">前筆</a>'
				+ '<a href="#" class="btnnext" onclick="btnClick(\'next\');">後筆</a>'
				+ '<a href="#" class="btnlast" onclick="btnClick(\'last\');">尾筆</a>'
				+ '<a href="#" class="btnselect" onclick="btnClick(\'search\');">查詢</a>'
				+ '<a href="#" class="separatorbar"></a>'
				+ '<a href="#" class=" btnadd" onclick="btnClick(\'add\');">新增</a>'
				+ '<a href="#" class="btnedit" onclick="btnClick(\'edit\');">修改</a>'
				+ '<a href="#" class="btnsave" onclick="btnClick(\'save\');">存檔</a>'
				+ '<a href="#" class="btnrefresh" onclick="btnClick(\'refresh\');">重新整理</a>'
				+ '<a href="#" class="btncancel" onclick="btnClick(\'cancel\');">取消</a>'
				+ '<a href="#" class="btnremove" onclick="btnClick(\'delete\');">刪除</a>'
				+ '<a href="#" class="btncopy" onclick="btnClick(\'copy\');">複製</a>'
				+ '<a href="#" class="separatorbar"></a>'
				+ '<a href="#" class="btnlock" onclick="btnClick(\'lock\');">覆核</a>'
				+ '<a href="#" class="btnunlock" onclick="btnClick(\'unlock\');">取消覆核</a>'
				+ '<a href="#" class="btnreject" onclick="btnClick(\'reject\');">退件</a>'
				+ '<a href="#" class="btnsend" onclick="btnClick(\'send\');">送出</a>'
				+ '<a href="#" class="separatorbar"></a>'
				+ '<a href="#" class="btninvalid" onclick="btnClick(\'invalid\');">作廢</a>'
				+ '<a href="#" class="btnuninvalid" onclick="btnClick(\'uninvalid\');">取消作廢</a>'
				+ '<a href="#" class="separatorbar"></a>'
				+ '<a href="#" class="btnclose" onclick="btnClick(\'close\');">結案</a>'
				+ '<a href="#" class="btnunclose" onclick="btnClick(\'unclose\');">取消結案</a>'
				+ '<a href="#" class="separatorbar"></a>'
				+ '<a href="#" class="btnreport" onclick="btnClick(\'report\');">報表</a>';
			$(this).append(toolbartxt);
			$('.btnfirst').linkbutton({ iconCls: 'icon-large-first', size: 'large', iconAlign: 'top', width: 55,height:57 })
			$('.btnprevious').linkbutton({ iconCls: 'icon-large-previous', size: 'large', iconAlign: 'top', width: 55, height: 57 })
			$('.btnnext').linkbutton({ iconCls: 'icon-large-next', size: 'large', iconAlign: 'top', width: 55, height: 57 })
			$('.btnlast').linkbutton({ iconCls: 'icon-large-last', size: 'large', iconAlign: 'top', width: 55, height: 57})
			$('.btnselect').linkbutton({ iconCls: 'icon-large-select', size: 'large', iconAlign: 'top', width: 55, height: 57 })
			$('.btnadd').linkbutton({ iconCls: 'icon-large-add', size: 'large', iconAlign: 'top', width: 55, height: 57 })
			$('.btnedit').linkbutton({ iconCls: 'icon-large-edit', size: 'large', iconAlign: 'top', width: 55, height: 57 })
			$('.btnsave').linkbutton({ iconCls: 'icon-large-save', size: 'large', iconAlign: 'top', width: 55, height: 57 })
			$('.btnrefresh').linkbutton({ iconCls: 'icon-large-refresh', size: 'large', iconAlign: 'top', width: 58, height: 57 })
			$('.btncancel').linkbutton({ iconCls: 'icon-large-cancel', size: 'large', iconAlign: 'top', width: 55, height: 57 })
			$('.btnremove').linkbutton({ iconCls: 'icon-large-delete', size: 'large', iconAlign: 'top', width: 55, height: 57 })
			$('.btncopy').linkbutton({ iconCls: 'icon-large-copy', size: 'large', iconAlign: 'top', width: 55, height: 57 })
			$('.btnlock').linkbutton({ iconCls: 'icon-large-lock', size: 'large', iconAlign: 'top', width: 55, height: 57 })
			$('.btnunlock').linkbutton({ iconCls: 'icon-large-unlock', size: 'large', iconAlign: 'top', width: 58, height: 57 })
			$('.btnreject').linkbutton({ iconCls: 'icon-large_smartart', size: 'large', iconAlign: 'top', width: 55, height: 57 })
			$('.btnsend').linkbutton({ iconCls: 'icon-large-reject', size: 'large', iconAlign: 'top', width: 55, height: 57 })
			$('.btninvalid').linkbutton({ iconCls: 'icon-large-invalid', size: 'large', iconAlign: 'top', width: 55, height: 57})
			$('.btnuninvalid').linkbutton({ iconCls: 'icon-large-uninvalid', size: 'large', iconAlign: 'top', width: 58, height: 57 })
			$('.btnclose').linkbutton({ iconCls: 'icon-large-close', size: 'large', iconAlign: 'top', width: 55, height: 57 })
			$('.btnunclose').linkbutton({ iconCls: 'icon-large-unclose', size: 'large', iconAlign: 'top', width: 58, height: 57 })
			$('.btnreport').linkbutton({ iconCls: 'icon-large-report', size: 'large', iconAlign: 'top', width: 55, height: 57 })
		});
		$.parser.plugins.push('toolBar');
		//return this;
	}
}(jQuery));