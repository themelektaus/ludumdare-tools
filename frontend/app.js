MIN_LD_NUMBER = 45
MAX_LD_NUMBER = 51

class Storage
{
    static get token()
    {
        return localStorage.getItem("token") || ""
    }
    
    static set token(value)
    {
        localStorage.setItem("token", value)
    }
    
    static removeToken()
    {
        localStorage.removeItem("token")
    }
    
    static get search()
    {
        return localStorage.getItem("search") || ""
    }
    
    static set search(value)
    {
        localStorage.setItem("search", value)
    }
    
    static get ld()
    {
        return +(localStorage.getItem("ld") || MAX_LD_NUMBER)
    }
    
    static set ld(value)
    {
        localStorage.setItem("ld", value)
    }
    
    static loadOptions()
    {
        const options = { }
        let savedOptions = { }
        
        const optionsJson = localStorage.getItem("options") ?? "{}"
        try { savedOptions = JSON.parse(optionsJson) } catch { }
        
        options.filterJam = savedOptions.filterJam ?? true
        options.filterCompo = savedOptions.filterCompo ?? true
        options.filterRated = savedOptions.filterRated ?? true
        options.filterUnrated = savedOptions.filterUnrated ?? true
        options.orderSmart = savedOptions.orderSmart ?? true
        options.orderTop = savedOptions.orderTop ?? false
        options.orderCategory = savedOptions.orderCategory ?? ""
        
        return options
    }
    
    static saveOptions(options)
    {
        delete options.orderSmart
        delete options.orderTop
        localStorage.setItem("options", JSON.stringify(options))
    }
    
    static getOption(key)
    {
        return Storage.loadOptions()[key]
    }

    static setOption(key, value)
    {
        const options = Storage.loadOptions()
        options[key] = value
        Storage.saveOptions(options)
    }
}

class DOM
{
    static loading = DOM.get(".loading")
    static moreButton = DOM.get(".more")
    
    static create(tagName)
    {
        return document.createElement(tagName)
    }
    
    static get(selectors)
    {
        return document.querySelector(selectors)
    }
    
    static getAll(selectors)
    {
        return document.querySelectorAll(selectors)
    }
    
    static timeout(key, startCallback, endCallback, ms)
    {
        DOM.timeouts ??= { }
        if (DOM.timeouts[key])
        {
            clearTimeout(DOM.timeouts[key])
            delete DOM.timeouts[key]
        }
        const f = () =>
        {
            if (startCallback)
            {
                endCallback(key)
                setTimeout(() => startCallback(key))
            }
            DOM.timeouts[key] = setTimeout(() =>
            {
                if (endCallback(key))
                    f()
            }, ms)
        }
        f()
    }
    
    static get body()
    {
        return document.body
    }
    
    static get dialogBackgrounds()
    {
        return DOM.getAll(".dialog__background")
    }
    
    static get loginDialog()
    {
        return DOM.get("#login-dialog")
    }
    
    static get usernameInput()
    {
        return DOM.get("#username")
    }
    
    static get passwordInput()
    {
        return DOM.get("#password")
    }
    
    static get loginDialogButtons()
    {
        return DOM.loginDialog.querySelectorAll("button")
    }
    
    static get gameDialog()
    {
        return DOM.get("#game-dialog")
    }
    
    static get gameDialogHeader()
    {
        return DOM.gameDialog.querySelector(".dialog__box-header")
    }
    
    static get gameDialogBody()
    {
        return DOM.gameDialog.querySelector(".dialog__box-body")
    }
    
    static get gameDialogButtons()
    {
        return DOM.gameDialog.querySelectorAll("button")
    }
    
    static get optionsDialog()
    {
        return DOM.get("#options-dialog")
    }
    
    static get optionsDialogButtons()
    {
        return DOM.optionsDialog.querySelectorAll("button")
    }
    
    static get loginButton()
    {
        return DOM.get("#login")
    }
    
    static get logoutButton()
    {
        return DOM.get("#logout")
    }
    
    static get eventElement()
    {
        return DOM.get(".event")
    }
    
    static get dataElement()
    {
        return DOM.get("#data")
    }
    
