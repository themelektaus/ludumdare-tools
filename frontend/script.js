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
    "-1": "Opt-out",
    0: "Unrated",
    1: "Poor",
    1.5: "Less good",
    2: "Fair",
    2.5: "Average",
    3: "Not bad",
    3.5: "Good",
    4: "Very good",
    4.5: "Excellent",
    5: "Perfect"
};

const CURRENT_LD_NUMBER = 51;

const timeouts = {};

let currentGame;

function wheelEvent(e) {
    if (e.target.classList.contains("dialog__background"))
        e.preventDefault();

    if (e.target.classList.contains("dialog__box-header"))
        e.preventDefault();

    if (e.target.classList.contains("dialog__box-footer"))
        e.preventDefault();
}

function disableScroll() {
    document.body.addEventListener("wheel", wheelEvent, { passive: false });
}

function enableScroll() {
    document.body.removeEventListener("wheel", wheelEvent)
}

function openLoginDialog()
{
    document.getElementById("login-dialog").classList.add("visible");
    document.getElementById("username").focus();
}

function openGameDialog(game) {
    currentGame = game;
    disableScroll();
    const dialog = document.getElementById("game-dialog");
    dialog.querySelector(".dialog__box-header").innerHTML = game.name;
    dialog.querySelector(".dialog__box-body").innerHTML = `<div class="markdown">${game.static_body_html}</div>`;
    dialog.classList.add("visible");
}

function openSubmission() {
    window.open(currentGame.static_path);
}

function closeGameDialog() {
    enableScroll();
    document.getElementById("game-dialog").classList.remove("visible");
}

function openOptionsDialog()
{
    document.getElementById("options-dialog").classList.add("visible");
}

function closeOptionsDialog()
{
    document.getElementById("options-dialog").classList.remove("visible");
    reset();
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
    const loginBox = document.querySelector(".dialog__box");
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
    document.getElementById("login-dialog").classList.remove("visible");
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
    return +(localStorage.getItem("ld") || CURRENT_LD_NUMBER);
}

function previousLD()
{
    if (busy)
        return;

    localStorage.setItem("ld", Math.max(45, getLD() - 1));
    reset();
}

function nextLD()
{
    if (busy)
        return;

    localStorage.setItem("ld", Math.min(CURRENT_LD_NUMBER, getLD() + 1));
    reset();
}

function reset()
{
    page = 0;
    dataElement.innerHTML = "";
    fetchGames();
}

let busy = false;
let searchTask = null;

function updateSearch(e)
{
    localStorage.setItem("search", e.target.value);

    if (searchTask)
    {
        clearTimeout(searchTask)
        searchTask = null;
    }

    searchTask = setTimeout(() =>
    {
        searchTask = null;
        if (busy)
            updateSearch(e);
        else
            reset();
    }, 1000);
}

function updateGUI(data)
{
    document.getElementById("previous-ld-button").style.display = getLD() == 45 ? "none" : "block";
    document.getElementById("next-ld-button").style.display = getLD() == CURRENT_LD_NUMBER ? "none" : "block";

    for (const ldElement of document.getElementsByClassName("ld"))
        ldElement.innerHTML = getLD();
}

function updateRateProgress(rateProgress) {
    for (const rateProgressElement of document.getElementsByClassName("rate-progress"))
        rateProgressElement.innerHTML = rateProgress.toFixed("2");
}

function fetchGames()
{
    if (busy)
        return;
    
    busy = true;

    moreButton.remove();

    eventElement.appendChild(loadingElement);

    updateGUI();
    updateRateProgress(0);
    
    const request = new XMLHttpRequest();

    request.open(
        "POST",
        "/api/ld" + getLD()
        + "/?page=" + (page++)
        + "&search=" + (localStorage.getItem("search") ?? "")
        + "&filter=" + (localStorage.getItem("search") ?? "")
    );

    request.onreadystatechange = function () {
        if (request.readyState !== 4)
            return;

        loadingElement.remove();

        if (request.status !== 200) {
            logoutElement.style.display = "none";
            loginElement.style.display = "block";
            infoElement.innerHTML = "⚠ Unauthorized ⚠";
            dataElement.appendChild(infoElement);
            return;
        }

        const data = JSON.parse(request.responseText);

        //const converter = new showdown.Converter();

        if (data === null) {
            infoElement.innerHTML = "⚠ No data found ⚠";
            dataElement.appendChild(infoElement);
            return;
        }

        updateRateProgress(data.rateProgress);

        const games = data.games;

        if (games.length % 2 != 0)
            games.push({});

        //for (const item of data.reverse()) {
        for (const item of games) {
            const link = item.static_path || "";
            //const cover = item.static_cover || "";
            const thumbnail = item.thumbnail_url || "";
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
            //coverWrapperElement.href = link;
            coverWrapperElement.onclick = e => openGameDialog.call(this, item);
            coverWrapperElement.classList.add("entry__cover-wrapper");
            entryInnerElement.appendChild(coverWrapperElement);

            const coverElement = document.createElement("div");
            coverElement.classList.add("entry__cover");
            //coverElement.style.backgroundImage = "url(\"" + cover + "\")";
            coverElement.style.backgroundImage = "url(\"" + thumbnail + "\")";
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

            if (item.id) {
                for (const i in grades) {
                    let rating = { id: 0, name: null, value: -1 };

                    const grade = grades[i];
                    if (!item.opt_outs[i]) {
                        rating = item.rating[grade.toLowerCase()]
                    }

                    const ratingElement = document.createElement("div");
                    ratingElement.classList.add("rating");
                    ratingElement.setAttribute("data-id", rating.id);
                    ratingElement.setAttribute("data-name", rating.name);
                    ratingElement.setAttribute("data-keyword", keywords[rating.value].toLowerCase());

                    let html = "";
                    html += "<div class=\"rating__grade\">" + grade + "</div>";
                    html += "<div class=\"rating__value\"><div class=\"value\">" + (rating.value || "&nbsp;") + "</div></div>";
                    html += "<div class=\"rating__keyword\"><div class=\"bar\"></div><div class=\"text\">" + keywords[rating.value] + "</div></div>";
                    ratingElement.innerHTML = html;

                    ratingsElement.appendChild(ratingElement);
                }
            }

            entryInnerElement.appendChild(ratingsElement);
        }

        if (games.length == 20)
            eventElement.appendChild(moreButton);

        busy = false;
    }

    const formData = new FormData();
    formData.append("token", getToken());

    const options = loadOptions();
    formData.append("filterJam", options.filterJam);
    formData.append("filterCompo", options.filterCompo);
    formData.append("filterRated", options.filterRated);
    formData.append("filterUnrated", options.filterUnrated);
    formData.append("orderSmart", options.orderSmart);
    formData.append("orderTop", options.orderTop);
    formData.append("orderCategory", options.orderCategory);

    request.send(formData);
}

