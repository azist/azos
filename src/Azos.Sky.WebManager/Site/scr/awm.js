
//---------------------------------------------------------------------------------------------
//GLOBALS
//---------------------------------------------------------------------------------------------


function global_READY()
{
  //IOS embedded site - make all link open inside the same app
  if (window.navigator.standalone)
  {
        //todo need to handle Internal vs internal links
        $( document ).on("click", "a", function( event ){
            event.preventDefault();
            location.href = $( event.target ).attr("href");
        });
  }
};


WAVE.LOCALIZER.rus =
{
    "--ANY-SCHEMA--":
    {
     "error":
     {
       "Field '@f@' must have a value": "Поле '@f@' должно быть заполнено",
       "Field '@f@' value can not be less than '@b@'": "Значение поля '@f@' не может быть меньше чем '@b@'",
       "Field '@f@' value can not be greater than '@b@'": "Значение поля '@f@' не может быть больше чем '@b@'",
       "Field '@f@' value can not be longer than @b@ characters": "Длина значения поля '@f@' не может быть больше чем @b@ символов",
       "Field '@f@' value can not be shorter than @b@ characters": "Длина значения поля '@f@' не может быть меньше чем @b@ символов",
       "Field '@f@' value '@v@' is not allowed": "Поле '@f@' не может иметь значение '@v@'",
       "Field '@f@' must be a valid e-mail address": "Неправильный е-майл адрес в поле '@f@'",
       "Field '@f@' must start from letter and contain only letters or digits separated by single '.' or '-' or '_'":
         "Поле '@f@' должно начинаться с буквы и содержать только буквы или цифры, разделённые одинарным '.' или '-' или '_'"
     }
    }



};//rus


var READY_FUNC = null;

$(document).ready(function() {
  global_READY();
  if (READY_FUNC) READY_FUNC();
});