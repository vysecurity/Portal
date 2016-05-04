$(document).ready(function () {


    //$("#username").on("change",function()
	//{
	//	var b = IsEmail($(this).val())
	//	if( b == false)
	//	{
	//		$('#huu').show();
	//	}
	//	else
	//	{
	//		$('#huu').hide();
	//	}
	//})
   
});

function IsEmail(email) {
  var regex = /^([a-zA-Z0-9_.+-])+\@(([a-zA-Z0-9-])+\.)+([a-zA-Z0-9]{2,4})+$/;
  return regex.test(email);
}