    static get searchInput()
    {
        return DOM.get("#search")
    }
    
    static get infoElement()
    {
        if (!DOM._infoElement)
            DOM._infoElement = DOM.create("div")
        return DOM._infoElement
    }
    
    static get previousLdButton()
    {
        return DOM.get("#previous-ld-button")
    }
    
    static get nextLdButton()
    {
        return DOM.get("#next-ld-button")
    }
    
    static get optionsButton()
    {
        return DOM.get("#options-button")
    }
    
    static get checkboxes()
    {
        return DOM.getAll(`input[type="checkbox"]`)
    }
    
    static get checkboxContainers()
    {
        return DOM.getAll(`.checkbox__container`)
    }
    
    static get orderCategoryCheckboxes()
    {
        return DOM.getAll(`input[data-option="orderCategory"]`)
    }
    
    static wheelEvent = e =>
    {
        if (e.target.classList.contains("dialog__background"))
            e.preventDefault()
    
        if (e.target.classList.contains("dialog__box-header"))
            e.preventDefault()
    
        if (e.target.classList.contains("dialog__box-footer"))
            e.preventDefault()
    }
    
    static disableScroll() 
    {
        DOM.body.addEventListener("wheel", DOM.wheelEvent, { passive: false })
    }
    
    static enableScroll()
    {
        DOM.body.removeEventListener("wheel", DOM.wheelEvent)
    }
    
    static openLoginDialog()
    {
        DOM.loginDialog.classList.add("visible")
        DOM.usernameInput.focus()
    }
    
    static shakeDialogBox()
    {
        DOM.timeout(
            DOM.get(".dialog__box"),
            x => x.classList.add("shake"),
            x => x.classList.remove("shake"),
            500
        )
    }
    
    static closeLoginDialog()
    {
        DOM.loginDialog.classList.remove("visible")
    }
    
    static openGameDialog(title, body)
    {
        DOM.disableScroll()
        DOM.gameDialogHeader.innerHTML = title
        DOM.gameDialogBody.innerHTML = `<div class="markdown">${body}</div>`
        DOM.gameDialog.classList.add("visible")
    }
    
    static closeGameDialog()
    {
        DOM.enableScroll()
        DOM.gameDialog.classList.remove("visible")
    }
    
    static openOptionsDialog()
    {
        DOM.optionsDialog.classList.add("visible")
    }

    static closeOptionsDialog()
    {
        DOM.optionsDialog.classList.remove("visible")
    }
}

class App
{
    static grades = [
        "Overall",
        "Fun",
        "Innovation",
        "Theme",
        "Graphics",
        "Audio",
        "Humor",
        "Mood"
    ]
    
    static keywords = {
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
    }
    
    static post(url, data)
    {
        const formData = new FormData
        for (const key in data)
            formData.append(key, data[key])
        return fetch(url, { method: "POST", body: formData })
    }
    
    constructor()
    {
        this.game = null
        this.clearGames()
    }
    
    clearGames()
    {
        this.page = 0
        DOM.dataElement.innerHTML = ""
    }
    
