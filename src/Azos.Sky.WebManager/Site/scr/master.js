 var CONSTS = {
    hdrTopHeight: 72,
    hdrTopMinHeight: 28,
    logoWidth: 100,
    minLogoWidth: 40,
    logoFSRegular: "2em",
    logoFSThreshold: 40,
    subLogoFSThreshold: 60
 };

var menuIsVisible = false;

READY_FUNC = function() {

     $(window).scroll(function(){
        var pos = $(window).scrollTop();
        var h = CONSTS.hdrTopHeight - pos;
        if (h<CONSTS.hdrTopMinHeight) h = CONSTS.hdrTopMinHeight;
        $("#hdrTop").height(h);

        var w = CONSTS.logoWidth - pos;
        if (w<CONSTS.minLogoWidth) w = CONSTS.minLogoWidth;

        $("#divLogo").width(w);

        var fsBelow = h<CONSTS.logoFSThreshold;
        var fs = fsBelow ? (h - 4)+'px' : CONSTS.logoFSRegular;
        $("#divTitle").css('font-size', fs);
        $("#divSubTitle").css('visibility', h<CONSTS.subLogoFSThreshold ? 'hidden' : 'visible');

        if ($("#btnNavMenu").css('display')!='none') $("#navTopMenu").css('visibility', 'hidden');
      });

     $(window).resize(function() {
        if ($("#btnNavMenu").css('display') == 'none') {
          $("#navTopMenu").css('visibility', 'visible');
          menuIsVisible = false;
        }
        else {
          if (menuIsVisible)
            $("#navTopMenu").css('visibility', 'visible');
          else {
            $("#navTopMenu").css('visibility', 'hidden');
            menuIsVisible = false;
          }
        }
     });

     $(document).click(function(evt) {
       var target = $(evt.target);

       if ($("#btnNavMenu").css('display')!='none')
       {
          if (target.attr('id') == 'btnNavMenu') {
            menuIsVisible = ($("#navTopMenu").css('visibility') != 'visible');
            if (menuIsVisible)
              $("#navTopMenu").css('visibility', 'visible');
            else
              $("#navTopMenu").css('visibility', 'hidden');
            return;
         }
         if (target.parent().attr('id') != 'navTopMenu')
           $("#navTopMenu").css('visibility', 'hidden');
       }
     });

};