function loadOptions() {
    let options = {};
    let savedOptions = {};

    const optionsJson = localStorage.getItem("options") ?? "{}";
    try { savedOptions = JSON.parse(optionsJson); } catch { }

    options.filterJam = savedOptions.filterJam ?? true;
    options.filterCompo = savedOptions.filterCompo ?? true;
    options.filterRated = savedOptions.filterRated ?? true;
    options.filterUnrated = savedOptions.filterUnrated ?? true;
    options.orderSmart = savedOptions.orderSmart ?? true;
    options.orderTop = savedOptions.orderTop ?? false;
    options.orderCategory = savedOptions.orderCategory ?? "";

    return options;
}

function saveOptions(options) {
    localStorage.setItem("options", JSON.stringify(options));
}

function getOption(key) {
    const options = loadOptions();
    return options[key];
}

function setOption(key, value) {
    const options = loadOptions();
    options[key] = value;
    saveOptions(options);
}

const checkboxContainer = document.querySelectorAll(".checkbox__container");
for (const c of checkboxContainer) {
    const input = c.querySelector("input");
    if (input.hasAttribute("data-filter-jam"))
        input.checked = getOption("filterJam");

    if (input.hasAttribute("data-filter-compo"))
        input.checked = getOption("filterCompo");

    if (input.hasAttribute("data-filter-rated"))
        input.checked = getOption("filterRated");

    if (input.hasAttribute("data-filter-unrated"))
        input.checked = getOption("filterUnrated");

    if (input.hasAttribute("data-order-smart"))
        input.checked = getOption("orderSmart");

    if (input.hasAttribute("data-order-top"))
        input.checked = getOption("orderTop");

    if (input.hasAttribute("data-order-category")) {
        const orderCategory = getOption("orderCategory");
        input.checked = orderCategory == input.getAttribute("data-order-category");
    }

    c.addEventListener("click", e => {
        input.checked = !input.checked;

        if (input.hasAttribute("data-filter-jam"))
            setOption("filterJam", input.checked);

        if (input.hasAttribute("data-filter-compo"))
            setOption("filterCompo", input.checked);

        if (input.hasAttribute("data-filter-rated"))
            setOption("filterRated", input.checked);

        if (input.hasAttribute("data-filter-unrated"))
            setOption("filterUnrated", input.checked);

        if (input.hasAttribute("data-order-smart")) {
            for (const orderInput of document.querySelectorAll(".order-input"))
                orderInput.checked = false;
            input.checked = true;
            setOption("orderSmart", true);
            setOption("orderTop", false);
            setOption("orderCategory", "");
        }

        if (input.hasAttribute("data-order-top")) {
            for (const orderInput of document.querySelectorAll(".order-input"))
                orderInput.checked = false;
            input.checked = true;
            setOption("orderSmart", false);
            setOption("orderTop", true);
            setOption("orderCategory", "");
        }

        if (input.hasAttribute("data-order-category")) {
            for (const orderInput of document.querySelectorAll(".order-input"))
                orderInput.checked = false;
            input.checked = true;
            setOption("orderSmart", false);
            setOption("orderTop", false);
            setOption("orderCategory", input.getAttribute("data-order-category"));
        }
    });
}

const loadingElement = document.querySelector(".loading");
loadingElement.remove();

const moreButton = document.querySelector(".more");
moreButton.onclick = () => fetchGames();

const loginElement = document.getElementById("login");
loginElement.style.display = getToken() ? "none" : "block";

const logoutElement = document.getElementById("logout");
logoutElement.style.display = getToken() ? "block" : "none";

const eventElement = document.querySelector(".event");
const dataElement = document.getElementById("data");

const searchInput = document.getElementById("search");
searchInput.value = localStorage.getItem("search");

let page = 0;

const infoElement = document.createElement("div");
infoElement.classList.add("info");

fetchGames();