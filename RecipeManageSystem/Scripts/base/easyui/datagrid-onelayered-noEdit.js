(function ($) {
    $.fn.maindg = function () {
        return this.each(function () {
            let cols = maindg_columnsSet();
            $(this).datagrid({
                columns: cols,
                rownumbers: true, singleSelect: true, remoteSort: false, autoRowHeight: false,  
            })
        })
    };
    $.parser.plugins.push('maindg');
})(jQuery);

