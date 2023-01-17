import App from "./app"
import DOM from "./dom"
import Storage from "./storage"

export default class UserSettings
{
    static #dirtyTimeout = null
    
    static getOptions()
    {
        if (Storage.token)
        {
            return App.post(
                `/api/options/get`, {
                    token: Storage.token
                }
            ).then(x => x.json())
        }
        
        return new Promise(resolve =>
        {
            resolve(Storage.options)
        })
    }
    
    static setOptions(app, value)
    {
        let promise
        
        if (Storage.token)
        {
            promise = App.post(
                `/api/options/set`, {
                    token: Storage.token,
                    preferGifs: value.preferGifs,
                    filterOnlyFavorites: value.filterOnlyFavorites,
                    filterJam: value.filterJam,
                    filterCompo: value.filterCompo,
                    filterRated: value.filterRated,
                    filterUnrated: value.filterUnrated,
                    orderCategory: value.orderCategory
                }
            )
        }
        else
        {
            promise = new Promise(resolve =>
            {
                Storage.options = value
                resolve()
            })
        }
        
        return promise.then(() =>
        {
            DOM.timeout(UserSettings, null, () =>
            {
                if (this.busy)
                    return true
                
                app.clearGames()
                app.fetchGames()
            }, 600)
        })
    }
}