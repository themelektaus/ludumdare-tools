export default class DOM
{
    static loading = DOM.get(".loading")
    static moreButton = DOM.get(".more")
    
    static noScrollY = 0;
    
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
    
    static get optionsButton()
    {
        return DOM.get("#options")
    }
    
    static get requiresLogin()
    {
        return DOM.getAll(".requires-login")
    }
    
    static get bottomElement()
    {
        return DOM.get(".bottom")
    }
    
    static get dataElement()
    {
        return DOM.get("#data")
    }
    
    static get searchInput()
    {
        return DOM.get("#search")
    }
    
    static get sidebar()
    {
        return DOM.get(".sidebar")
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
    
    static get preferGifs()
    {
        return DOM.get(`input[data-option="preferGifs"]`)
    }
    
    static disableScroll() 
    {
        document.body.classList.add("no-scroll")
    }
    
    static enableScroll()
    {
        document.body.classList.remove("no-scroll")
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