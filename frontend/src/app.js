import Const from "./const"
import DOM from "./dom"
import Storage from "./storage"
import UserSettings from "./user-settings"

export default class App
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
    
    async run()
    {
        if (this.running)
            throw new Error
        
        this.running = true
        
        window.addEventListener("scroll", () =>
        {
            if (DOM.moreButton.parentNode)
                if (document.body.offsetHeight - window.innerHeight - window.scrollY < 400)
                    DOM.moreButton.click()
        })
        
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
        
        DOM.optionsButton.addEventListener("click", DOM.openOptionsDialog)
        DOM.optionsDialogButtons[0].addEventListener("click", () => DOM.closeOptionsDialog())
        
        DOM.searchInput.value = await Storage.search
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
                DOM.enableScroll()
                item.parentNode.classList.remove("visible")
            })
        })
        
        DOM.previousLdButton.addEventListener("click", () => this.previous())
        DOM.nextLdButton.addEventListener("click", () => this.next())
        
        const options = await UserSettings.getOptions()
        
        if (!Storage.token)
        {
            DOM.requiresLogin.forEach(item =>
            {
                if (item.classList.contains("but-is-readonly"))
                {
                    item.style.pointerEvents = "none"
                    item.style.opacity = .5
                }
                else
                {
                    item.style.display = "none"
                }
            })
        }
        
        DOM.checkboxes.forEach(item =>
        {
            const option = item.dataset.option
            const category = item.dataset.category
            
            if (option)
            {
                const checked = options[option]
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
                item.addEventListener("click", async () =>
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
                        
                        options[option] = ""
                        
                        DOM.orderCategoryCheckboxes.forEach(x =>
                        {
                            if (x.checked)
                                options[option] = category
                        })
                        
                        await UserSettings.setOptions(this, options)
                    }
                    else
                    {
                        input.checked = !input.checked
                        options[option] = input.checked
                        await UserSettings.setOptions(this, options)
                    }
                })
            }
        })
        
        DOM.moreButton.click()
    }
    
    fetchGames()
    {
        if (this.busy)
            return
        
        this.busy = true
        
        DOM.moreButton.remove()
        
        DOM.bottomElement.appendChild(DOM.loading)
        
        DOM.previousLdButton.style.display = Storage.ld == Const.MIN_LD_NUMBER ? "none" : "block"
        DOM.nextLdButton.style.display = Storage.ld == Const.MAX_LD_NUMBER ? "none" : "block"
        DOM.getAll(".ld").forEach(x => x.innerHTML = Storage.ld)
        DOM.getAll(".rate-progress").forEach(x => x.innerHTML = "")
        
        App.post(
            `/api/ld${Storage.ld}/?page=${this.page++}`, {
                token: Storage.token,
                search: Storage.search
            }
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
            
            if (!data.user && Storage.token)
            {
                this.logout()
                return
            }
            
            if (data.user)
            {
                DOM.optionsButton.style.display = "";
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
            
            for (const i in games)
            {
                const game = games[i]
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
                    favorite: DOM.create("div"),
                    userComments: DOM.create("div"),
                    ratings: DOM.create("div")
                }
                
                element = elements.entry
                element.classList.add("entry")
                if (link)
                {
                    element.style.translate = "0 100px"
                    element.style.opacity = 0
                    element.style.transition = "all 220ms"
                    setTimeout(() =>
                    {
                        elements.entry.style.translate = null
                        elements.entry.style.opacity = null
                    }, i * 60 + 100)
                }
                else
                {
                    element.style.visibility = "hidden"
                }
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
                
                if (data.user)
                {
                    element = elements.favorite
                    element.classList.add("entry__favorite")
                    if (data.user.settings.favoriteGameIds.includes(game.id))
                        element.classList.add("entry__favorite-active")
                    element.addEventListener("click", e =>
                    {
                        e.target.classList.toggle("entry__favorite-active")
                        if (e.target.classList.contains("entry__favorite-active"))
                        {
                            App.post("api/favorite/add", {
                                token: Storage.token,
                                gameId: game.id
                            })
                        }
                        else
                        {
                            App.post("api/favorite/remove", {
                                token: Storage.token,
                                gameId: game.id
                            })
                        }
                    })
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
            }
            
            if (games.length == 20)
                DOM.bottomElement.appendChild(DOM.moreButton)
            
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
        
        Storage.ld = Math.max(Const.MIN_LD_NUMBER, Storage.ld - 1)
        this.clearGames()
        this.fetchGames()
    }

    next()
    {
        if (this.busy)
            return
        
        Storage.ld = Math.max(Const.MIN_LD_NUMBER, Storage.ld + 1)
        this.clearGames()
        this.fetchGames()
    }
}