    run()
    {
        if (this.running)
            throw new Error
        
        this.running = true
        
        DOM.loading.remove()
        
        DOM.moreButton.onclick = () => this.fetchGames()
        
        DOM.loginButton.addEventListener("click", DOM.openLoginDialog)
        DOM.loginButton.style.display = Storage.token ? "none" : "block"
        
        DOM.usernameInput.addEventListener("keydown", e =>
        {
            if (e.keyCode == 13)
                this.login()
        })
        
        DOM.passwordInput.addEventListener("keydown", e =>
        {
            if (e.keyCode == 13)
                this.login()
        })
        
        DOM.loginDialogButtons[0].addEventListener("click", () => this.login())
        DOM.loginDialogButtons[1].addEventListener("click", DOM.closeLoginDialog)
        
        DOM.gameDialogButtons[0].addEventListener("click", () => this.openSubmission())
        DOM.gameDialogButtons[1].addEventListener("click", () => DOM.closeGameDialog())
        
        DOM.logoutButton.addEventListener("click", () => this.logout())
        DOM.logoutButton.style.display = Storage.token ? "block" : "none"
        
        DOM.searchInput.value = Storage.search
        DOM.searchInput.addEventListener("input", () =>
        {
            const x = DOM.searchInput
            Storage.search = x.value
            DOM.timeout(x, null, () =>
            {
                if (this.busy)
                    return true
                
                this.clearGames()
                this.fetchGames()
            }, 1000)
        })
        
        DOM.infoElement.classList.add("info")
        
        DOM.dialogBackgrounds.forEach(item =>
        {
            item.addEventListener("click", () =>
            {
                item.parentNode.classList.remove("visible")
            })
        })
        
        DOM.previousLdButton.addEventListener("click", () => this.previous())
        DOM.nextLdButton.addEventListener("click", () => this.next())
        
        DOM.optionsButton.addEventListener("click", DOM.openOptionsDialog)
        DOM.optionsDialogButtons[0].addEventListener("click", () =>
        {
            DOM.closeOptionsDialog()
            this.clearGames()
            this.fetchGames()
        })
        
        DOM.checkboxes.forEach(item =>
        {
            const option = item.dataset.option
            const category = item.dataset.category
            
            if (option)
            {
                const checked = Storage.getOption(option)
                if (option == "orderCategory")
                {
                    if (checked == category)
                        item.checked = checked
                }
                else
                {
                    item.checked = checked
                }
                
            }
        })
        
        DOM.checkboxContainers.forEach(item =>
        {
            const input = item.querySelector("input")
            const option = input.dataset.option
            const category = input.dataset.category
            
            if (option)
            {
                item.addEventListener("click", () =>
                {
                    if (option == "orderCategory")
                    {
                        DOM.orderCategoryCheckboxes.forEach(x =>
                        {
                            if (x.dataset.category == category)
                                x.checked = !x.checked
                            else
                                x.checked = false
                        })
                        DOM.orderCategoryCheckboxes.forEach(x =>
                        {
                            if (x.checked)
                                Storage.setOption(option, category)
                        })
                    }
                    else
                    {
                        input.checked = !input.checked
                        Storage.setOption(option, input.checked)
                    }
                })
            }
        })
        
        this.fetchGames()
    }
    
