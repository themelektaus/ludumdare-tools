const timeouts = { };

function openLoginDialog()
{
    document.getElementById("login-dialog").style.display = "block";
    document.getElementById("username").focus();
}

function onLoginKeyDown()
{
    if (arguments[0].keyCode !== 13)
        return false;
    
    login.apply(this);
    return true;
}

function shakeDialogBox()
{
    const loginBox = document.querySelector(".login__box");
    if (timeouts[loginBox])
    {
        clearTimeout(timeouts[loginBox]);
        timeouts[loginBox] = null;
    }
    loginBox.classList.remove("shake");
    setTimeout(() => loginBox.classList.add("shake"));
    timeouts[loginBox] = setTimeout(() => loginBox.classList.remove("shake"), 500);
}

function login()
{
    const username = document.getElementById("username").value;
    const password = document.getElementById("password").value;
    
    const request = new XMLHttpRequest();
    
    request.open("POST", "/api/login");
    
    request.onreadystatechange = function()
    {
        if (request.readyState != 4)
            return;
        
        if (request.status != 200 || !request.responseText)
        {
            shakeDialogBox();
            return
        }
        
        localStorage.setItem("token", request.responseText);
        location.reload();
    };
    
    const formData = new FormData();
    formData.append("username", username);
    formData.append("password", password);
    request.send(formData);
}

function cancel()
{
    document.getElementById("login-dialog").style.display = "none";
}

function logout()
{
    const request = new XMLHttpRequest();
    
    request.open("POST", "/api/logout");
    
    request.onreadystatechange = function()
    {
        if (request.readyState !== 4)
            return;
            
        if (request.status !== 200)
            return;
        
        localStorage.removeItem("token");
        location.reload();
    };

    const formData = new FormData();
    formData.append("token", getToken());
    request.send(formData);
}

function getToken()
{
    return localStorage.getItem("token") || "";
}

function getLD()
{
    return +(localStorage.getItem("ld") || 50);
}

function previousLD()
{
    localStorage.setItem("ld", Math.max(1, getLD() - 1));
    location.reload();
}

function nextLD()
{
    localStorage.setItem("ld", Math.min(50, getLD() + 1));
    location.reload();
}

const grades = [
    "Overall",
    "Fun",
    "Innovation",
    "Theme",
    "Graphics",
    "Audio",
    "Humor",
    "Mood"
];

const keywords = {
  "-1"  : "Opt-out",
    0   : "Unrated",
    1   : "Poor",
    1.5 : "Less good",
    2   : "Fair",
    2.5 : "Average",
    3   : "Not bad",
    3.5 : "Good",
    4   : "Very good",
    4.5 : "Excellent",
    5   : "Perfect"
};

const loginElement = document.getElementById("login");
const logoutElement = document.getElementById("logout");

logoutElement.style.display = getToken() ? "block" : "none";
loginElement.style.display = getToken() ? "none" : "block";

if (getLD() == 1)
    document.getElementById("previous-ld-button").style.display = "none";
    
if (getLD() == 50)
    document.getElementById("next-ld-button").style.display = "none";

const dataElement = document.getElementById("data");

const infoElement = document.createElement("div");
infoElement.classList.add("info");

for (const ldElement of document.getElementsByClassName("ld"))
    ldElement.innerHTML = getLD();

const request = new XMLHttpRequest();

request.open("POST", "/api/ld" + getLD() + "/ratings");

request.onreadystatechange = function()
{
    if (request.readyState !== 4)
        return;
    
    dataElement.innerHTML = "";
        
    if (request.status !== 200)
    {
        logoutElement.style.display = "none";
        loginElement.style.display = "block";
        infoElement.innerHTML = "⚠ Unauthorized ⚠";
        dataElement.appendChild(infoElement);
        return;
    }
    
    const data = JSON.parse(request.responseText);
    
    //const converter = new showdown.Converter();
    
    if (data === null)
    {
        infoElement.innerHTML = "⚠ No data found ⚠";
        dataElement.appendChild(infoElement);
        return;
    }
    
    if (data.length % 2 != 0)
        data.unshift({ });
    
    for (const item of data.reverse())
    {
        const link = item.static_path || "";
        const cover = item.static_cover || "";
        let title = item.name || "Untitled";
        const type = item.subsubtype || "unknown";
        const userComments = item.userComments || 0;
        
        if (title.length > 50)
            title = title.substr(0, 50) + "...";
        
        const entryElement = document.createElement("div");
        entryElement.classList.add("entry");
        if (!link)
            entryElement.style.visibility = "hidden";
        dataElement.appendChild(entryElement);
        
        const entryInnerElement = document.createElement("div");
        entryInnerElement.classList.add("entry__inner");
        entryElement.appendChild(entryInnerElement);
        
        const coverWrapperElement = document.createElement("a");
        coverWrapperElement.href = link;
        coverWrapperElement.classList.add("entry__cover-wrapper");
        entryInnerElement.appendChild(coverWrapperElement);
        
        const coverElement = document.createElement("div");
        coverElement.classList.add("entry__cover");
        coverElement.style.backgroundImage = "url(\"" + cover + "\")";
        coverWrapperElement.appendChild(coverElement);
        
        const titleElement = document.createElement("div");
        titleElement.classList.add("entry__title");
        titleElement.innerHTML = title;
        entryInnerElement.appendChild(titleElement);
        
        const typeElement = document.createElement("div");
        typeElement.classList.add("entry__type");
        typeElement.classList.add("entry__type-" + type);
        typeElement.innerHTML = type.toUpperCase();
        entryInnerElement.appendChild(typeElement);

        const userCommentsElement = document.createElement("div");
        userCommentsElement.classList.add("entry__user-comments");
        userCommentsElement.classList.add("entry__user-comments-" + (
            userComments == 0 ? "zero" : (userComments == 1 ? "one" : "more")
        ));
        entryInnerElement.appendChild(userCommentsElement);
        
        //const bodyElement = document.createElement("div");
        //bodyElement.classList.add("entry__body");
        //bodyElement.innerHTML = converter.makeHtml(item.static_body);
        //entryInnerElement.appendChild(bodyElement);
        
        const ratingsElement = document.createElement("div");
        ratingsElement.classList.add("ratings");
        
        if (item.id)
        {
            for (const i in grades)
            {
                let value = -1;

                const grade = grades[i];
                if (!item.opt_outs[i])
                {
                    value = item.rating[grade.toLowerCase()]
                    if (!value)
                        value = 0;
                }

                const ratingElement = document.createElement("div");
                ratingElement.classList.add("rating");
                ratingElement.setAttribute("data-keyword", keywords[value].toLowerCase());
                
                let html = "";
                html += "<div class=\"rating__grade\">" + grade + "</div>";
                html += "<div class=\"rating__value\"><div class=\"value\">" + (value || "&nbsp;") + "</div></div>";
                html += "<div class=\"rating__keyword\"><div class=\"bar\"></div><div class=\"text\">" + keywords[value] + "</div></div>";
                ratingElement.innerHTML = html;
                
                ratingsElement.appendChild(ratingElement);
            }
        }
        
        entryInnerElement.appendChild(ratingsElement);
    }
}

const formData = new FormData();
formData.append("token", getToken());
request.send(formData);