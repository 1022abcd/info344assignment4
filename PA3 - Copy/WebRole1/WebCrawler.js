function go() {
    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/StartCrawling",
        success: function (data) {
        }
    });
}

function stop() {
    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/StopCrawling",
        success: function (data) {
        }
    });
}

function clear2() {
    console.log("yay");
    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/ClearIndex",
        success: function (data) {
        }
    });
}

function searchForNBA() {
    console.log("Connected to seachForNBA");
    var input = $("#query").val().trim(); 
    $.ajax({
        crossDomain: true,
        contentType: "application/json; charset=utf-8",
        url: "http://ec2-54-186-6-227.us-west-2.compute.amazonaws.com/pa4nba.php?playerName=" + input,
        data: {},
        dataType: "jsonp",
        success: function (data) {
            $("#nbaResult").empty();
            if (data.length > 0) {
                nbafound = true;
                $("#nbaresults").show();
                var parentDiv = document.getElementById("nbaresults");
                var playerimg = document.getElementById("nbaimg");
                var teamlogo = document.getElementById("teamlogo");
                var fullname = document.getElementById("fullname");
                var stats = data[0];
                var result = document.createElement("div");
                var splitted = stats[0].split(' ');
                result.className = "pageTile";
                playerimg.src = 'https://nba-players.herokuapp.com/players/' + splitted[1] + "/" + splitted[0];
                fullname.innerHTML = stats[0];
                $("#TEAM").text(stats[1]);
                $("#GP").text(stats[2]);
                $("#PPG").text(stats[21]);
                $("#3PTM").text(stats[7]);
                $("#REB").text(stats[15]);
                $("#AST").text(stats[16]);
                parentDiv.appendChild(result);
            }
        }
    });
}

function search() {
    searchForNBA();
    searchRequest();
    //getSearchResult();
}

function searchRequest() {
    $("#result").empty();
    $.ajax({
        type: "POST",
        url: "PA2.asmx/SearchTrie",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({
            word: $("#query").val().trim()
        }),
        dataType: "json",
        success: function (data) {
            $("#result").empty();
            var test = JSON.parse(data.d);
            
            for (i = 0; i < test.length; i++) {
                var div = document.createElement("div");
                div.innerHTML = test[i];
                $("#result").append(div);
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            console.log("error " + XMLHttpRequest + " " + errorThrown);
            console.log(XMLHttpRequest);
        }
    });
    $("#result").empty();
}

function getSearchResult() {
    console.log("Connected to GetSearchResult");
    $("#linkResult").empty();
    $.ajax({
        type: "POST",
        data: JSON.stringify({ 'input': $("#query").val().trim() }),
        dataType: "json",
        url: "Admin.asmx/GetSearchResult",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            console.log(data);

            var test = JSON.parse(data.d);
            for (i = 0; i < test.length; i++) {
                var div = document.createElement("div");
                div.innerHTML = test[i];
                $("#linkResult").append(div);
            }
        }
    });
}

function findTitle() {
    $("#title").empty();
    $.ajax({
        type: "POST",
        data: JSON.stringify({ 'link': $("#input").val().trim()}),
        dataType: "json",
        url: "Admin.asmx/GetPageTitle",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            console.log(data);
            var div = document.createElement("div");
            div.innerHTML = data.d;
            $("#title").append(div);
        }
    });
}

function getHTMLQueueCount() {
    console.log("gethtml");
    $.ajax({
        type: "POST",
        data: {},
        dataType: "json",
        url: "Admin.asmx/GetHTMLQueueCount",
        success: function (data) {
            console.log(data);
            var test = stringify(data.d);
            var div = document.createElement("div");
            div.innerHTML = test;
            $("#htmlCount").append(div);
        }
    });
}

window.onload = function getHTMLQueueCount() {
    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/GetHTMLQueueCount",
        success: function (data) {
            console.log(data);
            var div = document.createElement("div");
            div.innerHTML = data;
            $("#htmlCount").append(div);
        }
    });

    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/GetLinkQueueCount",
        success: function (data) {
            console.log(data);
            var div = document.createElement("div");
            div.innerHTML = data;
            $("#xmlCount").append(div);
        }
    });

    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/GetState",
        success: function (data) {
            console.log(data);
            data = data.replace('[', '');
            data = data.replace(']', '');
            var div = document.createElement("div");
            div.innerHTML = data;
            $("#state").append(div);
        }
    });

    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/GetPerformance",
        success: function (data) {
            console.log(data);
            data = data.replace('[', '');
            data = data.replace(']', '');
            var split = data.split(',');

            var memory = document.createElement("div");
            var cpu = document.createElement("div");
            cpu.innerHTML = split[0];
            memory.innerHTML = split[1];
            $("#memory").append(memory);
            $("#cpu").append(cpu);
        }
    });

    $.ajax({
        type: "POST",
        data: {},
        dataType: "text",
        url: "Admin.asmx/LastTenTable",
        success: function (data) {
            console.log(data);
            var div = document.createElement("div");
            data = data.replace('[', '');
            data = data.replace(']', '');
            div.innerHTML = data;
            $("#lastten").append(div);
        }
    });

    $.ajax({
        type: "POST",
        data: {},
        dataType: "json",
        url: "Admin.asmx/GetErrors",
        success: function (data) {
            console.log(data);
            var div = document.createElement("div");
            div.innerHTML = data;
            $("#error").append(div);
        }
    });
};