

function card(x)
{
    //original code
    //var url = ("cards/test" + x + ".html");
    //var carded = window.open(url, "_blank");
    //

    //updated for MVC migration
    var cardDisplayPath = "card/display/?template=";
    var url = (cardDisplayPath + x);
	var carded = window.open(url, "_blank");
}
function displayCreateBW() { 
	document.getElementById("cardbar").style.display = 'none';
	document.getElementById("socialbar").style.display = 'none';
	document.getElementById("createbar").style.display = 'inline-block';
	var frame = document.getElementById("bigthing");
	frame.data = ("cards/cardstockbw.html");
}

function displaySocial()
{
	document.getElementById("cardbar").style.display = 'none';
	document.getElementById("createbar").style.display = 'none';
	document.getElementById("socialbar").style.display = 'inline-block';
	var frame = document.getElementById("bigthing");
	frame.data = ("social.html");
}

function displayCreate()
{
	document.getElementById("cardbar").style.display = 'none';
	document.getElementById("socialbar").style.display = 'none';
	document.getElementById("createbar").style.display = 'inline-block';
	var frame = document.getElementById("bigthing");
	frame.data = ("cards/cardstockbw.html");
}

function displayCard()
{ 
	document.getElementById("socialbar").style.display = 'none';
	document.getElementById("createbar").style.display = 'none';
	document.getElementById("cardbar").style.display = 'inline-block';
	var frame = document.getElementById("bigthing"); 
	frame.data="cards/cardstockbw.html";
}
function displayFriendCard()
{

}
function nope() {alert("can't do that yet")}