    fetchGames()
    {
        if (this.busy)
            return
        
        this.busy = true
        
        DOM.moreButton.remove()
        
        DOM.eventElement.appendChild(DOM.loading)
        
        DOM.previousLdButton.style.display = Storage.ld == MIN_LD_NUMBER ? "none" : "block"
        DOM.nextLdButton.style.display = Storage.ld == MAX_LD_NUMBER ? "none" : "block"
        DOM.getAll(".ld").forEach(x => x.innerHTML = Storage.ld)
        DOM.getAll(".rate-progress").forEach(x => x.innerHTML = "")
        
        const data = { token: Storage.token }
        const options = Storage.loadOptions()
        for (const key in options)
            data[key] = options[key]
        
        App.post(
            `/api/ld${Storage.ld}/?page=${this.page++}&search=${Storage.search}`,
            data
        )
        .then(response =>
        {
            DOM.loading.remove()
            
            if (!response.ok)
            {
                DOM.logoutButton.style.display = "none"
                DOM.loginButton.style.display = "block"
                DOM.infoElement.innerHTML = "⚠ Unauthorized ⚠"
                DOM.dataElement.appendChild(DOM.infoElement)
                throw new Error
            }
            
            return response.json()
        })
        .then(data =>
        {
            if (data === null)
            {
                DOM.infoElement.innerHTML = "⚠ No data found ⚠"
                DOM.dataElement.appendChild(DOM.infoElement)
                throw new Error
            }
            
            DOM.getAll(".rate-progress").forEach(x =>
            {
                const v = data.rateProgress
                if (v)
                    x.innerHTML = `You rated <b>${v.toFixed(2)}</b> games.`
            })
            
            const games = data.games

            if (games.length % 2 != 0)
                games.push({})
            
            for (const game of games)
            {
                const link = game.static_path || ""
                const thumbnail = game.thumbnail_url || ""
                
                let title = game.name || "Untitled"
                if (title.length > 50)
                    title = title.substr(0, 50) + "..."
                
                const type = game.subsubtype || "unknown"
                const userComments = game.userComments || 0
                
                let element
                const elements = {
                    entry: DOM.create("div"),
                    inner: DOM.create("div"),
                    coverWrapper: DOM.create("a"),
                    cover: DOM.create("div"),
                    title: DOM.create("div"),
                    type: DOM.create("div"),
                    userComments: DOM.create("div"),
                    ratings: DOM.create("div")
                }
                
                element = elements.entry
                element.classList.add("entry")
                if (!link)
                    element.style.visibility = "hidden"
                DOM.dataElement.appendChild(element)
                
                element = elements.inner
                element.classList.add("entry__inner")
                elements.entry.appendChild(element)
                
                element = elements.coverWrapper
                element.onclick = e => this.showGame(game)
                element.classList.add("entry__cover-wrapper")
                elements.inner.appendChild(element)
                
                element = elements.cover
                element.classList.add("entry__cover")
                element.style.backgroundImage = "url(\"" + thumbnail + "\")"
                elements.coverWrapper.appendChild(element)
                
                element = elements.title
                element.classList.add("entry__title")
                element.innerHTML = title
                elements.inner.appendChild(element)
                
                element = elements.type
                element.classList.add("entry__type")
                element.classList.add("entry__type-" + type)
                element.innerHTML = type.toUpperCase()
                elements.inner.appendChild(element)
                
                element = elements.userComments
                element.classList.add("entry__user-comments")
                element.classList.add("entry__user-comments-" + (
                    userComments == 0 ? "zero" : (userComments == 1 ? "one" : "more")
                ))
                elements.inner.appendChild(element)
                
                elements.ratings.classList.add("ratings")
                
                if (game.id)
                {
                    for (const i in App.grades)
                    {
                        let rating = { id: 0, name: null, value: -1 }
                        
                        const grade = App.grades[i]
                        if (!game.opt_outs[i])
                            rating = game.rating[grade.toLowerCase()]
                        
                        element = DOM.create("div")
                        element.classList.add("rating")
                        element.setAttribute("data-id", rating.id)
                        element.setAttribute("data-name", rating.name)
                        element.setAttribute("data-keyword", App.keywords[rating.value].toLowerCase())
                        
                        let html = ""
                        html += "<div class=\"rating__grade\">" + grade + "</div>"
                        html += "<div class=\"rating__value\"><div class=\"value\">" + (rating.value || "&nbsp;") + "</div></div>"
                        html += "<div class=\"rating__keyword\"><div class=\"bar\"></div><div class=\"text\">" + App.keywords[rating.value] + "</div></div>"
                        element.innerHTML = html
                        
                        elements.ratings.appendChild(element)
                    }
                }
                
                elements.inner.appendChild(elements.ratings)
            }
            
            if (games.length == 20)
                DOM.eventElement.appendChild(DOM.moreButton)
            
            this.busy = false
        })
    }
    
    showGame(game)
    {
        this.game = game
        DOM.openGameDialog(game.name, game.static_body_html)
    }
    
    openSubmission()
    {
        window.open(this.game.static_path)
    }
    
    closeGame()
    {
        this.game = null
        DOM.closeGameDialog()
    }
    
    login()
    {
        App.post("/api/login",
        {
            username: DOM.usernameInput.value,
            password: DOM.passwordInput.value
        })
        .then(response =>
        {
            if (!response.ok)
            {
                DOM.shakeDialogBox()
                throw new Error
            }
            return response.text()
        })
        .then(token =>
        {
            Storage.token = token
            location.reload()
        })
    }
    
    logout()
    {
        App.post("/api/logout",
        {
            token: Storage.token
        })
        .then(response =>
        {
            if (!response.ok)
                throw new Error
            
            Storage.removeToken()
            location.reload()
        })
    }
    
    previous()
    {
        if (this.busy)
            return
        
        Storage.ld = Math.max(MIN_LD_NUMBER, Storage.ld - 1)
        this.clearGames()
        this.fetchGames()
    }

    next()
    {
        if (this.busy)
            return
        
        Storage.ld = Math.max(MIN_LD_NUMBER, Storage.ld + 1)
        this.clearGames()
        this.fetchGames()
    }
}

const app = new App

app.run()